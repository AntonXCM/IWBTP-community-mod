using UnityEditor;
using UnityEngine;

/// <summary>
/// Редактор вентиляции в сцене: кисть для тайлов, выходов и ластик.
/// Файл должен лежать в папке Editor.
/// </summary>
[CustomEditor(typeof(VentilationMesh))]
public class VentilationMeshEditor : Editor
{
    private enum PaintMode { Tile, Exit, Erase }

    private static readonly string[] ModeNames = { "Тайл", "Выход", "Ластик" };

    private static readonly Color TileFill = new Color(0.25f, 0.85f, 0.35f, 0.35f);
    private static readonly Color ExitFill = new Color(1f, 0.6f, 0.1f, 0.35f);
    private static readonly Color ExitLine = new Color(1f, 0.6f, 0.1f, 0.9f);
    private static readonly Color EraseFill = new Color(1f, 0.25f, 0.25f, 0.35f);
    private static readonly Color GridColor = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color TileLine = new Color(1f, 1f, 1f, 0.35f);

    // Состояние редактора общее для всех инспекторов и переживает смену выделения.
    private static bool editing;
    private static PaintMode mode = PaintMode.Tile;
    private static Tool previousTool = Tool.Move;
    private static int dragUndoGroup;

    // Предпросмотр: true — освещение видно как настроено, false — временно скрыто, чтобы видеть
    // всю вентиляцию при рисовании. В сцену не сохраняется и настройку vertexLighting не трогает.
    private static bool previewLighting = true;

    private readonly Vector3[] corners = new Vector3[4];
    private Rect toolbarRect = new Rect(10f, 10f, 350f, 54f);
    private Vector2Int? lastPaintedCell;
    private bool hasHover;
    private Vector2Int hoverCell;

    private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;

