using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffect : MonoBehaviour
{
    private float time;
    [SerializeField] private float spawnsPerSeconds;
    [SerializeField] private float timeToDestroy;
    [SerializeField] private GameObject trailGhostPrefab;
    private SpriteRenderer spriteObj;
    private SpriteRenderer spriteTrail;
    private void Start()
    {
        spriteObj = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (time > 0) time -= Time.deltaTime;
        else
        {
            CreateTrailGhost();
            time = 1f / spawnsPerSeconds;
        }
    }
    private void CreateTrailGhost()
    {
        GameObject trailGhost = Instantiate(trailGhostPrefab, transform.position, transform.rotation);
        trailGhost.transform.localScale = transform.localScale;
        Destroy(trailGhost, timeToDestroy);

        spriteTrail = trailGhost.GetComponent<SpriteRenderer>();
        spriteTrail.sprite = spriteObj.sprite;
        spriteTrail.sortingLayerName = spriteObj.sortingLayerName;
        spriteTrail.sortingOrder = spriteObj.sortingOrder - 10;
    }
}
