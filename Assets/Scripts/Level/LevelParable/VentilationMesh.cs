using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Хранит позиции тайлов вентиляции и строит из них меш и EdgeCollider2D.
///
/// Атлас — сетка субячеек 4x4, индексы (строка 0 — верх картинки):
///   0 1 2 3
///   4 5 6 7
///   8 9 a b
///   c d e f
/// Он состоит из 4 текстур по 2x2 субячейки:
///   блок 0 (0,1,4,5)  — соединение со всех сторон   (левый верх)
///   блок 1 (2,3,6,7)  — соединение по горизонтали   (правый верх)
///   блок 2 (8,9,c,d)  — соединение по вертикали     (левый низ)
///   блок 3 (a,b,e,f)  — без соединений              (правый низ)
///
/// Каждый тайл = 4 четверти (TL, TR, BL, BR). Для четверти смотрим двух соседей
/// с её стороны (по горизонтали и по вертикали), выбираем блок и берём из него
/// субячейку того же угла. Диагонали не учитываются.
///
/// Выход — клетка без меша и коллайдера. Соседние тайлы считают её соединением
/// (текстуры), а в контуре коллайдера на этой стороне остаётся проём.
///
/// Вертексное освещение (опционально): прозрачность вершин зависит от расстояния
/// до заданного Transform — вблизи меш виден, вдали плавно исчезает.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VentilationMesh : MonoBehaviour, ISerializationCallbackReceiver
{
    public enum PlaneMode { XY, XZ }

    [Header("Сетка")]
    [SerializeField, Min(0.01f)] private float tileSize = 1f;
    [Tooltip("XY — для 2D-сцены (виден с -Z), XZ — лежит на земле (виден сверху). Коллайдеры строятся только в XY.")]
    [SerializeField] private PlaneMode plane = PlaneMode.XY;
    [Tooltip("Y растёт вниз, как строки в таблице: клетка (0,0) — левый верхний угол сетки")]
    [SerializeField] private bool yGrowsDown = true;

    [Header("Атлас")]
    [Tooltip("Отступ внутри субячейки (доля от её размера) — защита от протекания соседних текстур")]
    [SerializeField, Range(0f, 0.1f)] private float uvInset = 0f;

    [Header("Коллайдер (EdgeCollider2D)")]
    [Tooltip("Строить EdgeCollider2D по краям тайлов. Включено — все EdgeCollider2D на этом объекте управляются скриптом.")]
    [SerializeField] private bool generateCollider = true;
    [Tooltip("Пикселей в одном тайле: 1 пиксель = tileSize / pixelsPerTile")]
    [SerializeField, Min(1)] private int pixelsPerTile = 16;
    [Tooltip("Насколько грань сдвигается внутрь тайла, в пикселях")]
    [SerializeField, Min(0)] private int insetTop = 1;
    [SerializeField, Min(0)] private int insetBottom = 4;
    [SerializeField, Min(0)] private int insetRight = 2;
    [SerializeField, Min(0)] private int insetLeft = 2;

    [Header("Вертексное освещение")]
    [Tooltip("Прозрачность вершин зависит от расстояния до Transform. Нужен материал с альфа-смешиванием, " +
             "который учитывает цвет вершин (Sprites/Default, Sprite-Unlit-Default в URP и т. п.).")]
    [SerializeField] private bool vertexLighting = false;
    [Tooltip("От этого Transform считается расстояние. Измеряется в плоскости меша: глубина (Z для XY) игнорируется. " +
             "Если не задан — освещение не применяется, всё видно.")]
    [SerializeField] private Transform lightSource;
    [Tooltip("До этого расстояния вершины полностью видимы (мировые единицы)")]
    [SerializeField, Min(0f)] private float lightInnerRadius = 2f;
    [Tooltip("Дальше этого расстояния вершины полностью прозрачны (мировые единицы)")]
    [SerializeField, Min(0f)] private float lightOuterRadius = 6f;
    [Tooltip("Сглаженный переход (smoothstep) вместо линейного")]
    [SerializeField] private bool smoothFalloff = true;

    // Сохраняемые данные. Скрыты из инспектора: редактируются кистью в сцене.
    [HideInInspector, SerializeField] private List<Vector2Int> savedTiles = new List<Vector2Int>();
    [HideInInspector, SerializeField] private List<Vector2Int> savedExits = new List<Vector2Int>();

    private const int AtlasCells = 4; // атлас 4x4 субячейки

    private readonly HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> exits = new HashSet<Vector2Int>();
    private Mesh mesh;
    private bool dirty;

    // Кэш последнего расчёта освещения: пересчитываем цвета, только если что-то сдвинулось.
    private bool lightingStale = true;
    private bool lastHadSource;
    private Vector2 lastSource;
    private float lastScale;

    // Буферы переиспользуются между перестроениями, чтобы не мусорить в GC.
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector3> normals = new List<Vector3>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Color32> colors = new List<Color32>();

    // ───────────────────────── Публичный API ─────────────────────────

    public IReadOnlyCollection<Vector2Int> Tiles => tiles;
    public IReadOnlyCollection<Vector2Int> Exits => exits;
    public PlaneMode PlaneKind => plane;

    /// <summary>Включает/выключает вертексное освещение. При выключении цвета вершин убираются.</summary>
    public bool VertexLighting
    {
        get => vertexLighting;
        set
        {
            if (vertexLighting == value) return;
            vertexLighting = value;
            Rebuild();
        }
    }

    /// <summary>Transform, от которого считается расстояние (например, игрок или фонарик).</summary>
    public Transform LightSource
    {
        get => lightSource;
        set
        {
            lightSource = value;
            lightingStale = true;
        }
    }

    public float LightInnerRadius => lightInnerRadius;
    public float LightOuterRadius => lightOuterRadius;

    /// <summary>Меняет радиусы освещения «на лету» (мировые единицы).</summary>
    public void SetLightRadii(float inner, float outer)
    {
        lightInnerRadius = Mathf.Max(0f, inner);
        lightOuterRadius = Mathf.Max(lightInnerRadius, outer);
        lightingStale = true;
    }

    /// <summary>Не схлопнутся ли самые узкие проходы (1 тайл) из-за сдвигов.</summary>
    public bool InsetsFitTile =>
        insetTop + insetBottom < pixelsPerTile && insetLeft + insetRight < pixelsPerTile;

    public bool HasTile(Vector2Int pos) => tiles.Contains(pos);
    public bool HasExit(Vector2Int pos) => exits.Contains(pos);

    /// <summary>Ставит тайл. Если в клетке был выход — он заменяется.</summary>
    public bool AddTile(Vector2Int pos, bool rebuild = true)
    {
        bool changed = exits.Remove(pos);
        changed |= tiles.Add(pos);
        if (changed && rebuild) Rebuild();
        return changed;
    }

    /// <summary>Ставит выход. Если в клетке был тайл — он заменяется.</summary>
    public bool AddExit(Vector2Int pos, bool rebuild = true)
    {
        bool changed = tiles.Remove(pos);
        changed |= exits.Add(pos);
        if (changed && rebuild) Rebuild();
        return changed;
    }

    public bool RemoveTile(Vector2Int pos, bool rebuild = true)
    {
        if (!tiles.Remove(pos)) return false;
        if (rebuild) Rebuild();
        return true;
    }

    public bool RemoveExit(Vector2Int pos, bool rebuild = true)
    {
        if (!exits.Remove(pos)) return false;
        if (rebuild) Rebuild();
        return true;
    }

    /// <summary>Убирает из клетки и тайл, и выход.</summary>
    public bool Erase(Vector2Int pos, bool rebuild = true)
    {
        bool changed = tiles.Remove(pos);
        changed |= exits.Remove(pos);
        if (changed && rebuild) Rebuild();
        return changed;
    }

    /// <summary>Заменяет все тайлы и перестраивает всё один раз.</summary>
    public void SetTiles(IEnumerable<Vector2Int> positions)
    {
        var copy = new List<Vector2Int>(positions);
        tiles.Clear();
        foreach (Vector2Int p in copy) tiles.Add(p);
        exits.ExceptWith(tiles);
        Rebuild();
    }

    /// <summary>Заменяет все выходы и перестраивает всё один раз.</summary>
    public void SetExits(IEnumerable<Vector2Int> positions)
    {
        var copy = new List<Vector2Int>(positions);
        exits.Clear();
        foreach (Vector2Int p in copy) exits.Add(p);
        tiles.ExceptWith(exits);
        Rebuild();
    }

    public void Clear()
    {
        tiles.Clear();
        exits.Clear();
        Rebuild();
    }

    /// <summary>
    /// Индексы субячеек атласа для тайла в порядке TL, TR, BL, BR (hex-строкой).
    /// Например "2345" или "2b6f". Null, если тайла нет. Удобно для отладки.
    /// </summary>
    public string GetTileCode(Vector2Int pos)
    {
        if (!tiles.Contains(pos)) return null;
        GetNeighbors(pos, out bool l, out bool r, out bool u, out bool d);
        return QuarterCell(0, 0, l, u).ToString("x")
             + QuarterCell(1, 0, r, u).ToString("x")
             + QuarterCell(0, 1, l, d).ToString("x")
             + QuarterCell(1, 1, r, d).ToString("x");
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        dirty = false;
        RebuildMesh();
        RebuildColliders();
    }

    // ── Помощники для редактора: перевод между сеткой и пространством объекта ──

    /// <summary>Плоскость сетки в мировых координатах (для луча из курсора).</summary>
    public Plane GetWorldPlane()
    {
        Vector3 normal = plane == PlaneMode.XY ? transform.forward : transform.up;
        return new Plane(normal, transform.position);
    }

    /// <summary>Клетка сетки, в которую попадает точка в локальных координатах объекта.</summary>
    public Vector2Int LocalToCell(Vector3 local)
    {
        float a = local.x / tileSize;
        float b = (plane == PlaneMode.XY ? local.y : local.z) / tileSize;
        return FlipY(new Vector2Int(Mathf.FloorToInt(a), Mathf.FloorToInt(b)));
    }

    /// <summary>
    /// Локальная позиция узла сетки (x, y): это угол клетки (x, y) с минимальными индексами.
    /// Клетка (x, y) лежит между узлами (x, y) и (x+1, y+1).
    /// </summary>
    public Vector3 GridPointToLocal(int x, int y)
    {
        float py = yGrowsDown ? -y : y;
        return ToLocal(x * tileSize, py * tileSize);
    }

    /// <summary>Четыре угла клетки в локальных координатах (по кругу).</summary>
    public void GetCellCornersLocal(Vector2Int cell, Vector3[] corners)
    {
        corners[0] = GridPointToLocal(cell.x, cell.y);
        corners[1] = GridPointToLocal(cell.x + 1, cell.y);
        corners[2] = GridPointToLocal(cell.x + 1, cell.y + 1);
        corners[3] = GridPointToLocal(cell.x, cell.y + 1);
    }

    [ContextMenu("Заполнить примером (T-фигура)")]
    private void FillTShapeExample()
    {
        // Таблица:  0 0 0
        //           1 1 1
        //           0 1 0
        SetTiles(new[]
        {
            new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
            new Vector2Int(1, 2),
        });

        // При включённом yGrowsDown ожидается: (1,1) -> 2345, (2,1) -> 2b6f
        Debug.Log($"(1,1) -> {GetTileCode(new Vector2Int(1, 1))}, (2,1) -> {GetTileCode(new Vector2Int(2, 1))}", this);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // ───────────────────────── Жизненный цикл ─────────────────────────

    private void OnEnable() => Rebuild();

    private void OnDisable() => ReleaseMesh();

    // Срабатывает при правках в инспекторе. Тяжёлую работу здесь делать нельзя — только флаг.
    private void OnValidate()
    {
        if (lightOuterRadius < lightInnerRadius) lightOuterRadius = lightInnerRadius;
        dirty = true;
    }

    private void Update()
    {
        if (dirty) Rebuild();
    }

    // После Update, чтобы цвета учитывали уже сдвинутый в этом кадре источник.
    private void LateUpdate()
    {
        if (vertexLighting) UpdateLighting();
    }

    public void OnBeforeSerialize()
    {
        savedTiles.Clear();
        savedTiles.AddRange(tiles);
        savedTiles.Sort(CompareCells); // стабильный порядок — меньше шума в диффах сцены

        savedExits.Clear();
        savedExits.AddRange(exits);
        savedExits.Sort(CompareCells);
    }

    public void OnAfterDeserialize()
    {
        tiles.Clear();
        foreach (Vector2Int t in savedTiles) tiles.Add(t);

        exits.Clear();
        foreach (Vector2Int e in savedExits) exits.Add(e);
        exits.ExceptWith(tiles); // клетка не может быть одновременно тайлом и выходом
    }

    private static int CompareCells(Vector2Int a, Vector2Int b)
    {
        return a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x);
    }

    private void ReleaseMesh()
    {
        var filter = GetComponent<MeshFilter>();
        if (filter != null) filter.sharedMesh = null;

        if (mesh != null)
        {
            SafeDestroy(mesh);
            mesh = null;
        }
    }

    private static void SafeDestroy(Object obj)
    {
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    // ───────────────────────── Координаты ─────────────────────────

    // Переводит между координатами сетки и «Y вверх». Преобразование симметрично:
    // применённое дважды, возвращает исходную клетку.
    private Vector2Int FlipY(Vector2Int c)
    {
        return yGrowsDown ? new Vector2Int(c.x, -c.y - 1) : c;
    }

    private Vector3 ToLocal(float x, float y)
    {
        return plane == PlaneMode.XY ? new Vector3(x, y, 0f) : new Vector3(x, 0f, y);
    }

    // ───────────────────────── Соседи ─────────────────────────

    // Соединиться можно и с тайлом, и с выходом.
    private bool IsOpen(Vector2Int c) => tiles.Contains(c) || exits.Contains(c);

    private void GetNeighbors(Vector2Int t, out bool left, out bool right, out bool top, out bool bottom)
    {
        Vector2Int up = yGrowsDown ? Vector2Int.down : Vector2Int.up;
        left = IsOpen(t + Vector2Int.left);
        right = IsOpen(t + Vector2Int.right);
        top = IsOpen(t + up);
        bottom = IsOpen(t - up);
    }

    /// <summary>
    /// Индекс субячейки атласа для четверти тайла.
    /// qx: 0 — левая, 1 — правая; qy: 0 — верхняя, 1 — нижняя.
    /// h / v — есть ли сосед с горизонтальной / вертикальной стороны этой четверти.
    /// </summary>
    private static int QuarterCell(int qx, int qy, bool h, bool v)
    {
        int block = h && v ? 0 : h ? 1 : v ? 2 : 3;
        int col = (block % 2) * 2 + qx;
        int row = (block / 2) * 2 + qy;
        return row * AtlasCells + col;
    }

    // ───────────────────────── Меш ─────────────────────────

    private void RebuildMesh()
    {
        var filter = GetComponent<MeshFilter>();
        if (mesh == null)
        {
            mesh = new Mesh { name = "VentilationMesh", hideFlags = HideFlags.DontSave };
        }

        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        triangles.Clear();

        foreach (Vector2Int t in tiles) BuildTile(t);

        if (vertexLighting) mesh.MarkDynamic(); // цвета вершин будут обновляться часто

        mesh.Clear(); // заодно убирает цвета вершин, если освещение выключено
        mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        if (vertexLighting)
        {
            lightingStale = true;
            UpdateLighting();
        }

        filter.sharedMesh = mesh;
    }

    private void BuildTile(Vector2Int t)
    {
        GetNeighbors(t, out bool left, out bool right, out bool top, out bool bottom);

        Vector2Int up = FlipY(t);
        Vector2 origin = new Vector2(up.x, up.y) * tileSize; // левый нижний угол тайла
        float half = tileSize * 0.5f;

        for (int qy = 0; qy < 2; qy++)
        {
            for (int qx = 0; qx < 2; qx++)
            {
                bool h = qx == 0 ? left : right;
                bool v = qy == 0 ? top : bottom;
                int cell = QuarterCell(qx, qy, h, v);

                // qy == 0 — верхняя половина тайла
                Vector2 min = origin + new Vector2(qx * half, (1 - qy) * half);
                AddQuad(min, min + new Vector2(half, half), cell);
            }
        }
    }

    private void AddQuad(Vector2 min, Vector2 max, int cell)
    {
        int i = vertices.Count;

        vertices.Add(ToLocal(min.x, min.y)); // левый нижний
        vertices.Add(ToLocal(min.x, max.y)); // левый верхний
        vertices.Add(ToLocal(max.x, max.y)); // правый верхний
        vertices.Add(ToLocal(max.x, min.y)); // правый нижний

        Vector3 n = plane == PlaneMode.XY ? Vector3.back : Vector3.up;
        for (int k = 0; k < 4; k++) normals.Add(n);

        // Строка 0 атласа — верх картинки, а UV.v растёт вверх.
        float step = 1f / AtlasCells;
        int col = cell % AtlasCells;
        int row = cell / AtlasCells;
        float pad = uvInset * step;
        float u0 = col * step + pad;
        float u1 = (col + 1) * step - pad;
        float v0 = 1f - (row + 1) * step + pad;
        float v1 = 1f - row * step - pad;

        uvs.Add(new Vector2(u0, v0));
        uvs.Add(new Vector2(u0, v1));
        uvs.Add(new Vector2(u1, v1));
        uvs.Add(new Vector2(u1, v0));

        triangles.Add(i);     triangles.Add(i + 1); triangles.Add(i + 2);
        triangles.Add(i);     triangles.Add(i + 2); triangles.Add(i + 3);
    }

    // ───────────────────────── Вертексное освещение ─────────────────────────

    // Пишет в цвет каждой вершины альфу по расстоянию до lightSource (RGB остаётся белым).
    // Расстояние считается в локальной плоскости меша и переводится в мировые единицы
    // через масштаб объекта (ожидается равномерный масштаб).
    private void UpdateLighting()
    {
        if (mesh == null || vertices.Count == 0) return;

        bool hasSource = lightSource != null;
        Vector2 src = Vector2.zero;
        if (hasSource)
        {
            Vector3 l = transform.InverseTransformPoint(lightSource.position);
            src = plane == PlaneMode.XY ? new Vector2(l.x, l.y) : new Vector2(l.x, l.z);
        }

        Vector3 s = transform.lossyScale;
        float scale = plane == PlaneMode.XY
            ? (Mathf.Abs(s.x) + Mathf.Abs(s.y)) * 0.5f
            : (Mathf.Abs(s.x) + Mathf.Abs(s.z)) * 0.5f;
        scale = Mathf.Max(scale, 1e-6f);

        // Источник и масштаб не менялись — меш не трогаем.
        if (!lightingStale && hasSource == lastHadSource && src == lastSource && Mathf.Approximately(scale, lastScale))
            return;

        lightingStale = false;
        lastHadSource = hasSource;
        lastSource = src;
        lastScale = scale;

        float inner = lightInnerRadius / scale;
        float outer = lightOuterRadius / scale;
        float inner2 = inner * inner;
        float outer2 = outer * outer;
        bool xy = plane == PlaneMode.XY;

        colors.Clear();
        for (int i = 0; i < vertices.Count; i++)
        {
            byte alpha = 255; // без источника всё остаётся видимым
            if (hasSource)
            {
                Vector3 v = vertices[i];
                float dx = v.x - src.x;
                float dy = (xy ? v.y : v.z) - src.y;
                alpha = AlphaFor(dx * dx + dy * dy, inner, outer, inner2, outer2);
            }
            colors.Add(new Color32(255, 255, 255, alpha));
        }

        mesh.SetColors(colors);
    }

    // 255 внутри inner, 0 за outer, между ними — плавный переход.
    private byte AlphaFor(float dist2, float inner, float outer, float inner2, float outer2)
    {
        if (dist2 <= inner2) return 255;
        if (dist2 >= outer2) return 0;

        float t = Mathf.Clamp01((outer - Mathf.Sqrt(dist2)) / (outer - inner)); // 1 у inner, 0 у outer
        if (smoothFalloff) t = t * t * (3f - 2f * t);
        return (byte)Mathf.RoundToInt(t * 255f);
    }

    // ───────────────────────── Коллайдеры ─────────────────────────

    private void RebuildColliders()
    {
        // EdgeCollider2D существует только в плоскости XY.
        if (!generateCollider || plane != PlaneMode.XY) return;

        List<Vector2[]> chains = BuildChains();
        EdgeCollider2D[] existing = GetComponents<EdgeCollider2D>();

        // Один EdgeCollider2D = одна цепочка. Лишние компоненты переиспользуем или удаляем.
        for (int i = 0; i < chains.Count; i++)
        {
            EdgeCollider2D col = i < existing.Length ? existing[i] : gameObject.AddComponent<EdgeCollider2D>();
            if (!SamePoints(col.points, chains[i])) col.points = chains[i];
        }

        for (int i = chains.Count; i < existing.Length; i++) SafeDestroy(existing[i]);
    }

    private List<Vector2[]> BuildChains()
    {
        // Контуры считаются в координатах «Y вверх».
        var solid = new HashSet<Vector2Int>();
        foreach (Vector2Int t in tiles) solid.Add(FlipY(t));

        var open = new HashSet<Vector2Int>(solid);
        foreach (Vector2Int e in exits) open.Add(FlipY(e));

        float pixel = tileSize / pixelsPerTile;
        var insets = new VentilationOutline.Insets
        {
            top = insetTop * pixel,
            bottom = insetBottom * pixel,
            left = insetLeft * pixel,
            right = insetRight * pixel,
        };

        return VentilationOutline.Build(solid, open, tileSize, insets);
    }

    // Не трогаем коллайдер, если точки не изменились: иначе сцена помечается изменённой при каждой загрузке.
    private static bool SamePoints(Vector2[] a, Vector2[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if ((a[i] - b[i]).sqrMagnitude > 1e-10f) return false;
        }
        return true;
    }
}
