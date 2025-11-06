using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorActivator : MonoBehaviour
{
    [SerializeField] private int progress;
    [SerializeField] private bool active;
    private void Start()
    {
        if (progress <= PlayerPrefs.GetInt("Progress")) gameObject.SetActive(active);
        else gameObject.SetActive(!active);
    }
}
