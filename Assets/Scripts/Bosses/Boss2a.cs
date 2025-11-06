using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BeePlatforms
{
    public GameObject block;
    public bool active;
    public float time;
}
public class Boss2a : MonoBehaviour
{
    private int numberOfPhase;
    private float time;
    [SerializeField] private BossPhase[] bossPhase;
    private TrailEffect[] trails;
    private Rigidbody2D rb;
    private Vector2 vectorRb;
    private Transform player;
    [SerializeField] private AudioSource sfxPlatforms;
    private int beePlatCount;
    [SerializeField] private int phaseRepeat;
    private void Start()
    {
        trails = GetComponentsInChildren<TrailEffect>();
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>().transform;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(vectorRb.x * bossPhase[numberOfPhase].speed, rb.velocity.y);
    }

    private void Update()
    {
        time += Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, bossPhase[numberOfPhase].point.position, bossPhase[numberOfPhase].speed * Time.deltaTime);
        for (int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            bossPhase[numberOfPhase].attack[i].SetActive(true);
        }
        if (time >= bossPhase[numberOfPhase].time && FindObjectOfType<PlayerController>().enabled) NextPhase();
        for (int i = 0; i < trails.Length; i++)
        {
            if (bossPhase[numberOfPhase].trail) trails[i].enabled = true;
            else trails[i].enabled = false;
        }
        if (player.position.x > transform.position.x) transform.rotation = Quaternion.Euler(0, 180, 0);
        else transform.rotation = Quaternion.Euler(0, 0, 0);
        if(time > bossPhase[numberOfPhase].beePlatforms[beePlatCount].time && beePlatCount < bossPhase[numberOfPhase].beePlatforms.Length)
        {
            bossPhase[numberOfPhase].beePlatforms[beePlatCount].block.SetActive(bossPhase[numberOfPhase].beePlatforms[beePlatCount].active);
            sfxPlatforms.Play();
            beePlatCount++;
        }
    }
    private void NextPhase()
    {
        for (int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            bossPhase[numberOfPhase].attack[i].SetActive(false);
        }
        if (numberOfPhase < bossPhase.Length - 1) numberOfPhase++;
        else numberOfPhase = phaseRepeat;
        time = 0;
        beePlatCount = 0;
    }
}
