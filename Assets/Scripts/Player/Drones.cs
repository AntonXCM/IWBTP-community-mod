using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drones : MonoBehaviour
{
    [SerializeField] private Transform firePosition;
    [SerializeField] private GameObject[] fireObjects;
    private bool ready = true;
    private KeyCode key;
    [SerializeField] private float reloadTime;
    private void Start()
    {
        key= (KeyCode)PlayerPrefs.GetInt("Key" + 3);
    }
    private void Update()
    {
        if (Time.timeScale == 1)
        {
            if (((Input.GetKey(key) || Input.GetKey(KeyCode.Mouse0) ||
                Input.GetKey(KeyCode.JoystickButton2) && PlayerPrefs.GetInt("Gamepad") == 1 || 
                Input.GetKey(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 2)) 
                && ready)
            {
                Fire();
                ready = false;
            }
        }
    }
    void Fire()
    {
        for (int i = 0; i < fireObjects.Length; i++) Instantiate(fireObjects[i], firePosition.position, firePosition.rotation);
        Invoke(nameof(Reload), reloadTime);
    }
    void Reload()
    {
        ready = true;
    }
}

