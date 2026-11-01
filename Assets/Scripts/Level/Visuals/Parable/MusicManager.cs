using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace LevelParable {
/// <summary>
/// Параметры одного переключения музыки.
/// Используется и в инспекторе (триггеры), и из кода: MusicManager.Play(settings).
/// </summary>
[System.Serializable]
public class MusicSettings
{
    [Tooltip("Аудиофайл. Если не задан (None) — текущая музыка затухает и наступает тишина.")]
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Зацикливание")]
    [Tooltip("Зацикливать ли трек. Если выключено — трек проиграется один раз.")]
    public bool loop = true;

    [Tooltip("Секунда, на которую происходит возврат при зацикливании.")]
    [Min(0f)] public float loopStart = 0f;

    [Tooltip("Секунда, в которой происходит возврат к Loop Start. 0 — конец файла.")]
    [Min(0f)] public float loopEnd = 0f;

    [Header("Переход")]
    [Tooltip("Длительность перехода в секундах. Если > 0 — текущая музыка затухает, а новая нарастает. " +
             "Если 0 — переключение мгновенное.")]
    [Min(0f)] public float transitionDuration = 1f;

    [Header("Стартовая позиция")]
    [Tooltip("Секунда нового трека, с которой начнётся воспроизведение.")]
    [Min(0f)] public float startTime = 0f;

    [Tooltip("Начать новый трек с той позиции, на которой сейчас играет текущий " +
             "(для вариаций одного трека). Start Time используется, только если сейчас музыка не играет.")]
    public bool keepCurrentPosition = false;

    [Tooltip("Если этот же клип уже играет: перезапустить его (true) или просто продолжить (false).")]
    public bool restartIfAlreadyPlaying = false;
}

/// <summary>
/// Проигрыватель музыки. Управляется через статические методы:
///   MusicManager.Play(settings);
///   MusicManager.Stop(2f);
/// Объект на сцене создавать не нужно — он создаётся автоматически.
/// </summary>
public class MusicManager : MonoBehaviour
{
    // ======================= Статический API =======================
    [SerializeField] AudioMixerGroup mixerGroup;

    /// <summary>Общая громкость музыки (0..1).</summary>
    public static float MasterVolume { get; set; } = 1f;

    /// <summary>Клип, который играет сейчас (null — тишина).</summary>
    public static AudioClip CurrentClip =>
        instance != null && instance.current != null ? instance.current.clip : null;

    /// <summary>Текущая позиция текущего трека в секундах.</summary>
    public static float CurrentTime =>
        instance != null && instance.current != null ? instance.current.Seconds : 0f;

    public static bool IsPlaying => CurrentClip != null;

    /// <summary>Переключить музыку согласно настройкам.</summary>
    public static void Play(MusicSettings settings)
    {
        if (settings == null || quitting) return;
        Instance.PlayInternal(settings);
    }

    /// <summary>Плавно выключить музыку.</summary>
    public static void Stop(float fadeDuration = 1f)
    {
        if (quitting || instance == null) return;
        instance.FadeOutAll(Mathf.Max(0f, fadeDuration));
    }

    // ======================= Внутренности =======================

    private class Voice
    {
        public AudioSource source;
        public AudioClip clip;
        public float volume = 1f;

        // Зацикливание
        public bool loop;
        public bool manualLoop;        // нужны свои точки (иначе работает нативный луп)
        public int loopStartSamples;
        public int loopEndSamples;
        public int lastSamples;

        // Фейд: level 0..1 двигается линейно, в громкость переводится по кривой равной мощности
        public float level;
        public float targetLevel;
        public float fadeSpeed;        // единиц level в секунду

        public float Seconds => source.timeSamples / (float)clip.frequency;
    }

    private static MusicManager instance;
    private static bool quitting;

    private static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("[MusicManager]");
                instance = go.AddComponent<MusicManager>();
            }
            return instance;
        }
    }

    private readonly List<Voice> voices = new List<Voice>();
    private readonly Stack<AudioSource> pool = new Stack<AudioSource>();
    private Voice current;

    // Сброс статики при отключённом Domain Reload (Enter Play Mode Options)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        quitting = false;
        MasterVolume = 1f;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    private void PlayInternal(MusicSettings s)
    {
        float duration = Mathf.Max(0f, s.transitionDuration);

        // Нет клипа — тишина
        if (s.clip == null)
        {
            FadeOutAll(duration);
            return;
        }

        // Этот же трек уже играет — не перезапускаем, только обновляем параметры
        if (current != null && current.clip == s.clip && !s.restartIfAlreadyPlaying)
        {
            current.volume = s.volume;
            ConfigureLoop(current, s);
            return;
        }

        // Позицию нужно определить до того, как старый трек будет остановлен
        float startSeconds = (s.keepCurrentPosition && current != null)
            ? current.Seconds
            : s.startTime;

        FadeOutAll(duration);
        current = StartVoice(s, startSeconds, duration);
    }

    private Voice StartVoice(MusicSettings s, float startSeconds, float fadeIn)
    {
        bool fade = fadeIn > 0f;

        var v = new Voice
        {
            source = RentSource(),
            clip = s.clip,
            volume = s.volume,
            level = fade ? 0f : 1f,
            targetLevel = 1f,
            fadeSpeed = fade ? 1f / fadeIn : 0f
        };

        v.source.clip = s.clip;
        ConfigureLoop(v, s);

        // Стартовая позиция (в сэмплах, с учётом частоты именно нового клипа)
        int total = Mathf.Max(1, s.clip.samples);
        int start = Mathf.Max(0, Mathf.RoundToInt(startSeconds * s.clip.frequency));

        // Позиция за концом петли — переносим внутрь петли с сохранением фазы
        if (v.loop && start >= v.loopEndSamples)
        {
            int loopLen = v.loopEndSamples - v.loopStartSamples;
            start = v.loopStartSamples + (start - v.loopStartSamples) % loopLen;
        }
        start = Mathf.Min(start, total - 1);

        v.source.timeSamples = start;
        v.lastSamples = start;
        v.source.volume = EvaluateVolume(v);
        v.source.Play();

        voices.Add(v);
        return v;
    }

    private static void ConfigureLoop(Voice v, MusicSettings s)
    {
        int total = Mathf.Max(1, v.clip.samples);
        int freq = v.clip.frequency;

        int ls = Mathf.Clamp(Mathf.RoundToInt(s.loopStart * freq), 0, total - 1);
        int le = s.loopEnd > 0f
            ? Mathf.Clamp(Mathf.RoundToInt(s.loopEnd * freq), 0, total)
            : total;

        if (le <= ls)
        {
            Debug.LogWarning($"[MusicManager] Некорректные точки зацикливания у '{v.clip.name}' " +
                             $"(Loop Start >= Loop End). Используется весь клип.");
            ls = 0;
            le = total;
        }

        v.loop = s.loop;
        v.loopStartSamples = ls;
        v.loopEndSamples = le;
        v.manualLoop = s.loop && (ls > 0 || le < total);
        v.source.loop = s.loop;          // нативный луп нужен и для ручного режима: источник не должен останавливаться
        v.lastSamples = v.source.timeSamples;
    }

    private void FadeOutAll(float duration)
    {
        current = null;
        for (int i = voices.Count - 1; i >= 0; i--)
        {
            Voice v = voices[i];
            if (duration <= 0f)
            {
                Release(v);
            }
            else
            {
                v.targetLevel = 0f;
                v.fadeSpeed = 1f / duration;
            }
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime; // переходы не зависят от Time.timeScale

        for (int i = voices.Count - 1; i >= 0; i--)
        {
            Voice v = voices[i];

            if (v.level != v.targetLevel)
                v.level = Mathf.MoveTowards(v.level, v.targetLevel, v.fadeSpeed * dt);

            // Затухание закончилось
            if (v.targetLevel <= 0f && v.level <= 0f)
            {
                Release(v);
                continue;
            }

            // Трек без зацикливания доиграл до конца
            if (!v.loop && !v.source.isPlaying && !AudioListener.pause)
            {
                Release(v);
                continue;
            }

            UpdateLoop(v);
            v.source.volume = EvaluateVolume(v);
        }
    }

    // Равномерная по мощности кривая: при кроссфейде суммарная громкость не «проваливается»
    private static float EvaluateVolume(Voice v)
    {
        return Mathf.Sin(v.level * Mathf.PI * 0.5f) * v.volume * MasterVolume;
    }

    private static void UpdateLoop(Voice v)
    {
        if (!v.manualLoop) return;

        int t = v.source.timeSamples;
        int loopLen = v.loopEndSamples - v.loopStartSamples;

        if (t >= v.loopEndSamples)
        {
            // Дошли до конца петли — возвращаемся к началу, сохраняя «перелёт»
            t = v.loopStartSamples + (t - v.loopEndSamples) % loopLen;
        }
        else if (t < v.lastSamples)
        {
            // Клип дошёл до своего конца (Loop End = конец файла) и нативно перемотался на 0
            t = v.loopStartSamples + t;
        }
        else
        {
            v.lastSamples = t;
            return;
        }

        v.source.timeSamples = t;
        v.lastSamples = t;
    }

    private AudioSource RentSource()
    {
        return pool.Count > 0 ? pool.Pop() : CreateSource();
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("MusicSource");
        go.transform.SetParent(transform, false);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.outputAudioMixerGroup = mixerGroup;
        src.spatialBlend = 0f; // 2D-звук
        return src;
    }

    private void Release(Voice v)
    {
        v.source.Stop();
        v.source.clip = null;
        pool.Push(v.source);
        voices.Remove(v);
        if (current == v) current = null;
    }
}
}