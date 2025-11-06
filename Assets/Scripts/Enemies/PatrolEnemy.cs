using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    [SerializeField] private float movement, animSpeed;
    [SerializeField] private float[] pos;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        anim.speed = animSpeed;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(movement, rb.velocity.y);
        if (transform.position.x < pos[0] && movement < 0 || transform.position.x > pos[1] && movement > 0)
        {
            movement = -movement;
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);
        }
    }
}

