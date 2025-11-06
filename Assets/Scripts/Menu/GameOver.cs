using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup audioMixer;
    private float speed = 1;
    private void Update()
    {
        if (speed > 0) speed -= Time.deltaTime / 2f;
        else speed = 0;
        audioMixer.audioMixer.SetFloat("ostPitch", speed);
    }
    private void OnDisable()
    {
        audioMixer.audioMixer.SetFloat("ostPitch", 1);
    }
}
