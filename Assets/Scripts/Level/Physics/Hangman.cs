using UnityEngine;

/// <summary>
/// Качели. Центр вращения - transform.position этого объекта.
/// На объекте должен быть Collider2D с включённым Is Trigger.
///
/// Внутри радиуса игрок летит свободно (гравитация + управление, скорость сохраняется).
/// За радиусом орбита работает как пружина: игрока тянет обратно к кругу,
/// поэтому он может ехать по окружности, чуть выходить за неё и отскакивать
/// (радиальная скорость инвертируется). Игрок остаётся обычным динамическим телом,
/// так что коллизии продолжают работать.
/// </summary>
public class Hangman : MonoBehaviour, IMovementOverride
{
    [Header("Геометрия")]
    [SerializeField] private float radius = 5f;

    [Header("Пружинистость орбиты")]
    [Tooltip("Жёсткость. ~50 - мягкая резина, ~300 - заметно тянется, 1500+ - почти жёсткая верёвка. " +
             "Выше ~3000 при fixedDeltaTime 0.02 симуляция становится нестабильной.")]
    [Range(1f, 3000f)] [SerializeField] private float stiffness = 300f;
    [Tooltip("Гашение при растяжении. 0 - идеальный отскок (радиальная скорость инвертируется без потерь), " +
             "больше - отскок слабее и орбита быстрее успокаивается.")]
    [Range(0f, 50f)] [SerializeField] private float damping = 2f;

    [Header("Посадка")]
    [Tooltip("Множитель скорости игрока в момент посадки. 1 - без изменений, <1 - гасит, >1 - усиливает, " +
             "0 - полная остановка, отрицательные значения разворачивают скорость.")]
    [SerializeField] private Vector2 landingSpeedMultiplier = Vector2.one;

    [Header("Управление игрока")]
    [Tooltip("Ускорение от кнопок влево/вправо. Разгоняет только до moveSpeed игрока, " +
             "более высокую скорость (инерцию) не ограничивает.")]
    [SerializeField] private float controlAcceleration = 30f;
    [Tooltip("Пауза перед повторным захватом после отвязки.")]
    [SerializeField] private float reattachDelay = 0.3f;

    [Header("Speedrun")]
    [SerializeField] private bool speedrun;
    [Tooltip("Минимальная скорость вдоль орбиты, при которой смена направления считается разворотом.")]
    [SerializeField] private float minBackSpeed = 0.5f;

