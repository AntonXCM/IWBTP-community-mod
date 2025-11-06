using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEffects : MonoBehaviour
{
    [SerializeField] private GameObject effect;
    [SerializeField] private Vector2 vector2;
    [SerializeField] private float effectPerSecond;
    [SerializeField] private float timeToEnd;
    [SerializeField] private GameObject finalEffect;
    [SerializeField] private GameObject trigger;
    private void Start()
    {
        InvokeRepeating(nameof(Effect), 0, 1f/effectPerSecond);
        Invoke(nameof(FinalEffect), timeToEnd);
    }
    void Effect()
    {
        Instantiate(effect, new Vector2(transform.position.x + Random.Range(-vector2.x, vector2.x), transform.position.y + Random.Range(-vector2.y, vector2.y)), transform.rotation);
    }
    void FinalEffect()
    {
        Instantiate(finalEffect, transform.position, transform.rotation);
        trigger.SetActive(true);
        Destroy(gameObject);
    }
}
