using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private Transform[] guns;
    [SerializeField] private Transform[] firePositions;
    [SerializeField] private GameObject[] fireObjects;
    private int numberOfGun;
    [SerializeField] private bool autoFire;
    private bool ready = true;
    private SpriteSelecter[] spriteSelecters;
    private KeyCode[] keys;
    [SerializeField] private float reloadTime;
    private int countOfBullet;
    private void Start()
    {
        keys = new KeyCode[2];
        keys[0] = (KeyCode)PlayerPrefs.GetInt("Key" + 3);
        keys[1] = (KeyCode)PlayerPrefs.GetInt("Key" + 4);
        if (PlayerPrefs.GetInt("Autofire") != 1) autoFire = false;
        else autoFire = true;
        spriteSelecters = FindObjectsOfType<SpriteSelecter>();
        for (int i = 0; i < fireObjects.Length; i++) spriteSelecters[i].enabled = autoFire;
        AutoFire();
    }
    private void Update()
    {
        if (Time.timeScale == 1 && countOfBullet < 5)
        {
            if ((autoFire && (Input.GetKey(keys[0]) || Input.GetKey(KeyCode.Mouse0) || 
                Input.GetKey(KeyCode.JoystickButton2) && PlayerPrefs.GetInt("Gamepad") == 1 || (Input.GetKey(KeyCode.JoystickButton0)) && PlayerPrefs.GetInt("Gamepad") == 2 || Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.JoystickButton4)) ||
                autoFire == false && (Input.GetKeyDown(keys[0]) || Input.GetKeyDown(KeyCode.Mouse0) || 
                Input.GetKeyDown(KeyCode.JoystickButton2) && PlayerPrefs.GetInt("Gamepad") == 1 || (Input.GetKeyDown(KeyCode.JoystickButton0)) && PlayerPrefs.GetInt("Gamepad") == 2 || Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton4))) &&
                ready)
            {
                Fire();
                ready = false;
            }
            if (Input.GetKeyDown(keys[1]) || Input.GetKeyDown(KeyCode.Mouse1) ||
                Input.GetKeyDown(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 1 || Input.GetKeyDown(KeyCode.JoystickButton2) && PlayerPrefs.GetInt("Gamepad") == 2)
            {
                autoFire = !autoFire;
                AutoFire();
            }
        }
    }
    void Fire()
    {
        guns[numberOfGun].localRotation = Quaternion.Euler(0, 0, -90);
        for (int i = 0; i < fireObjects.Length; i++) Instantiate(fireObjects[i], firePositions[numberOfGun].position, firePositions[numberOfGun].rotation);
        if (numberOfGun < guns.Length - 1) numberOfGun++;
        else numberOfGun = 0;
        Invoke(nameof(Reload), reloadTime);
    }
    void AutoFire()
    {
        spriteSelecters = FindObjectsOfType<SpriteSelecter>();
        for (int i = 0; i < fireObjects.Length; i++) spriteSelecters[i].enabled = autoFire;
        if (autoFire) PlayerPrefs.SetInt("Autofire", 1);
        else PlayerPrefs.SetInt("Autofire", 0);
    }
    void Reload()
    {
        for (int i = 0; i < guns.Length; i++) guns[i].localRotation = Quaternion.Euler(0, 0, 0);
        ready = true;
    }
    public void CountOfBullet(int count)
    {
        countOfBullet += count;
    }
}
