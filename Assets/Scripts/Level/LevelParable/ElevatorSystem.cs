using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelParable
{
/// <summary>
/// Статический реестр лифтов и кабинки + хранение разведанных лифтов в PlayerPrefs.
/// Ничего настраивать не нужно: Elevator и ElevatorCabin регистрируются сами, пока активны,
/// и сами удаляются при выключении объекта / выгрузке сцены.
/// </summary>
public static class ElevatorSystem
{
    /// <summary>Ключ PlayerPrefs со списком разведанных лифтов (id разделены символом '|').</summary>
    public const string PrefsKey = "ElevatorSystem.Discovered";

    private const char SeparatorChar = '|';

    // ---------- Статическое состояние (всё сбрасывается при смене сцены / запуске) ----------
    private static readonly List<Elevator> s_elevators = new List<Elevator>();
    private static readonly Predicate<Elevator> s_isDead = e => e == null;
    private static readonly Comparison<Elevator> s_floorOrder = CompareFloors;
    private static ElevatorCabin s_cabin;
    private static int s_lastActionFrame = -1;

    // ---------- Глобальные события ----------
    /// <summary>Игрок вошёл в лифт (аргумент — лифт, в который он вошёл).</summary>
    public static event Action<Elevator> PlayerEntered;

    /// <summary>Игрок вышел из кабинки (аргументы — лифт входа (может быть null) и лифт выхода).</summary>
    public static event Action<Elevator, Elevator> PlayerArrived;

    /// <summary>Текущая кабинка сцены или null.</summary>
    public static ElevatorCabin Cabin => s_cabin != null ? s_cabin : null;

    // =====================================================================================
    //  Сброс статики
    // =====================================================================================

    // Вызывается до загрузки первой сцены. Нужен, если в Project Settings отключён
    // Domain Reload (Enter Play Mode Options): иначе статика «переживает» остановку игры.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_elevators.Clear();
        s_cabin = null;
        s_lastActionFrame = -1;
        PlayerEntered = null;
        PlayerArrived = null;

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Страховка: убираем «мёртвые» ссылки на объекты выгруженной сцены.
    private static void OnSceneUnloaded(Scene scene)
    {
        s_elevators.RemoveAll(s_isDead);
        if (s_cabin == null) s_cabin = null; // Unity-null -> настоящий null
    }

    // =====================================================================================
    //  Регистрация (вызывается из Elevator / ElevatorCabin)
    // =====================================================================================

    internal static void Register(Elevator elevator)
    {
        if (s_elevators.Contains(elevator)) return;

        foreach (Elevator other in s_elevators)
        {
            if (other != null && other.Id == elevator.Id)
            {
                Debug.LogWarning(
                    $"[ElevatorSystem] Несколько лифтов с именем «{elevator.name}» в сцене «{elevator.gameObject.scene.name}». " +
                    "Дайте им уникальные имена, иначе они считаются одним лифтом.", elevator);
                break;
            }
        }

        s_elevators.Add(elevator);
    }

    // Удаляем только самих себя: порядок OnDisable старой сцены / OnEnable новой не важен.
    internal static void Unregister(Elevator elevator) => s_elevators.Remove(elevator);

    internal static void Register(ElevatorCabin cabin)
    {
        if (s_cabin != null && s_cabin != cabin && s_cabin.gameObject.scene == cabin.gameObject.scene)
            Debug.LogWarning("[ElevatorSystem] В сцене больше одной кабинки (ElevatorCabin). Используется последняя активированная.", cabin);

        s_cabin = cabin;
    }

    internal static void Unregister(ElevatorCabin cabin)
    {
        if (s_cabin == cabin) s_cabin = null;
    }

    // =====================================================================================
    //  Разведанные лифты (PlayerPrefs)
    // =====================================================================================

    public static bool IsDiscovered(Elevator elevator) =>
        elevator != null && ReadDiscovered().Contains(elevator.Id);

    internal static void MarkDiscovered(Elevator elevator)
    {
        HashSet<string> set = ReadDiscovered();
        if (!set.Add(elevator.Id)) return; // уже разведан

        PlayerPrefs.SetString(PrefsKey, string.Join(SeparatorChar.ToString(), set));
        PlayerPrefs.Save();
    }

    /// <summary>Забыть все разведанные лифты (например, при новой игре).</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Заполняет список разведанными И существующими сейчас лифтами,
    /// отсортированными по имени (с учётом чисел: Elevator_2 идёт раньше Elevator_10).
    /// </summary>
    public static void GetDiscovered(List<Elevator> result)
    {
        result.Clear();
        HashSet<string> known = ReadDiscovered();

        foreach (Elevator e in s_elevators)
            if (e != null && known.Contains(e.Id))
                result.Add(e);

        result.Sort(s_floorOrder);
    }

    // PlayerPrefs не кэшируем намеренно: так внешние DeleteAll/DeleteKey не рассинхронизируют систему.
    private static HashSet<string> ReadDiscovered()
    {
        var set = new HashSet<string>();
        string raw = PlayerPrefs.GetString(PrefsKey, string.Empty);
        if (raw.Length == 0) return set;

        foreach (string id in raw.Split(SeparatorChar))
            if (id.Length > 0) set.Add(id);

        return set;
    }

    // =====================================================================================
    //  Сортировка по имени
    // =====================================================================================

    private static int CompareFloors(Elevator a, Elevator b)
    {
        int c = NaturalCompare(a.name, b.name);
        return c != 0 ? c : string.CompareOrdinal(a.Id, b.Id);
    }

    // Сравнение строк, при котором числа сравниваются как числа, регистр не важен.
    private static int NaturalCompare(string a, string b)
    {
        int i = 0, j = 0;
        while (i < a.Length && j < b.Length)
        {
            if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
            {
                int si = i, sj = j;
                while (i < a.Length && char.IsDigit(a[i])) i++;
                while (j < b.Length && char.IsDigit(b[j])) j++;

                string na = a.Substring(si, i - si).TrimStart('0');
                string nb = b.Substring(sj, j - sj).TrimStart('0');
                if (na.Length != nb.Length) return na.Length < nb.Length ? -1 : 1;

                int cn = string.CompareOrdinal(na, nb);
                if (cn != 0) return cn;
            }
            else
            {
                int cc = char.ToUpperInvariant(a[i]).CompareTo(char.ToUpperInvariant(b[j]));
                if (cc != 0) return cc;
                i++;
                j++;
            }
        }
        return (a.Length - i).CompareTo(b.Length - j);
    }

    // =====================================================================================
    //  Телепорт игрока и защита от «двойного» нажатия
    // =====================================================================================

    internal static void TeleportPlayer(Transform player, Vector3 target)
    {
        target.z = player.position.z; // сохраняем глубину игрока
        player.position = target;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body == null) return;

        body.position = target;
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = Vector2.zero;
#else
        body.velocity = Vector2.zero;
#endif
        body.angularVelocity = 0f;
    }

    // Одно нажатие клавиши не должно сработать дважды за кадр
    // (например, выйти из кабинки и сразу войти в лифт, к которому телепортировались).
    internal static bool CanInteract => Time.frameCount != s_lastActionFrame;

    internal static void MarkAction() => s_lastActionFrame = Time.frameCount;

    // =====================================================================================
    //  Безопасный вызов событий
    // =====================================================================================

    // Подписчик, уничтоженный при смене сцены без отписки, не ломает остальных:
    // «мёртвые» подписчики пропускаются и автоматически отписываются.
    internal static void RaiseEntered(Elevator origin)
    {
        Action<Elevator> handlers = PlayerEntered;
        if (handlers == null) return;

        foreach (Delegate d in handlers.GetInvocationList())
        {
            if (IsDestroyedTarget(d)) { PlayerEntered -= (Action<Elevator>)d; continue; }
            try { ((Action<Elevator>)d).Invoke(origin); }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }

    internal static void RaiseArrived(Elevator origin, Elevator destination)
    {
        Action<Elevator, Elevator> handlers = PlayerArrived;
        if (handlers == null) return;

        foreach (Delegate d in handlers.GetInvocationList())
        {
            if (IsDestroyedTarget(d)) { PlayerArrived -= (Action<Elevator, Elevator>)d; continue; }
            try { ((Action<Elevator, Elevator>)d).Invoke(origin, destination); }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }

    private static bool IsDestroyedTarget(Delegate d) =>
        d.Target is UnityEngine.Object target && target == null;
}
}