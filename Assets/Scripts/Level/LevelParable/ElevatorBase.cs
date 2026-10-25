using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelParable
{

    /// <summary>
    /// Общая логика «двери»: игрок рядом (триггер) -> дверь открывается (bool "active" в аниматоре
    /// + звук), игрок ушёл -> закрывается. Нажатие клавиши рядом с дверью вызывает OnInteract().
    /// Нужен Collider2D с включённым Is Trigger на этом же объекте.
    /// </summary>
    public abstract class ElevatorBase : MonoBehaviour
    {
        [Header("Взаимодействие")]
        [SerializeField] private string playerTag = "Player";

        [Header("Анимация")]
        [Tooltip("Если не задан — ищется на этом объекте и в дочерних.")]
        [SerializeField] private Animator animator;
        [Tooltip("Bool-параметр аниматора: true — игрок рядом, дверь открыта.")]
        [SerializeField] private string activeParameter = "active";

        [Header("Звуки")]
        [Tooltip("Если не задан — на объекте будет создан 2D AudioSource.")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip interactClip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private static readonly Predicate<Collider2D> s_isStale = IsStale;

        // Все коллайдеры игрока внутри триггера (у игрока их может быть несколько).
        private readonly HashSet<Collider2D> _contacts = new HashSet<Collider2D>();
        private Transform _playerRoot;
        private int _activeHash;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        /// <summary>Игрок сейчас в триггере двери.</summary>
        public bool IsPlayerNear
        {
            get
            {
                _contacts.RemoveWhere(s_isStale); // на случай, если игрок отключён/уничтожен без OnTriggerExit
                return _contacts.Count > 0;
            }
        }

        /// <summary>Трансформ игрока, находящегося рядом с дверью (или null).</summary>
        protected Transform PlayerTransform => IsPlayerNear ? _playerRoot : null;

        // =====================================================================================

        protected virtual void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            _activeHash = Animator.StringToHash(activeParameter);

            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D-звук, чтобы не зависеть от расстояния до камеры
            }

            Collider2D zone = GetComponent<Collider2D>();
            if (zone == null || !zone.isTrigger)
                Debug.LogWarning($"[{GetType().Name}] «{name}»: нужен Collider2D с включённым Is Trigger.", this);
        }

        protected virtual void OnDisable()
        {
            _contacts.Clear();
            _isOpen = false;
        }

        protected virtual void Reset()
        {
            Collider2D zone = GetComponent<Collider2D>();
            if (zone != null) zone.isTrigger = true;
        }

        protected virtual void Update()
        {
            bool near = IsPlayerNear;
            SetOpen(near);

            if (near && InputDefault.Interact())
                TryInteract();
        }

        // =====================================================================================
        //  Взаимодействие
        // =====================================================================================

        /// <summary>
        /// Пытается использовать дверь (игрок должен быть рядом).
        /// Можно вызывать из нового Input System / UI-кнопки.
        /// </summary>
        public bool TryInteract()
        {
            if (!IsPlayerNear || !ElevatorSystem.CanInteract) return false;
            OnInteract();
            return true;
        }

        protected abstract void OnInteract();

        /// <summary>Вызывается, когда игрок впервые коснулся триггера (был вне его).</summary>
        protected virtual void OnPlayerEnterZone() { }

        // =====================================================================================
        //  Триггер
        // =====================================================================================

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayer(other)) return;

            bool wasEmpty = _contacts.Count == 0;
            if (!_contacts.Add(other)) return;

            Rigidbody2D body = other.attachedRigidbody;
            _playerRoot = body != null ? body.transform : other.transform;

            if (wasEmpty) OnPlayerEnterZone();
        }

        private void OnTriggerExit2D(Collider2D other) => _contacts.Remove(other);

        private bool IsPlayer(Collider2D other)
        {
            if (other.CompareTag(playerTag)) return true;
            Rigidbody2D body = other.attachedRigidbody;
            return body != null && body.CompareTag(playerTag);
        }

        private static bool IsStale(Collider2D c) =>
            c == null || !c.enabled || !c.gameObject.activeInHierarchy;

        // =====================================================================================
        //  Анимация и звук
        // =====================================================================================

        // Идемпотентно: звук и анимация срабатывают только при смене состояния.
        protected void SetOpen(bool open)
        {
            if (_isOpen == open) return;
            _isOpen = open;

            if (animator != null) animator.SetBool(_activeHash, open);
            Play(open ? openClip : closeClip);
        }

        protected void PlayInteract() => Play(interactClip);

        protected void Play(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip, volume);
        }
    }
}