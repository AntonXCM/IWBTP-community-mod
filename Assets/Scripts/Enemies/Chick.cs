using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chick : MonoBehaviour, IDamageAble, IJumper, IDestroy
{
    private Animator anim;
    private Rigidbody2D rb;
    private float movement;
    private float movementUp = 1;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float animSpeed;
    [SerializeField] private LayerMask layer;
    [SerializeField] private Transform posJump;
    [SerializeField] private Vector2 hitboxJump;
    private bool isGround;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform[] points;
    [SerializeField] private GameObject effect;
    [SerializeField] private HitPoints hitPoints;
    [SerializeField] private float scaleDown;
    private float speedUp = 1;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (transform.rotation.y == 0) movement = 1;
        else movement = -1;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(movement * movementUp * moveSpeed * speedUp, rb.velocity.y);
    }
    private void Update()
    {
        if (transform.position.x <= points[0].position.x && rb.velocity.x > -1f)
        {
            movement = 1;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (transform.position.x >= points[1].position.x && rb.velocity.x < 1f)
        {
            movement = -1;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        anim.SetBool("Walking", true);
        anim.speed = moveSpeed * movementUp * speedUp / animSpeed;
        isGround = Physics2D.OverlapBox(posJump.position, hitboxJump, 0, layer);
        anim.SetBool("Ground", isGround);
        if (rb.velocity.y < -177) rb.velocity = new Vector2(rb.velocity.x, -177);
        if (movementUp > 1) movementUp -= Time.deltaTime;
        else movementUp = 1;
    }
    void IDamageAble.TakeDamage()
    {
        movementUp++;
        if (hitPoints)
        {
            hitPoints.TakeDamage();
            transform.localScale = new Vector2(transform.localScale.x - scaleDown, transform.localScale.y - scaleDown);
            speedUp += 0.1f;
            hitboxJump = new Vector2(hitboxJump.x - 3.5f, hitboxJump.y);
            jumpForce -= 0.6f;
            rb.gravityScale+= 0.6f;
        }
    }
    void IJumper.TakeJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce * 10);
    }
    void IDestroy.TakeDeath()
    {
        Instantiate(effect, transform.position, Quaternion.Euler(0,0,0));
        Destroy(gameObject);
    }
}
