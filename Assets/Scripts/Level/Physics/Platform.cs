using UnityEngine;
//Code contibuted by AntonXCM!
public class Platform : MonoBehaviour
{
    [SerializeField] private Transform player, relativeTransform;
    private bool GetPlayerAbove => player.position.y > relativeTransform.position.y;
    private bool wasPlayerAbove;
    private void Start()
    {
        wasPlayerAbove = GetPlayerAbove;
        SwitchColliders(wasPlayerAbove);
    }

    private void FixedUpdate()
    {
        bool isPlayerAbove = GetPlayerAbove;
        if (isPlayerAbove == wasPlayerAbove) return;
        wasPlayerAbove = isPlayerAbove;
        SwitchColliders(isPlayerAbove);
    }
    [SerializeField] private Collider2D[] colliders;
    private void SwitchColliders(bool enable) //TODO: Extract colliders and this func to new parent 
    {
        foreach (var collider in colliders)
            collider.enabled = enable;
    }
}
