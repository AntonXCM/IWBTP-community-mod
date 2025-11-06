using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BossPhase
{
    public Transform point;
    public float speed;
    public GameObject[] attack;
    public float time;
    public bool flip;
    public bool trail;
    public BeePlatforms[] beePlatforms;
}
public class Boss1a : MonoBehaviour
{
    private int numberOfPhase;
    private float time;
    [SerializeField] private BossPhase[] bossPhase;
    private TrailEffect[] trails;
    private void Start()
    {
        trails = GetComponentsInChildren<TrailEffect>();
    }
    private void Update()
    {
        time += Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, bossPhase[numberOfPhase].point.position, bossPhase[numberOfPhase].speed * Time.deltaTime);
        for(int i = 0; i < bossPhase[numberOfPhase].attack.Length; i++)
        {
            if(bossPhase[numberOfPhase].attack[i]) bossPhase[numberOfPhase].attack[i].SetActive(true);
        }
        if (FindObjectOfType<PlayerController>() && time >= bossPhase[numberOfPhase].time) NextPhase();
        if (bossPhase[numberOfPhase].flip) transform.rotation = Quaternion.Euler(0, 180, 0);
        else transform.rotation = Quaternion.Euler(0, 0, 0);
        if (trails[0])
        {
            for (int i = 0; i < trails.Length; i++)
            {
                if (bossPhase[numberOfPhase].trail) trails[i].enabled = true;
                else trails[i].enabled = false;
            }
        }
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
