using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TypeOfTrigger
{
    jump, destroy
}
public class TriggerForEnemies : MonoBehaviour
{
    [SerializeField] private TypeOfTrigger type;
    [SerializeField] private float timeToRestart;
    private void Start()
    {
        Invoke(nameof(ColliderReload), timeToRestart);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IJumper jumper = collision.GetComponent<IJumper>();
        if(jumper != null && type == TypeOfTrigger.jump)
        {
            jumper.TakeJump();
            GetComponent<Collider2D>().enabled = false;
            Invoke(nameof(ColliderReload), timeToRestart);
        }
        IDestroy destroy = collision.GetComponent<IDestroy>();
        if (destroy != null && type == TypeOfTrigger.destroy)
        {
            destroy.TakeDeath();
        }
    }
    private void ColliderReload()
    {
        GetComponent<Collider2D>().enabled = true;
    }
}
