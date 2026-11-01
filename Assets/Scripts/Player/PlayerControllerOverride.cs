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
    private float KeepMomentum(float currentVy, float jumpVy) =>
        rb.gravityScale > 0 ? Mathf.Max(currentVy, jumpVy) : Mathf.Min(currentVy, jumpVy);
}