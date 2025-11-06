using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireInTheRandom : MonoBehaviour
{
    [SerializeField] private Vector2 pitch;
    private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = Random.Range(pitch.x, pitch.y);
    }
}
