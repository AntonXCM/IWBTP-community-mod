using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitLoad : MonoBehaviour, IDamageAble
{
    [SerializeField] private Material[] materials;
    [SerializeField] private GameObject[] effects;
    [SerializeField] private float timeToDead, distance;
    private Transform player;
    [SerializeField] private bool active;
    void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        if (active) TakeDamage();
    }
    private void Update()
    {
        if(distance > 0 && Vector2.Distance(transform.position, player.position) < distance)
        {
            TakeDamage();
            distance = -1;
        }
    }
    void IDamageAble.TakeDamage()
    {
        TakeDamage();
        GetComponent<AudioSource>().Play();
    }
    void ResetMaterial()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < sprites.Length; i++) sprites[i].material = materials[0];
        Invoke(nameof(ResetMaterial2), 0.04f);
    }
    void ResetMaterial2()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < sprites.Length; i++) sprites[i].material = materials[1];
        Invoke(nameof(ResetMaterial), 0.04f);
    }
    public void TakeDamage()
    {
        ResetMaterial2();
        Invoke(nameof(Dead), timeToDead);
    }
    void Dead()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            Instantiate(effects[i], transform.position, Quaternion.Euler(0, 0, 0));
        }
        Destroy(gameObject);
    }
}

