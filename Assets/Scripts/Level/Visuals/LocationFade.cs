using System;
using UnityEngine;

/// <summary>
/// Плавно скрывает или проявляет массив SpriteRenderer через альфу (SmoothStep),
/// когда объект света находится внутри одной из заданных областей.
/// Физика не используется: проверка нахождения — чистая математика.
/// </summary>
[DisallowMultipleComponent]
public class LocationFade : MonoBehaviour
{
    public enum FadeMode
    {
        [Tooltip("Спрайты видны всегда, а когда свет в области — плавно скрываются")]
        HideWhenLightInside,
        [Tooltip("Спрайты скрыты всегда, а когда свет в области — плавно проявляются")]
        RevealWhenLightInside
    }

    public enum ZoneShape
    {
        Box,
        Circle
    }

    [Serializable]
    public class Zone
    {
        public string name = "Zone";
        public ZoneShape shape = ZoneShape.Box;

        [Tooltip("Центр области (в локальных координатах якоря)")]
        public Vector3 center = Vector3.zero;

        [Tooltip("Размер для Box (в локальных координатах якоря)")]
        public Vector3 size = new Vector3(3f, 3f, 3f);

        [Tooltip("Радиус для Circle (в локальных координатах якоря)")]
        [Min(0f)] public float radius = 1.5f;

        public Color gizmoColor = new Color(1f, 0.85f, 0.2f, 1f);
        public bool enabled = true;
    }

    [Header("Объекты")]
    [Tooltip("Объект света, положение которого проверяется")]
    [SerializeField] private Transform lightObject;

    [Tooltip("Спрайты, которыми управляем")]
    [SerializeField] private SpriteRenderer[] sprites = Array.Empty<SpriteRenderer>();
    [SerializeField] private SpriteMask[] masks = new SpriteMask[0];

    [Header("Области")]
    [Tooltip("Относительно чего заданы области. Если пусто — используется этот объект.")]
    [SerializeField] private Transform zonesAnchor;

    [Tooltip("Игнорировать ось Z (удобно для 2D: проверка идёт только по X и Y)")]
    [SerializeField] private bool ignoreZ = true;

    [SerializeField] private Zone[] zones = new Zone[] { new Zone() };

    [Header("Поведение")]
    [SerializeField] private FadeMode mode = FadeMode.HideWhenLightInside;

    [Tooltip("Время полного перехода (сек)")]
    [SerializeField, Min(0.0001f)] private float fadeDuration = 0.5f;

    [Tooltip("Множитель альфы в скрытом состоянии (0 — полностью прозрачно)")]
    [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0f;

    [Tooltip("Множитель альфы в видимом состоянии (1 — исходная альфа спрайта)")]
    [SerializeField, Range(0f, 1f)] private float visibleAlpha = 1f;

    [Tooltip("Отключать SpriteRenderer, когда он полностью скрыт (экономит отрисовку)")]
    [SerializeField] private bool disableRendererWhenHidden = false;

    // Исходная альфа каждого спрайта (чтобы не перетирать художественную прозрачность)
    private float[] baseAlpha;

    // 0 — состояние "свет снаружи", 1 — состояние "свет внутри"
    private float progress;

    /// <summary>Находится ли свет в данный момент внутри какой-либо области.</summary>
    public bool IsLightInside { get; private set; }

    private void Awake()
    {
        CacheBaseAlpha();
    }

    private void OnEnable()
    {
        if (baseAlpha == null || baseAlpha.Length != sprites.Length)
            CacheBaseAlpha();

        // Мгновенно выставляем стартовое состояние без плавного перехода
        IsLightInside = EvaluateLightInside();
        progress = IsLightInside ? 1f : 0f;
        ApplyAlpha();
    }

    private void OnDisable()
    {
        // Возвращаем исходную прозрачность
        if (baseAlpha == null) return;
        for (int i = 0; i < sprites.Length; i++)
        {
            var sr = sprites[i];
            if (sr == null) continue;
            sr.enabled = true;
            SetAlpha(sr, baseAlpha[i]);
        }
    }

    private void Update()
    {
        IsLightInside = EvaluateLightInside();

        float target = IsLightInside ? 1f : 0f;
        progress = Mathf.MoveTowards(progress, target, Time.deltaTime / fadeDuration);

        ApplyAlpha();
    }

    // ---------------------------------------------------------------- логика

    private bool EvaluateLightInside()
    {
        if (lightObject == null || zones == null) return false;

        Transform anchor = zonesAnchor != null ? zonesAnchor : transform;
        Vector3 p = anchor.InverseTransformPoint(lightObject.position);

        for (int i = 0; i < zones.Length; i++)
        {
            Zone z = zones[i];
            if (z == null || !z.enabled) continue;

            Vector3 d = p - z.center;
            if (ignoreZ) d.z = 0f;

            if (z.shape == ZoneShape.Box)
            {
                Vector3 half = z.size * 0.5f;
                bool inside = Mathf.Abs(d.x) <= half.x && Mathf.Abs(d.y) <= half.y;
                if (!ignoreZ) inside &= Mathf.Abs(d.z) <= half.z;
                if (inside) return true;
            }
            else // Circle
            {
                if (d.sqrMagnitude <= z.radius * z.radius) return true;
            }
        }

        return false;
    }

    private void ApplyAlpha()
    {
        // t = 1 означает "спрайты должны быть полностью видимы"
        float visibility = mode == FadeMode.RevealWhenLightInside ? progress : 1f - progress;

        // SmoothStep даёт плавное ускорение и замедление на концах
        float k = Mathf.SmoothStep(hiddenAlpha, visibleAlpha, visibility);

        for (int i = 0; i < masks.Length; i++)
        {
            masks[i].alphaCutoff = 1f-k;
        }
        for (int i = 0; i < sprites.Length; i++)
        {
            SpriteRenderer sr = sprites[i];
            if (sr == null) continue;

            SetAlpha(sr, baseAlpha[i] * k);

            if (disableRendererWhenHidden)
                sr.enabled = k > 0.0001f;
        }
    }

    private void CacheBaseAlpha()
    {
        baseAlpha = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            baseAlpha[i] = sprites[i] != null ? sprites[i].color.a : 1f;
    }

    private static void SetAlpha(SpriteRenderer sr, float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    public void RefreshSprites(SpriteRenderer[] newSprites = null)
    {
        if (newSprites != null) sprites = newSprites;
        CacheBaseAlpha();
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (visibleAlpha < hiddenAlpha) visibleAlpha = hiddenAlpha;
    }
#endif

    private void OnDrawGizmosSelected()
    {
        if (zones == null) return;

        Transform anchor = zonesAnchor != null ? zonesAnchor : transform;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = anchor.localToWorldMatrix;

        foreach (Zone z in zones)
        {
            if (z == null) continue;

            Color c = z.gizmoColor;
            if (!z.enabled) c.a *= 0.25f;

            Gizmos.color = c;

            if (z.shape == ZoneShape.Box)
            {
                Vector3 size = z.size;
                if (ignoreZ) size.z = 0.01f;
                Gizmos.DrawWireCube(z.center, size);

                Gizmos.color = new Color(c.r, c.g, c.b, c.a * 0.12f);
                Gizmos.DrawCube(z.center, size);
            }
            else
            {
                Gizmos.DrawWireSphere(z.center, z.radius);
            }
        }

        Gizmos.matrix = prev;
    }
}