using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartScene : MonoBehaviour
{
    private KeyCode key;
    private PlayerController player;
    [SerializeField] private bool ezMemClose;
    [SerializeField] private AchieveSystem achieveSystem;
    private void Start()
    {
        key = (KeyCode)PlayerPrefs.GetInt("Key" + 5);
        player = FindObjectOfType<PlayerController>();
    }
    void Update()
    {
        if(Input.GetKeyDown(key) || 
            Input.GetKeyDown(KeyCode.JoystickButton6) && PlayerPrefs.GetInt("Gamepad") == 1 || Input.GetKeyDown(KeyCode.JoystickButton8) && PlayerPrefs.GetInt("Gamepad") == 2)
        {
            player.gameObject.SetActive(false);
            if (PlayerPrefs.GetInt("ezMode") == 1 && Random.Range(11, 99) >= 96 && ezMemClose == false) EzMem();
            else SceneManager.LoadScene(PlayerPrefs.GetInt("SaveScene"));
        }
        if (ezMemClose && player.transform.position.x < -216 && PlayerPrefs.GetInt("ezMode") == 1)
        {
            achieveSystem.GetAchieve(32);
            EzMem();
        }
    }
    private void EzMem()
    {
        SceneManager.LoadScene(17);
    }
}
