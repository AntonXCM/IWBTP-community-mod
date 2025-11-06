using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipCutscene : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject menu;
    private float time;
    [SerializeField] private float timeToSkip;

    void Update()
    {
        time += Time.deltaTime;
        if(time >= timeToSkip || Input.GetKeyDown(KeyCode.Return))
        {
            audioSource.time = timeToSkip;
            anim.speed = 999;
            menu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
