using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndBoss : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private float volumeAudio;
    [SerializeField] private GameObject victory;
    [SerializeField] private float time;
    [SerializeField] private int scene;
    private void Start()
    {
        volumeAudio = audioSource.volume;
    }
    private void Update()
    {
        if (audioSource.volume > 0) audioSource.volume -= Time.deltaTime * volumeAudio;
        else audioSource.volume = 0;
        time -= Time.deltaTime;
        if (time <= 0) Victory();
        if (time <= -10) SceneManager.LoadScene(scene);
    }
    void Victory()
    {
        victory.SetActive(true);
    }
}
