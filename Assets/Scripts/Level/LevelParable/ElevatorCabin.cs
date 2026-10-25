using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LevelParable {

/// <summary>
/// Кабинка — единое место на сцене, куда ведут все лифты. Скрипт вешается на ДВЕРЬ кабинки
/// (объект с триггером, Animator и звуками). Использование двери выводит игрока
/// в выбранный разведанный лифт.
/// По умолчанию выбран следующий по имени разведанный лифт после того, в который игрок вошёл
/// (лифт 1 -> выход в лифте 2 -> вход в лифт 2 -> выход в лифте 3), стрелками можно выбрать другой.
/// </summary>
public class ElevatorCabin : ElevatorBase
{
    [Serializable] public class StringEvent : UnityEvent<string> { }

    [Header("Кабинка")]
    [Tooltip("Куда ставить игрока при входе в лифт. Лучше внутри триггера двери кабинки. Если не задан — позиция этого объекта.")]
    [SerializeField] private Transform playerPoint;
    [Tooltip("Звук переключения этажа (необязательно).")]
    [SerializeField] private AudioClip selectClip;

    [Header("События")]
    [Tooltip("Выбранный этаж изменился. Передаётся DisplayName лифта — удобно привязать к тексту UI.")]
    public StringEvent onSelectedFloorChanged = new StringEvent();

    private readonly List<Elevator> _floors = new List<Elevator>();
    private Elevator _origin;   // лифт, через который вошёл игрок
    private Elevator _selected; // выбранный лифт выхода

    public Elevator SelectedFloor => _selected != null ? _selected : null;

    // ---------- Авто-синхронизация ----------

    private void OnEnable() => ElevatorSystem.Register(this);

    protected override void OnDisable()
    {
        ElevatorSystem.Unregister(this);
        base.OnDisable();
    }

    // ---------- Ввод выбора этажа ----------

    protected override void Update()
    {
        base.Update();

        if (!IsPlayerNear || !ElevatorSystem.CanInteract) return;
        // Спасибо, клод, но у нас хардкорный гейминг и ещё отсылка на Vitavania 
    // [SerializeField] private KeyCode floorUpKey = KeyCode.UpArrow;
    // [SerializeField] private KeyCode floorDownKey = KeyCode.DownArrow;
        // if (KeyDown(floorUpKey)) SelectNextFloor();
        // else if (KeyDown(floorDownKey)) SelectPreviousFloor();
    }

    // ---------- Вход (вызывается лифтом) ----------

    internal void Enter(Transform player, Elevator origin)
    {
        _origin = origin;

        ElevatorSystem.TeleportPlayer(player, playerPoint != null ? playerPoint.position : transform.position);

        RefreshFloors();
        _selected = PickDefaultFloor();
        NotifySelection();
    }

    // ---------- Дверь изнутри ----------

    protected override void OnInteract() => TravelToSelected();

    /// <summary>Выйти через дверь в выбранный лифт (игрок должен быть у двери).</summary>
    public bool TravelToSelected()
    {
        Transform player = PlayerTransform;
        if (player == null) return false;

        RefreshFloors();
        Elevator destination = _floors.Contains(_selected) ? _selected : PickDefaultFloor();
        if (destination == null)
        {
            Debug.LogWarning("[ElevatorCabin] Нет разведанных лифтов — выходить некуда.", this);
            return false;
        }

        Elevator origin = _origin;
        _origin = null;
        _selected = null;

        PlayInteract();
        ElevatorSystem.MarkAction();

        destination.Arrive(player);
        ElevatorSystem.RaiseArrived(origin, destination);
        return true;
    }

    public void SelectNextFloor() => StepFloor(+1);
    public void SelectPreviousFloor() => StepFloor(-1);

    // ---------- Внутренняя логика ----------

    private void StepFloor(int direction)
    {
        RefreshFloors();
        if (_floors.Count == 0) { _selected = null; return; }

        int index = _floors.IndexOf(_selected);
        index = index < 0 ? 0 : (index + direction + _floors.Count) % _floors.Count;
        _selected = _floors[index];

        Play(selectClip);
        NotifySelection();
    }

    // Список этажей пересобирается при каждом обращении, поэтому не хранит устаревших
    // индексов: выбранный лифт хранится ссылкой, а не номером.
    private void RefreshFloors() => ElevatorSystem.GetDiscovered(_floors);

    // Следующий по имени после лифта входа (по кругу). Если лифт входа неизвестен — первый.
    private Elevator PickDefaultFloor()
    {
        if (_floors.Count == 0) return null;
        int index = _floors.IndexOf(_origin);
        return _floors[index < 0 ? 0 : (index + 1) % _floors.Count];
    }

    private void NotifySelection() =>
        onSelectedFloorChanged.Invoke(_selected != null ? _selected.DisplayName : string.Empty);
}}