using UnityEngine;
using UnityEngine.Events;

namespace LevelParable {

/// <summary>
/// Лифт на сцене. Игрок «разведывает» его, просто коснувшись триггера,
/// а нажав клавишу взаимодействия в триггере — переносится в кабинку.
/// Порядок лифтов в списке этажей определяется сортировкой по имени объекта.
/// </summary>
public class Elevator : ElevatorBase
{
    [Header("Лифт")]
    [Tooltip("Название для UI. На порядок этажей не влияет — он всегда определяется именем объекта.")]
    [SerializeField] private string displayName;
    [Tooltip("Куда ставить игрока при прибытии. Если не задан — позиция этого объекта.")]
    [SerializeField] private Transform exitPoint;

    [Header("События")]
    [Tooltip("Игрок вошёл в этот лифт (и перенёсся в кабинку).")]
    public UnityEvent onPlayerEnter = new UnityEvent();
    [Tooltip("Игрок вышел из кабинки и прибыл в этот лифт.")]
    public UnityEvent onPlayerExit = new UnityEvent();

    /// <summary>Уникальный id: «сцена/имя объекта». Используется для сохранения в PlayerPrefs.</summary>
    public string Id { get; private set; }

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public bool IsDiscovered => ElevatorSystem.IsDiscovered(this);
    public Vector3 ExitPosition => exitPoint != null ? exitPoint.position : transform.position;

    // ---------- Авто-синхронизация: лифт сам регистрируется в системе ----------

    private void OnEnable()
    {
        Id = (gameObject.scene.name + "/" + name).Replace('|', '_');
        ElevatorSystem.Register(this);
    }

    protected override void OnDisable()
    {
        ElevatorSystem.Unregister(this);
        base.OnDisable();
    }

    // ---------- Логика ----------

    // Достаточно пройти мимо: касание триггера = лифт разведан.
    protected override void OnPlayerEnterZone() => ElevatorSystem.MarkDiscovered(this);

    protected override void OnInteract()
    {
        ElevatorCabin cabin = ElevatorSystem.Cabin;
        if (cabin == null)
        {
            Debug.LogWarning($"[Elevator] «{name}»: в сцене нет активной кабинки (ElevatorCabin).", this);
            return;
        }

        Transform player = PlayerTransform;
        if (player == null) return;

        PlayInteract();
        ElevatorSystem.MarkAction();

        cabin.Enter(player, this);

        onPlayerEnter.Invoke();
        ElevatorSystem.RaiseEntered(this);
    }

    /// <summary>Вызывается кабинкой: игрок выходит (прибывает) в этом лифте.</summary>
    internal void Arrive(Transform player)
    {
        ElevatorSystem.TeleportPlayer(player, ExitPosition);
        onPlayerExit.Invoke();
    }
}
}