        // Предпросмотр действует, только пока объект выделен. Потом освещение снова работает как настроено.
        var vm = target as VentilationMesh;
        if (vm != null) vm.ShowLighting = true;
    }

    // После Undo/Redo восстановились сериализованные списки — пересобираем меш и коллайдеры.
    private void OnUndoRedo()
    {
        var vm = target as VentilationMesh;
        if (vm == null) return;
        vm.Rebuild();
        SceneView.RepaintAll();
    }

    // ───────────────────────── Инспектор ─────────────────────────

    public override void OnInspectorGUI()
    {
        var vm = (VentilationMesh)target;

        EditorGUILayout.LabelField("Редактор", EditorStyles.boldLabel);

        bool wantEditing = GUILayout.Toggle(editing,
            editing ? "Редактирование включено" : "Редактировать в сцене", "Button", GUILayout.Height(24f));
        if (wantEditing != editing) SetEditing(wantEditing);

        using (new EditorGUI.DisabledScope(!editing))
        {
            mode = (PaintMode)GUILayout.Toolbar((int)mode, ModeNames);
        }

        using (new EditorGUI.DisabledScope(!vm.VertexLighting))
        {
            previewLighting = EditorGUILayout.ToggleLeft(
                "Показывать вертексное освещение в сцене (только предпросмотр)", previewLighting);
        }
        vm.ShowLighting = previewLighting;

        EditorGUILayout.HelpBox(
            "ЛКМ — рисовать (можно тянуть), Shift+ЛКМ — стереть.\n" +
            "Выход — клетка без меша и коллайдера: соседние тайлы соединяются с ней, а в контуре остаётся проём.",
            MessageType.None);

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck()) vm.Rebuild();

        if (!vm.InsetsFitTile)
        {
            EditorGUILayout.HelpBox(
                "Сумма сдвигов верх+низ или лево+право не меньше размера тайла: узкие проходы схлопнутся.",
                MessageType.Warning);
        }

        if (vm.VertexLighting && vm.LightSource == null)
        {
            EditorGUILayout.HelpBox(
                "Вертексное освещение включено, но Transform источника не задан — всё остаётся видимым.",
                MessageType.Warning);
        }

        if (vm.PlaneKind != VentilationMesh.PlaneMode.XY)
        {
            EditorGUILayout.HelpBox("EdgeCollider2D строятся только в плоскости XY.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Тайлов: {vm.Tiles.Count}    Выходов: {vm.Exits.Count}");

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Перестроить"))
            {
                vm.Rebuild();
            }

            if (GUILayout.Button("Очистить всё"))
            {
                if (EditorUtility.DisplayDialog("Очистить вентиляцию", "Удалить все тайлы и выходы?",
                        "Удалить", "Отмена"))
                {
                    Undo.RegisterCompleteObjectUndo(vm, "Clear ventilation");
                    vm.Clear();
                    MarkChanged(vm);
                }
            }
        }
    }

    // ───────────────────────── Сцена ─────────────────────────

    private void OnSceneGUI()
    {
        var vm = (VentilationMesh)target;
        Event e = Event.current;

        DrawSceneToolbar(vm);
        vm.ShowLighting = previewLighting; // только предпросмотр, настройки сцены не меняются

        // Выбрали другой инструмент (Move, Rotate...) — выходим из режима рисования.
        if (editing && Tools.current != Tool.None && Tools.current != Tool.View) editing = false;
        bool canPaint = editing && Tools.current == Tool.None;

        hasHover = false;
        if (canPaint && !toolbarRect.Contains(e.mousePosition) && TryGetCell(vm, e.mousePosition, out Vector2Int cell))
        {
            hasHover = true;
            hoverCell = cell;
        }

        DrawOverlay(vm, e);
        DrawLightRadii(vm, e);

        if (!canPaint) return;

        int id = GUIUtility.GetControlID(FocusType.Passive);
        switch (e.GetTypeForControl(id))
        {
            case EventType.Layout:
                // Не даём клику по сцене сменить выделение.
                HandleUtility.AddDefaultControl(id);
                break;

            case EventType.MouseDown:
                if (e.button == 0 && !e.alt && hasHover)
                {
                    GUIUtility.hotControl = id;
                    lastPaintedCell = null;
                    Undo.IncrementCurrentGroup();
                    dragUndoGroup = Undo.GetCurrentGroup();
                    Paint(vm, hoverCell, e.shift);
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    if (hasHover) Paint(vm, hoverCell, e.shift);
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    lastPaintedCell = null;
                    e.Use();
                }
                break;

            case EventType.MouseMove:
                SceneView.RepaintAll(); // чтобы подсветка клетки следовала за курсором
                break;
        }
    }

    private void DrawSceneToolbar(VentilationMesh vm)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(toolbarRect, EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        bool wantEditing = GUILayout.Toggle(editing, "Редактор", EditorStyles.miniButton, GUILayout.Width(80f));
        if (wantEditing != editing) SetEditing(wantEditing);

        using (new EditorGUI.DisabledScope(!editing))
        {
            mode = (PaintMode)GUILayout.Toolbar((int)mode, ModeNames, EditorStyles.miniButton);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(!vm.VertexLighting))
        {
            previewLighting = GUILayout.Toggle(previewLighting, "Освещение", EditorStyles.miniButton,
                GUILayout.Width(80f));
        }
        GUILayout.Label(editing ? "ЛКМ — рисовать, Shift+ЛКМ — стереть" : "Включите редактор, чтобы рисовать",
            EditorStyles.miniLabel);
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private static void SetEditing(bool value)
    {
        if (value == editing) return;
        editing = value;

        // На время рисования прячем гизмо перемещения, иначе оно перехватывает клики.
        if (editing)
        {
            previousTool = Tools.current;
            Tools.current = Tool.None;
        }
        else
        {
            Tools.current = previousTool;
        }

        SceneView.RepaintAll();
    }

    private static bool TryGetCell(VentilationMesh vm, Vector2 mousePosition, out Vector2Int cell)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        Plane plane = vm.GetWorldPlane();

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 world = ray.GetPoint(distance);
            cell = vm.LocalToCell(vm.transform.InverseTransformPoint(world));
            return true;
        }

        cell = default;
        return false;
    }

    // ───────────────────────── Рисование ─────────────────────────

    private void Paint(VentilationMesh vm, Vector2Int cell, bool forceErase)
    {
        // Одну и ту же клетку за одно перетаскивание обрабатываем один раз.
        if (lastPaintedCell.HasValue && lastPaintedCell.Value == cell) return;
        lastPaintedCell = cell;

        PaintMode m = forceErase ? PaintMode.Erase : mode;
        if (!WouldChange(vm, cell, m)) return;

        Undo.RegisterCompleteObjectUndo(vm, "Paint ventilation");

        switch (m)
        {
            case PaintMode.Tile: vm.AddTile(cell); break;
            case PaintMode.Exit: vm.AddExit(cell); break;
            default: vm.Erase(cell); break;
        }

        // Весь мазок кисти — один шаг в истории Undo.
        Undo.CollapseUndoOperations(dragUndoGroup);
        MarkChanged(vm);
    }

    private static bool WouldChange(VentilationMesh vm, Vector2Int cell, PaintMode m)
    {
        switch (m)
        {
            case PaintMode.Tile: return !vm.HasTile(cell);
            case PaintMode.Exit: return !vm.HasExit(cell);
            default: return vm.HasTile(cell) || vm.HasExit(cell);
        }
    }

    private static void MarkChanged(VentilationMesh vm)
    {
        EditorUtility.SetDirty(vm);
        PrefabUtility.RecordPrefabInstancePropertyModifications(vm);
        SceneView.RepaintAll();
    }

    // ───────────────────────── Отрисовка в сцене ─────────────────────────

    private void DrawOverlay(VentilationMesh vm, Event e)
    {
        if (e.type != EventType.Repaint) return;

        Matrix4x4 oldMatrix = Handles.matrix;
        Color oldColor = Handles.color;
        Handles.matrix = vm.transform.localToWorldMatrix;

        if (editing) DrawGridAndTiles(vm);

        // Выходы не имеют меша, поэтому показываем их всегда, пока объект выделен.
        foreach (Vector2Int exit in vm.Exits)
        {
            vm.GetCellCornersLocal(exit, corners);
            Handles.DrawSolidRectangleWithOutline(corners, ExitFill, ExitLine);
            Handles.color = ExitLine;
            Handles.DrawLine(corners[0], corners[2]);
            Handles.DrawLine(corners[1], corners[3]);
        }

        if (hasHover)
        {
            Color fill = e.shift || mode == PaintMode.Erase ? EraseFill
                       : mode == PaintMode.Exit ? ExitFill
                       : TileFill;

            vm.GetCellCornersLocal(hoverCell, corners);
            Handles.DrawSolidRectangleWithOutline(corners, fill, new Color(fill.r, fill.g, fill.b, 1f));
        }

        Handles.matrix = oldMatrix;
        Handles.color = oldColor;
    }

    // Радиусы освещения вокруг источника: жёлтый — полная видимость, оранжевый — полная прозрачность.
    // Рисуется в плоскости меша, потому что расстояние считается именно в ней.
    private static void DrawLightRadii(VentilationMesh vm, Event e)
    {
        if (e.type != EventType.Repaint || !vm.VertexLighting || vm.LightSource == null) return;

        Plane plane = vm.GetWorldPlane();
        Vector3 center = plane.ClosestPointOnPlane(vm.LightSource.position);

        Color oldColor = Handles.color;
        Handles.color = new Color(1f, 0.92f, 0.3f, 0.9f);
        Handles.DrawWireDisc(center, plane.normal, vm.LightInnerRadius);
        Handles.color = new Color(1f, 0.55f, 0.1f, 0.9f);
        Handles.DrawWireDisc(center, plane.normal, vm.LightOuterRadius);
        Handles.color = oldColor;
    }

    private void DrawGridAndTiles(VentilationMesh vm)
    {
        // Область сетки: охватывает все клетки и курсор, плюс запас по краям.
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

        foreach (Vector2Int t in vm.Tiles)
        {
            min = Vector2Int.Min(min, t);
            max = Vector2Int.Max(max, t);
        }
        foreach (Vector2Int x in vm.Exits)
        {
            min = Vector2Int.Min(min, x);
            max = Vector2Int.Max(max, x);
        }
        if (hasHover)
        {
            min = Vector2Int.Min(min, hoverCell);
            max = Vector2Int.Max(max, hoverCell);
        }
        if (min.x > max.x)
        {
            min = new Vector2Int(-4, -4);
            max = new Vector2Int(3, 3);
        }

        min -= Vector2Int.one * 2;
        max += Vector2Int.one * 2;

        Handles.color = GridColor;
        for (int x = min.x; x <= max.x + 1; x++)
        {
            Handles.DrawLine(vm.GridPointToLocal(x, min.y), vm.GridPointToLocal(x, max.y + 1));
        }
        for (int y = min.y; y <= max.y + 1; y++)
        {
            Handles.DrawLine(vm.GridPointToLocal(min.x, y), vm.GridPointToLocal(max.x + 1, y));
        }

        foreach (Vector2Int t in vm.Tiles)
        {
            vm.GetCellCornersLocal(t, corners);
            Handles.DrawSolidRectangleWithOutline(corners, Color.clear, TileLine);
        }
    }
}