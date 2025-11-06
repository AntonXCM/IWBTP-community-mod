using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OstLooper : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private float startOstTime;
    [SerializeField] private float startLoop;
    [SerializeField] private float endLoop;
    [SerializeField] private float currentTime;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if(startOstTime > 0) audioSource.time = startOstTime;
    }
    void Update()
    {
        currentTime = audioSource.time;
        if(audioSource.time >= endLoop)
        {
            audioSource.time = startLoop;
        }
    }
}
