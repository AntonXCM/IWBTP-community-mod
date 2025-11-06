using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class GameMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private AudioMixerGroup audioMixer;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) || 
            Input.GetKeyDown(KeyCode.JoystickButton7) && PlayerPrefs.GetInt("Gamepad") == 1 || Input.GetKeyDown(KeyCode.JoystickButton9) && PlayerPrefs.GetInt("Gamepad") == 2)
        {
            menu.SetActive(!menu.activeSelf);
            if (menu.activeSelf)
            {
                Time.timeScale = 0;
                audioMixer.audioMixer.SetFloat("ostLowPass", 1100);
                audioMixer.audioMixer.SetFloat("sfxPitch", 0);
            }
            else
            {
                Time.timeScale = 1;
                audioMixer.audioMixer.SetFloat("ostLowPass", 22000);
                audioMixer.audioMixer.SetFloat("sfxPitch", 1);
            }
        }
    }
    private void OnEnable()
    {
        Time.timeScale = 1;
        audioMixer.audioMixer.SetFloat("ostLowPass", 22000);
        audioMixer.audioMixer.SetFloat("sfxPitch", 1);
    }
    private void OnDisable()
    {
        Time.timeScale = 1;
        audioMixer.audioMixer.SetFloat("ostLowPass", 22000);
        audioMixer.audioMixer.SetFloat("sfxPitch", 1);
    }
}
