using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnObj;
    [SerializeField] private float timeToSpawn;
    private void Start()
    {
        InvokeRepeating(nameof(Spawn), 0, timeToSpawn);
    }
    void Spawn()
    {
        Instantiate(spawnObj, transform.position, transform.rotation);
    }
}
