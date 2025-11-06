using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OstSaveTime : MonoBehaviour
{
    private AudioSource ost;
    [SerializeField] private float time;
    [SerializeField] private bool reset;
    private void Start()
    {
        ost = GetComponent<AudioSource>();
        if(reset || PlayerPrefs.GetFloat("ostTime") < time) PlayerPrefs.SetFloat("ostTime", time);
        OnEnable();
    }
    private void OnEnable()
    {
        ost = GetComponent<AudioSource>();
        ost.time = PlayerPrefs.GetFloat("ostTime");
    }
    private void Update()
    {
        PlayerPrefs.SetFloat("ostTime", ost.time);
    }
}
