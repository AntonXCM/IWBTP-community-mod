using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private bool blue;
    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerHit player = collision.GetComponent<PlayerHit>();
        if (player != null)
        {
            Vector2 velocity = player.GetComponent<Rigidbody2D>().velocity;
            if (velocity.x * velocity.x + velocity.y * velocity.y > 0.01f)
            {
                if(blue) { player.Damage(); print($"player killed by blue {velocity}"); }
            }
            else
            {
                if (blue == false) { player.Damage(); print("player killed by orange"); }
            }
        }
    }
}
