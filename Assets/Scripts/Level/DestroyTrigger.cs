using UnityEngine;

public class DestroyTrigger : MonoBehaviour
{
    [SerializeField] private Object destroyable;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        Destroy(destroyable);
    }
}