    [Header("Звуки")]
    [Tooltip("Необязательно. Если не задан, берётся AudioSource с этого объекта или создаётся новый.")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attachSound;
    [Tooltip("Отвязка в радиусе, ближе к центру (до innerZoneFraction * radius).")]
    [SerializeField] private AudioClip detachSoundCenter;
    [Tooltip("Отвязка в радиусе, дальше от центра (от innerZoneFraction * radius до зоны у края).")]
    [SerializeField] private AudioClip detachSoundMiddle;
    [Tooltip("Отвязка рядом с радиусом (в пределах edgeTolerance по обе стороны от него).")]
    [SerializeField] private AudioClip detachSoundEdge;
    [Tooltip("Отвязка дальше радиуса (за зоной у края, пружина растянута).")]
    [SerializeField] private AudioClip detachSoundOutside;
    [Tooltip("Граница между Center и Middle как доля радиуса.")]
    [Range(0.05f, 0.95f)] [SerializeField] private float innerZoneFraction = 0.5f;
    [Tooltip("Полуширина зоны Edge вокруг радиуса, в единицах мира.")]
    [Min(0f)] [SerializeField] private float edgeTolerance = 0.5f;

    [Header("Визуал")]
    [Tooltip("Необязательно. Спрайт рычага, вращается вокруг центра качелей.")]
    [SerializeField] private Transform arm;
    [Tooltip("Куда смотрит спрайт в обычном положении. Спрайт вниз - 90, вверх - -90, вправо - 0, влево - 180. " +
             "Если рычаг повёрнут не туда, подберите значение.")]
    [SerializeField] private float armAngleOffset = 90f;
    [Tooltip("Скорость возврата рычага в исходное положение после отвязки, градусов в секунду.")]
    [SerializeField] private float armReturnSpeed = 360f;

    private PlayerController player;
    private Rigidbody2D rb;
    private float nextAttachTime;
    private Quaternion armRestLocalRotation;

    private void Awake()
    {
        if (arm != null) armRestLocalRotation = arm.localRotation;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Reset()
    {
        CircleCollider2D c = GetComponent<CircleCollider2D>();
        if (c == null) c = gameObject.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = radius;
    }

    // ---------- Захват / отвязка ----------

    private static PlayerController GetPlayer(Collider2D c)
    {
        PlayerController p = c.GetComponentInParent<PlayerController>();
        return p != null && p.CompareTag("Player") ? p : null;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryAttach(other);

    private void TryAttach(Collider2D other)
    {
        if (player != null || Time.time < nextAttachTime) return;

        PlayerController p = GetPlayer(other);
        if (p == null || p.IsMovementOverridden) return;

        player = p;
        player.extraJump = 1;
        rb = p.Body;
        p.SetMovementOverride(this);
        rb.WakeUp();
        rb.velocity *= landingSpeedMultiplier;   // скорость игрока переходит качелям
        Play(attachSound);
    }

    /// <summary>
    /// Отвязывает игрока (IMovementOverride). Вызывается из PlayerController при прыжке или отключении.
    /// Звук выхода играет только при прыжке: при отключении/смерти игрока он молчит.
    /// </summary>
    public void Release()
    {
        if (player == null) return;

        bool byJump = player.isActiveAndEnabled;   // false, если игрока отключают (смерть, респавн, смена сцены)
        float dist = Vector2.Distance(rb.position, transform.position);

        Detach();
        if (byJump) Play(PickDetachSound(dist));
    }

    /// <summary>Тихая отвязка без звука.</summary>
    private void Detach()
    {
        if (player == null) return;

        // Velocity уже лежит в Rigidbody2D, возвращать нечего
        player.SetMovementOverride(null);
        player = null;
        rb = null;
        nextAttachTime = Time.time + reattachDelay;
    }

    private void OnDisable() => Detach();

    // ---------- Звуки ----------

    private AudioClip PickDetachSound(float dist)
    {
        if (dist > radius + edgeTolerance) return detachSoundOutside;
        if (dist >= radius - edgeTolerance) return detachSoundEdge;
        if (dist >= radius * innerZoneFraction) return detachSoundMiddle;
        return detachSoundCenter;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    // ---------- Физика ----------

    private float ControlAcceleration(float vx)
    {
        int input = PlayerController.Movement;
        if (input == 0 || input * vx >= player.MoveSpeed) return 0f;
        return input * controlAcceleration;
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        rb.WakeUp();   // авто-сон Box2D не должен останавливать качели

        float dt = Time.fixedDeltaTime;
        Vector2 offset = rb.position - (Vector2)transform.position;
        float dist = offset.magnitude;

        Vector2 v = rb.velocity;
        v.x += ControlAcceleration(v.x) * dt;

        if (dist > radius)
        {
            Vector2 n = offset / dist;                   // от центра к игроку
            Vector2 t = new Vector2(-n.y, n.x);          // вдоль орбиты, против часовой

            // Пружина тянет обратно к кругу. Гравитацию добавит сам Unity.
            float stretch = dist - radius;
            float vr = Vector2.Dot(v, n);
            float pull = Mathf.Max(0f, stiffness * stretch + damping * vr);   // пружина не толкает наружу
            v -= n * (pull * dt);

            if (speedrun && n.y * pull < minBackSpeed && v.y < minBackSpeed)
            {
                Kill();
                return;
            }
        }

        rb.velocity = v;
    }

    private void Kill()
    {
        Detach();   // без звука выхода: смерть играет свои звуки
        PlayerHit.instance.Damage();
    }

    // ---------- Визуал ----------

    private Quaternion ArmRestRotation =>
        arm.parent != null ? arm.parent.rotation * armRestLocalRotation : armRestLocalRotation;

    private void LateUpdate()
    {
        if (arm == null) return;

        if (player != null)
        {
            // Пока игрок на качелях (и внутри радиуса, и за ним) рычаг смотрит на него
            Vector2 d = rb.position - (Vector2)transform.position;
            if (d.sqrMagnitude < 0.0001f) return;
            arm.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + armAngleOffset);
        }
        else
        {
            // После отвязки плавно возвращаем рычаг в исходное положение
            arm.rotation = Quaternion.RotateTowards(arm.rotation, ArmRestRotation, armReturnSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 c = transform.position;

        // Зоны звука выхода
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(c, radius * innerZoneFraction);
        Gizmos.DrawWireSphere(c, Mathf.Max(0f, radius - edgeTolerance));
        Gizmos.DrawWireSphere(c, radius + edgeTolerance);

        Gizmos.color = speedrun ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(c, radius);
    }
}