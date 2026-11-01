using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnObj;
    [SerializeField] private float timeToSpawn, timeToPrepare;
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(timeToPrepare);
        InvokeRepeating(nameof(Spawn), 0, timeToSpawn);
    }
    void Spawn()
    {
        Instantiate(spawnObj, transform.position, transform.rotation).SetActive(true);
    }
}
