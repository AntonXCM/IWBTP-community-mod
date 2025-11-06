using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private bool blue;
    private void OnTriggerStay2D(Collider2D collision)
    {
        IDamageAble damageAble = collision.GetComponent<IDamageAble>();
        PlayerHit player = collision.GetComponent<PlayerHit>();
        if (damageAble != null && player != null)
        {
            if (player.GetComponent<Rigidbody2D>().velocity.x != 0 || player.GetComponent<Rigidbody2D>().velocity.y > 3f || player.GetComponent<Rigidbody2D>().velocity.y < -3f)
            {
                if(blue)damageAble.TakeDamage();
            }
            else
            {
                if (blue == false) damageAble.TakeDamage();
            }
        }
    }
}
