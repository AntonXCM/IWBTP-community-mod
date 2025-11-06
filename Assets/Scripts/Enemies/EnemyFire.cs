using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
enum Type
{
    aim, rotate, random
}
public class EnemyFire : MonoBehaviour
{
    [SerializeField] private int countOfBullets;
    private int count;
    [SerializeField] private GameObject[] fireObjects;
    [SerializeField] private Transform parent;
    [SerializeField] private Transform firePositions;
    [SerializeField] private Transform[] extraFirePositions;
    private Transform player;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private float timeReload;
    [SerializeField] private float randomReload;
    [SerializeField] private float dead;
    private float rotateZ;
    private float timeF;
    private float timeR;
    [SerializeField] private Type type;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Vector2 randomZ = new Vector2(0,360);
    private void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        OnEnable();
        if (dead > 0) Invoke(nameof(Dead), dead);
    }
    void Dead()
    {
        GetComponent<EnemyFire>().enabled = false;
    }
    private void OnEnable()
    {
        timeR = timeReload + Random.Range(-randomReload, randomReload);
        timeF = 0;
        rotateZ = transform.eulerAngles.z;
        count = 0;
    }
    private void Update()
    {
        if (type == Type.aim)
        {
            Vector3 difference = player.position - transform.position;
            rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        }
        if (type == Type.rotate) rotateZ += Time.deltaTime * rotateSpeed;
        if (type == Type.random) rotateZ = Random.Range(randomZ.x, randomZ.y);
        if (count > 0)
        {
            timeF += Time.deltaTime;
            if (timeF >= timeBetweenShots)
            {
                for (int i = 0; i < fireObjects.Length; i++)
                {
                    if (parent == false)
                    {
                        Instantiate(fireObjects[i], firePositions.position, Quaternion.Euler(0, 0, rotateZ));
                        for (int e = 0; e < extraFirePositions.Length; e++) Instantiate(fireObjects[i], extraFirePositions[e].position, Quaternion.Euler(0, 0, rotateZ));
                    }
                    else
                    {
                        Instantiate(fireObjects[i], firePositions.position, Quaternion.Euler(0, 0, rotateZ), parent);
                        for (int e = 0; e < extraFirePositions.Length; e++) Instantiate(fireObjects[i], extraFirePositions[e].position, Quaternion.Euler(0, 0, rotateZ), parent);
                    }
                }
                count--;
                timeF = 0;
            }
        }
        else
        {
            timeR -= Time.deltaTime;
            if(timeR <= 0)
            {
                count = countOfBullets;
                timeR = timeReload + Random.Range(-randomReload, randomReload);
            }
        }
    }
}
