using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Transform to;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if(player != null) player.transform.position = to.transform.position;
    }
}