using UnityEngine;
//Code contibuted by AntonXCM!
public class PhysicsResponse : MonoBehaviour, IDamageAble
{
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private ForceMode2D forceMode;
    [SerializeField] private Vector3 power;
    public void TakeDamage()
    {
        rigidbody.AddForce(new(Random11 * power.x, power.y), forceMode);
        rigidbody.angularVelocity += Random11 * power.z;
    }
    private static float Random11 => Random.value * 2 - 1;
}