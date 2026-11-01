using UnityEngine;

/// <summary>
/// Настраиваемый деспавнер: либо готовый пресет, либо собственный набор правил из инспектора.
/// </summary>
public class RuleDespawner : ScreenDespawner
{
    public enum CombineMode
    {
        /// <summary>Деспавнить, если выполняется ЛЮБОЕ правило (ИЛИ).</summary>
        Any,
        /// <summary>Деспавнить, только если выполняются ВСЕ правила (И).</summary>
        All
    }

    [Header("Rules")]
    [Tooltip("Custom — использовать список правил ниже. Остальные значения — готовые пресеты (правила по ИЛИ).")]
    [SerializeField] private ScreenDespawnPreset preset = ScreenDespawnPreset.OutsideBox;

    [Tooltip("Для пресетов: сколько экранов вокруг камеры остаётся загруженными (X — по горизонтали, Y — по вертикали).")]
    [SerializeField] private Vector2Int presetRange = Vector2Int.one;

    [Tooltip("Только для Custom: как объединять правила.")]
    [SerializeField] private CombineMode combine = CombineMode.Any;

    [Tooltip("Только для Custom. Пустой список — деспавна не будет никогда.")]
    [SerializeField] private ScreenRule[] customRules = new ScreenRule[0];

    private ScreenRule[] activeRules = new ScreenRule[0];
    private bool activeAny = true;

    protected virtual void Awake() => Rebuild();

    // Изменения в инспекторе во время игры применяются сразу.
    private void OnValidate()
    {
        presetRange = Vector2Int.Max(presetRange, Vector2Int.zero);
        if (Application.isPlaying)
            Rebuild();
    }

    private void Rebuild()
    {
        if (preset == ScreenDespawnPreset.Custom)
        {
            activeRules = customRules ?? new ScreenRule[0];
            activeAny = combine == CombineMode.Any;
        }
        else
        {
            activeRules = ScreenDespawnPresets.Build(preset, presetRange);
            activeAny = true;
        }

        InvalidateCache();
    }

    protected override bool ShouldDespawn(Vector2Int objectScreen, Vector2Int cameraScreen)
    {
        ScreenRule[] rules = activeRules;
        if (rules == null || rules.Length == 0)
            return false;

        if (activeAny)
        {
            for (int i = 0; i < rules.Length; i++)
                if (rules[i].Evaluate(objectScreen, cameraScreen))
                    return true;
            return false;
        }

        for (int i = 0; i < rules.Length; i++)
            if (!rules[i].Evaluate(objectScreen, cameraScreen))
                return false;
        return true;
    }

    /// <summary>Переключиться на пресет из кода.</summary>
    public void SetPreset(ScreenDespawnPreset newPreset, Vector2Int range)
    {
        preset = newPreset;
        presetRange = Vector2Int.Max(range, Vector2Int.zero);
        Rebuild();
    }

    /// <summary>Задать собственные правила из кода.</summary>
    public void SetCustomRules(ScreenRule[] rules, bool any = true)
    {
        preset = ScreenDespawnPreset.Custom;
        customRules = rules ?? new ScreenRule[0];
        combine = any ? CombineMode.Any : CombineMode.All;
        Rebuild();
    }

#if UNITY_EDITOR
    [ContextMenu("Copy preset to custom rules")]
    private void CopyPresetToCustom()
    {
        if (preset == ScreenDespawnPreset.Custom)
            return;

        UnityEditor.Undo.RecordObject(this, "Copy preset to custom rules");
        customRules = ScreenDespawnPresets.Build(preset, presetRange);
        combine = CombineMode.Any;
        preset = ScreenDespawnPreset.Custom;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}