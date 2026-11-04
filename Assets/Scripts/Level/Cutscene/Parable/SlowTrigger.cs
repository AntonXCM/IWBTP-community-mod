using System;
using UnityEngine;

public class SlowTrigger : MonoBehaviour
{
    [SerializeField] float speed;
    float playerspeed;
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.TryGetComponent<PlayerController>(out var pc))
        {
            playerspeed = pc.MoveSpeed;
            pc.SetSpeed( speed);
        }
    }
    public void OnTriggerExit2D(Collider2D collider)
    {
        if(collider.TryGetComponent<PlayerController>(out var pc))
            pc.SetSpeed(playerspeed);
    }
}