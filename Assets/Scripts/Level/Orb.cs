using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] private GameObject effect;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if(player != null)
        {
            player.GetExtraJump();
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
            Invoke(nameof(Restart), 2f);
            GetComponent<Collider2D>().enabled = false;
        }
    }
    void Restart()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        GetComponent<Collider2D>().enabled = true;
    }
}
