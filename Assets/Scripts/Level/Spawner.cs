using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnObj;
    [SerializeField] private float timeToSpawn, timeToPrepare;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(timeToPrepare);   
        var wait = new WaitForSeconds(timeToSpawn);   
        while(true)
        {
            Instantiate(spawnObj, transform.position, transform.rotation).SetActive(true);
            yield return wait;
        }
    }
}
