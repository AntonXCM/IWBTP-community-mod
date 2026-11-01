using System;
using UnityEngine;

/// <summary>
/// Car-like 2D movement: turns toward the target, drives in reverse if the target is behind,
/// brakes using the stopping-distance formula. Self-contained: needs only a Rigidbody2D.
/// Call <see cref="Move"/> every FixedUpdate while there is a target. If it is not called,
/// the car stops applying force (coasting).
/// </summary>
[AddComponentMenu("Movement/Car Movement")]
[RequireComponent(typeof(Rigidbody2D))]
public class CarMovement : MonoBehaviour
{
    [Tooltip("Thrust force (ForceMode2D.Force).")]
    [SerializeField] private float speed = 10f;

    [Tooltip("Max steering input in degrees. Torque grows as a cube of the angle, so this is a soft limit.")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Hard cap on angular velocity (deg/s). 0 or less disables the cap.")]
    [SerializeField] private float angularVelocityHardCap = 120f;
    [SerializeField] private float straightforwardTargetVelocity = 0;

    private float torque = 0;

    private Rigidbody2D _rb;
    private Vector2 _force;
    private bool _reverse;

    public event Action<bool> OnChangeForceSign;

    public Rigidbody2D Rigidbody => _rb;
    public float ForceMultiplier { get; private set; }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public bool Reverse
    {
        get => _reverse;
        private set
        {
            if (_reverse == value) return;
            _reverse = value;
            OnChangeForceSign?.Invoke(value);
        }
    }

    /// <summary>Unity 6 uses linearVelocity, older versions use velocity.</summary>
    public Vector2 Velocity
    {
#if UNITY_6000_0_OR_NEWER
        get => _rb.linearVelocity;
        set => _rb.linearVelocity = value;
#else
        get => _rb.velocity;
        set => _rb.velocity = value;
#endif
    }

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (_force != Vector2.zero)
        {
            _rb.AddForce(_force, ForceMode2D.Force);
            _force = Vector2.zero;
        }
        if(torque != 0)
        {
            var angular = _rb.angularVelocity + torque;
            if (angularVelocityHardCap > 0f)
                angular = Mathf.Clamp(angular, -angularVelocityHardCap, angularVelocityHardCap);
            _rb.angularVelocity = angular;
            torque = 0;
        }
    }

    /// <summary>Steer and accelerate toward a world-space point. Force is applied in the next FixedUpdate.</summary>
    public void Move(Vector2 target)
    {
        Vector2 toTarget = target - _rb.position;

        var rotation = _rb.rotation;
        var angleDiff = Mathf.DeltaAngle(rotation, Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg);

        Reverse = Mathf.Abs(angleDiff) > 90f;
        if (Reverse) angleDiff -= Mathf.Sign(angleDiff) * 180f;
        torque = angleDiff * rotationSpeed;

        ForceMultiplier = Reverse ? -1 : 1;

        var rad = rotation * Mathf.Deg2Rad;
        _force = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (speed * ForceMultiplier) + straightforwardTargetVelocity * toTarget.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_rb) return;
        Vector3 pos = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + (Vector3)_force);

        var acceleration = speed * Mathf.Abs(ForceMultiplier) / _rb.mass;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pos + transform.up,
            pos + transform.up + transform.right * (Velocity.sqrMagnitude / (2f * acceleration)));
    }
}