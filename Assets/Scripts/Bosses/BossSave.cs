using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSave : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    private Rigidbody2D rb;
    private Vector2 vectorRb;
    private float speed = 120;
    [SerializeField] private GameObject[] bossElements;
    [SerializeField] private int hit;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        vectorRb = new Vector2(0, 0);
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(vectorRb.x * speed, rb.velocity.y);
    }

    private void Update()
    {
        if (transform.position.x <= points[0].position.x)
        {
            vectorRb = new Vector2(1, 0);
        }
        if (transform.position.x >= points[1].position.x)
        {
            vectorRb = new Vector2(-1, 0);
        }
    }
    public void TakeDamage()
    {
        hit++;
        if (hit > 0)
        {
            if (transform.position.x < points[2].position.x) vectorRb = new Vector2(1, 0);
            else vectorRb = new Vector2(-1, 0);
        }
        if(hit == 1)
        {
            bossElements[0].SetActive(true);
            bossElements[1].SetActive(true);
            bossElements[5].SetActive(false);
        }
        if (hit > 55) Instantiate(bossElements[2], transform.position, Quaternion.Euler(0, 0, Random.Range(0, 30)));
        if (hit == 15 || hit == 30 || hit == 40 || hit == 50 || hit == 60 || hit == 70 || hit == 80 || hit == 85) Instantiate(bossElements[3], transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)), bossElements[4].transform);
        speed += 3;
    }
}

