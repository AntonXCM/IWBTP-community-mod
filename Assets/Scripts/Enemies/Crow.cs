using UnityEngine;

[RequireComponent(typeof(CarMovement))]
public class Crow : MonoBehaviour
{
    public Vector2 jumpVelocity;
    public float jumpDistance;
    private CarMovement _car;
    private Rigidbody2D _rb => _car.Rigidbody;

    private void Awake() => _car = GetComponent<CarMovement>();

    private void FixedUpdate()
    {
        var active = IsActive();
        var bodyType = active ? RigidbodyType2D.Dynamic : RigidbodyType2D.Static;
        if (_rb.bodyType != bodyType) _rb.bodyType = bodyType;
        if (!active) return;

        if (!TryGetTarget(out var targetPos)) return;
        _car.Move(targetPos);
        
        if(PlayerController.HoldsJump && targetPos.y > transform.position.y)
        {
            float factor =Mathf.Clamp01(1f - Mathf.Abs(targetPos.x - transform.position.x) / jumpDistance);
            _rb.velocity += jumpVelocity * factor * factor;    
        }
    }

    protected virtual bool IsActive() => true;

    protected virtual bool TryGetTarget(out Vector2 position)
    {
        position = PlayerHit.instance.transform.position;
        return true;
    }
}