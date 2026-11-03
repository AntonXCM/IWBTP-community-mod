using UnityEngine;
public interface IMovementOverride
{
    void Release();
}

public partial class PlayerController
{
    private IMovementOverride movementOverride;

    public bool IsMovementOverridden => movementOverride != null;
    public Rigidbody2D Body => rb;
    public float MoveSpeed => moveSpeed;

    public void SetMovementOverride(IMovementOverride value)
    {
        movementOverride = value;
        if (value != null) rb.WakeUp();
    }
    private bool ReleaseOverride()
    {
        if (movementOverride == null) return false;
        movementOverride.Release();
        return true;
    }

    /// Заменяет составляющую скорости вдоль оси, остальное не трогает.
    private static Vector2 WithComponentAlong(Vector2 v, Vector2 axis, float value) =>
        v + axis * (value - Vector2.Dot(v, axis));

    /// Прыжок вдоль "вверх" (против гравитации).
    /// keepMomentum: сохранить разгон, если он уже больше обычного прыжка.
    private Vector2 JumpVelocity(Vector2 v, float jumpSpeed, bool keepMomentum)
    {
        Vector2 up = -gravityVector;
        float vUp = Vector2.Dot(v, up);
        return WithComponentAlong(v, up, keepMomentum ? Mathf.Max(vUp, jumpSpeed) : jumpSpeed);
    }
}