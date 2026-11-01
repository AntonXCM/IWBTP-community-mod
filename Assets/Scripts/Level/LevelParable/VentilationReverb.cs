using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Плавно включает SFX Reverb на канале AudioMixer, пока объект игрока находится в вентиляции.
///
/// Настройка микшера:
///  1. Выберите группу (например, SFX) → Add… → SFX Reverb.
///  2. Выставьте у реверба «выключенное» состояние, например Room = -10000.
///  3. ПКМ по названию параметра (Room) → «Expose … to script».
///  4. В Exposed Parameters (справа вверху окна Audio Mixer) переименуйте параметр
///     и впишите это имя в список Parameters ниже.
///
/// Включить можно несколько параметров сразу (Room, Reflections Level, Decay Time...):
/// у каждого свои значения «выключено» и «включено», между ними идёт плавный переход.
///
/// Проверка «внутри» учитывает позицию, поворот и масштаб вентиляции (см. VentilationMesh.ContainsWorldPoint).
/// </summary>
[DisallowMultipleComponent]
public class VentilationReverb : MonoBehaviour
{
    [System.Serializable]
    public class MixerParameter
    {
        [Tooltip("Имя exposed-параметра в AudioMixer")]
        public string name = "VentReverbRoom";
        [Tooltip("Значение, когда игрок вне вентиляции (реверб выключен)")]
        public float offValue = -10000f;
        [Tooltip("Значение, когда игрок в вентиляции")]
        public float onValue = -1000f;
    }

    [Header("Что проверяем")]
    [Tooltip("Объект игрока. Берётся его позиция (pivot).")]
    [SerializeField] private Transform player;
    [Tooltip("Вентиляции, внутри которых включается реверб. Достаточно быть в любой из них.")]
    [SerializeField] private VentilationMesh[] ventilations;
    [Tooltip("Считать клетки-выходы частью вентиляции")]
    [SerializeField] private bool includeExits = false;

    [Header("Микшер")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private MixerParameter[] parameters = { new MixerParameter() };

    [Header("Переходы")]
    [Tooltip("Секунд на полное включение при входе в вентиляцию (0 — мгновенно)")]
    [SerializeField, Min(0f)] private float fadeInTime = 0.4f;
    [Tooltip("Секунд на полное выключение при выходе (0 — мгновенно)")]
    [SerializeField, Min(0f)] private float fadeOutTime = 0.8f;

    private float weight;        // 0 — реверб выключен, 1 — включён (до сглаживания кривой)
    private bool warnedMissing;

    public bool IsInside { get; private set; }

    /// <summary>Текущая степень включения реверба, 0..1.</summary>
    public float Weight => weight;

    public Transform Player
    {
        get => player;
        set => player = value;
    }

    private void Start()
    {
        // На старте сцены — без плавного перехода, сразу в правильное состояние.
        IsInside = CheckInside();
        weight = IsInside ? 1f : 0f;
        Apply();
    }

    private void Update()
    {
        IsInside = CheckInside();

        float target = IsInside ? 1f : 0f;
        if (Mathf.Approximately(weight, target)) return;

        float time = IsInside ? fadeInTime : fadeOutTime;
        weight = time <= 0f
            ? target
            : Mathf.MoveTowards(weight, target, Time.unscaledDeltaTime / time); // не зависит от timeScale

        Apply();
    }

    // Возвращаем микшер в «выключенное» состояние: в редакторе значения exposed-параметров
    // могут оставаться изменёнными после остановки игры.
    private void OnDisable()
    {
        weight = 0f;
        Apply();
    }

    private bool CheckInside()
    {
        if (player == null || ventilations == null) return false;

        Vector3 position = player.position;
        foreach (VentilationMesh vent in ventilations)
        {
            if (vent != null && vent.ContainsWorldPoint(position, includeExits)) return true;
        }
        return false;
    }

    private void Apply()
    {
        if (mixer == null || parameters == null) return;

        float t = Mathf.SmoothStep(0f, 1f, weight); // мягкое начало и конец перехода

        foreach (MixerParameter p in parameters)
        {
            if (p == null || string.IsNullOrEmpty(p.name)) continue;

            bool ok = mixer.SetFloat(p.name, Mathf.Lerp(p.offValue, p.onValue, t));
            if (!ok && !warnedMissing)
            {
                warnedMissing = true;
                Debug.LogWarning($"AudioMixer не имеет exposed-параметра \"{p.name}\". " +
                                 "Выведите параметр реверба наружу (Expose to script) и проверьте имя.", this);
            }
        }
    }
}