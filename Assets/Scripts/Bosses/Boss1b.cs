using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1b : MonoBehaviour
{
    private int numberOfPhase;
    private float time;
    [SerializeField] private BossPhase[] bossPhase;
    private TrailEffect[] trails;
    private Rigidbody2D rb;
    private Vector2 vectorRb;
    private void Start()
    {
        trails = GetComponentsInChildren<TrailEffect>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(vectorRb.x * bossPhase[numberOfPhase].speed, rb.velocity.y);
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (transform.position.x > bossPhase[numberOfPhase].point.position.x + 15)
        {
            vectorRb = new Vector2(-1, 0);
        }
        else if (transform.position.x < bossPhase[numberOfPhase].point.position.x - 15)
        {
            vectorRb = new Vector2(1, 0);
        }
        else
        {
            vectorRb = new Vector2(0, 0);
        }
        for (int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            bossPhase[numberOfPhase].attack[i].SetActive(true);
        }
        if (FindObjectOfType<PlayerController>() && time >= bossPhase[numberOfPhase].time) NextPhase();
        for (int i = 0; i < trails.Length; i++)
        {
            if (bossPhase[numberOfPhase].trail) trails[i].enabled = true;
            else trails[i].enabled = false;
        }
        if (bossPhase[numberOfPhase].flip) transform.rotation = Quaternion.Euler(0, 180, 0);
        else transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void NextPhase()
    {
        for (int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            bossPhase[numberOfPhase].attack[i].SetActive(false);
        }
        if (numberOfPhase < bossPhase.Length - 1) numberOfPhase++;
        else numberOfPhase = 0;
        time = 0;
    }
}
