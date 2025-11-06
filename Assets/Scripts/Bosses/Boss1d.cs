using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BossPhase2
{
    public GameObject[] attack;
    public float time;
    public float posY;
    public float speed;
}
public class Boss1d : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    private bool right;
    private bool down;
    private int numberOfPhase;
    private float time;
    [SerializeField] private BossPhase2[] bossPhase;
    private Rigidbody2D rb;
    private float speedX;
    private float speedY;
    [SerializeField] private float speedOnX;
    [SerializeField] private float speedOnY;
    private Transform player;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>().transform;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speedX * speedOnX, speedY * speedOnY);
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (right)
        {
            if (speedX < 1) speedX += Time.deltaTime;
            else speedX = 1;
        }
        else
        {
            if (speedX > -1) speedX -= Time.deltaTime;
            else speedX = -1;
        }
        if(down)
        {
            if (speedY > -1) speedY -= Time.deltaTime * bossPhase[numberOfPhase].speed;
            else speedY = -1;
        }
        else
        {
            if (speedY < 1) speedY += Time.deltaTime * bossPhase[numberOfPhase].speed;
            else speedY = 1;
        }
        if (transform.position.x < points[0].position.x) right = true;
        if (transform.position.x > points[1].position.x) right = false;
        if (transform.position.y > bossPhase[numberOfPhase].posY) down = true;
        else down = false;
        for (int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            if (bossPhase[numberOfPhase].attack[i]) bossPhase[numberOfPhase].attack[i].SetActive(true);
        }
        if (FindObjectOfType<PlayerController>() && time >= bossPhase[numberOfPhase].time) NextPhase();
        if (player.position.x > transform.position.x) transform.rotation = Quaternion.Euler(0, 180, 0);
        else transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void NextPhase()
    {
        for (int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            if(bossPhase[numberOfPhase].attack[i]) bossPhase[numberOfPhase].attack[i].SetActive(false);
        }
        if (numberOfPhase < bossPhase.Length - 1) numberOfPhase++;
        else numberOfPhase = 0;
        time = 0;
    }
}