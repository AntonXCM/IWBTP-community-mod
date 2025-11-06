using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineOO : MonoBehaviour
{
    [SerializeField] private float timeToChange, timeToStart;
    [SerializeField] private bool active;
    private GameObject block;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Camera cam;
    [SerializeField] private Vector2 vector;
    private void Start()
    {
        block = GetComponentInChildren<Collider2D>().gameObject;
        block.SetActive(active);
        InvokeRepeating(nameof(Change), timeToStart, timeToChange);
    }
    private void Change()
    {
        active = !active;
        block.SetActive(active);
        if (audioSource && vector.x == cam.transform.position.x && vector.y == cam.transform.position.y) audioSource.Play();
    }
}
