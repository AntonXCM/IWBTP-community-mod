using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class Volume : MonoBehaviour
{
    [SerializeField] private Slider[] sliders;
    [SerializeField] private AudioMixerGroup audioMixer;
    private void Start()
    {
        if (PlayerPrefs.GetInt("AnotherTime") != 1)
        {
            PlayerPrefs.SetFloat("sfxVolume", -10);
            PlayerPrefs.SetFloat("ostVolume", -10);
            PlayerPrefs.SetInt("AnotherTime", 1);
        }
        audioMixer.audioMixer.SetFloat("sfxVolume", PlayerPrefs.GetFloat("sfxVolume"));
        audioMixer.audioMixer.SetFloat("ostVolume", PlayerPrefs.GetFloat("ostVolume"));
        sliders[0].value = (30f + PlayerPrefs.GetFloat("sfxVolume")) / 40f;
        sliders[1].value = (30f + PlayerPrefs.GetFloat("ostVolume")) / 40f;
    }
    private void Update()
    {
        if(sliders[0].value > 0.01f) audioMixer.audioMixer.SetFloat("sfxVolume", PlayerPrefs.GetFloat("sfxVolume"));
        else audioMixer.audioMixer.SetFloat("sfxVolume", -80);
        if (sliders[1].value > 0.01f) audioMixer.audioMixer.SetFloat("ostVolume", PlayerPrefs.GetFloat("ostVolume"));
        else audioMixer.audioMixer.SetFloat("ostVolume", -80);
        PlayerPrefs.SetFloat("sfxVolume", -30f + 40f * sliders[0].value);
        PlayerPrefs.SetFloat("ostVolume", -30f + 40f * sliders[1].value);
    }
}
