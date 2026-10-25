using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

//Да, я навайбкодил, не блигодари ^_^

/// <summary>
/// Спрайт, который "пачкается" при смерти игрока рядом (как в Super Meat Boy).
///
/// Логика по расстоянию d от места смерти:
///   d <= fullDirtDistance  -> гарантированно самая грязная стадия
///   d >  maxDistance       -> ничего не происходит
///   между ними             -> случайный прирост стадий:
///                             ближе к fullDirtDistance — почти максимум,
///                             в середине — самый большой разброс (maxVariance),
///                             у maxDistance — 50/50: либо +0, либо +1 стадия.
///
/// Состояние хранится в статической памяти:
///   - перезапуск той же сцены      -> состояние сохраняется;
///   - загрузка другой сцены        -> сбрасывается;
///   - перезапуск игры              -> сбрасывается (статика не переживает запуск).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class BloodSprite : MonoBehaviour
{
    [Header("Расстояния")]
    [Tooltip("Если смерть произошла ближе этого расстояния — спрайт гарантированно становится самым грязным.")]
    [SerializeField, Min(0f)] private float fullDirtDistance = 1.5f;

    [Tooltip("Дальше этого расстояния смерть никак не влияет на спрайт.")]
    [SerializeField, Min(0f)] private float maxDistance = 8f;

    [Tooltip("Считать расстояние до ближайшей точки границ спрайта (удобно для больших стен/тайлов). " +
             "Если выключено — до центра (pivot) объекта.")]
    [SerializeField] private bool measureFromBounds = true;

    [Header("Случайность")]
    [Tooltip("Максимальный разброс (±стадий), достигаемый в середине между fullDirtDistance и maxDistance. " +
             "К краям разброс уменьшается.")]
    [SerializeField, Min(0f)] private float maxVariance = 1f;

    [Header("Спрайты (0 — чистый, последний — самый грязный)")]
    [SerializeField] private Sprite[] stages;

    [Header("Сохранение")]
    [Tooltip("Необязательный уникальный ID внутри сцены. Если пусто — ID строится из позиции в иерархии. " +
             "Задайте вручную для объектов, которые создаются в рантайме.")]
    [SerializeField] private string customId;

    // Разброс на самом дальнем краю (±0.5 при среднем 0.5 даёт ровно "0 или +1").
    private const float FarSpread = 0.5f;
    private const float FarMean = 0.5f;

    private SpriteRenderer spriteRenderer;
    private string key;
    private int currentStage;

    private int MaxStage => stages.Length - 1;
    private bool IsConfigured => stages != null && stages.Length >= 2;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        key = BuildKey();
    }

    private void OnEnable()
    {
        PlayerHit.OnDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerHit.OnDeath -= HandlePlayerDeath;
    }

    private void Start()
    {
        // sceneLoaded (где при смене сцены чистится память) вызывается до Start,
        // поэтому здесь данные уже актуальны.
        if (!IsConfigured) return;

        if (DirtMemory.TryGet(key, out int saved) && saved > 0)
        {
            currentStage = Mathf.Clamp(saved, 0, MaxStage);
            ApplySprite();
        }
    }

    private void OnValidate()
    {
        maxDistance = Mathf.Max(maxDistance, fullDirtDistance);
    }

    // ---------------------------------------------------------------- логика

    private void HandlePlayerDeath(Vector2 deathPosition)
    {
        if (!IsConfigured || currentStage >= MaxStage) return;

        int gain = CalculateGain(DistanceTo(deathPosition));
        if (gain > 0)
            SetStage(currentStage + gain);
    }

    private float DistanceTo(Vector2 point)
    {
        if (measureFromBounds)
        {
            Bounds b = spriteRenderer.bounds;
            Vector3 closest = b.ClosestPoint(new Vector3(point.x, point.y, b.center.z));
            return Vector2.Distance(point, closest);
        }
        return Vector2.Distance(point, transform.position);
    }

    /// <summary>Сколько стадий добавить при смерти на расстоянии distance.</summary>
    private int CalculateGain(float distance)
    {
        if (distance <= fullDirtDistance) return MaxStage;   // вплотную — гарантированно максимум
        if (distance > maxDistance) return 0;                // слишком далеко

        // t: 0 — у границы полного загрязнения, 1 — у максимальной дистанции
        float t = Mathf.InverseLerp(fullDirtDistance, maxDistance, distance);

        // Среднее значение прироста линейно падает от MaxStage до 0.5
        float mean = Mathf.Lerp(MaxStage, FarMean, t);

        // Разброс: 0 у границы -> maxVariance в середине -> FarSpread на краю
        float spread = t < 0.5f
            ? Mathf.Lerp(0f, maxVariance, t * 2f)
            : Mathf.Lerp(maxVariance, FarSpread, (t - 0.5f) * 2f);

        float value = mean + Random.Range(-spread, spread);
        return Mathf.Clamp(Mathf.RoundToInt(value), 0, MaxStage);
    }

    private void SetStage(int stage)
    {
        stage = Mathf.Clamp(stage, 0, MaxStage);
        if (stage == currentStage) return;

        currentStage = stage;
        ApplySprite();
        DirtMemory.Set(key, currentStage);
    }

    private void ApplySprite()
    {
        Sprite s = stages[currentStage];
        if (s != null) spriteRenderer.sprite = s;
    }

    private string BuildKey()
    {
        string scenePath = gameObject.scene.path;

        if (!string.IsNullOrEmpty(customId))
            return scenePath + "|" + customId;

        // Путь из индексов в иерархии — уникален в пределах сцены
        var sb = new StringBuilder();
        for (Transform t = transform; t != null; t = t.parent)
            sb.Insert(0, t.GetSiblingIndex() + "/");

        return scenePath + "|" + sb;
    }
}

/// <summary>
/// Статическое хранилище стадий загрязнения.
/// Живёт, пока запущена игра; очищается при загрузке другой сцены (Single-режим).
/// Перезагрузка той же сцены данные не трогает.
/// </summary>
internal static class DirtMemory
{
    private static readonly Dictionary<string, int> stages = new Dictionary<string, int>();
    private static string lastSingleScenePath;

    // Нужно и для Editor с отключённым Domain Reload: гарантирует чистое состояние при каждом Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        stages.Clear();
        lastSingleScenePath = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;

        if(string.IsNullOrEmpty(lastSingleScenePath))
        {
            lastSingleScenePath = scene.path;
        } else if (lastSingleScenePath != scene.path)
        {
            Debug.Log(lastSingleScenePath + "   " + scene.path);
            stages.Clear();
            lastSingleScenePath = scene.path;
        }
    }

    public static bool TryGet(string key, out int stage) => stages.TryGetValue(key, out stage);

    public static void Set(string key, int stage) => stages[key] = stage;
}