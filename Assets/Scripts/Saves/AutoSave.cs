using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSave : MonoBehaviour
{
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private float rotateY;
    [SerializeField] private int scene;
    [SerializeField] private bool ResetTimerPT;
    private void Start()
    {
        PlayerPrefs.SetFloat("SaveX", playerPosition.x);
        PlayerPrefs.SetFloat("SaveY", playerPosition.y);
        PlayerPrefs.SetFloat("SaveZ", rotateY);
        PlayerPrefs.SetInt("SaveScene", scene);
        PlayerPrefs.SetFloat("ostTime", 0);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.JoystickButton7) && PlayerPrefs.GetInt("Gamepad") == 1 || Input.GetKeyDown(KeyCode.JoystickButton9) && PlayerPrefs.GetInt("Gamepad") == 2)
        {
            Invoke(nameof(Scene), 1.2f);
            GetComponent<Animator>().SetTrigger("Level");
            if (ResetTimerPT)
            {
                if(PlayerPrefs.GetInt("ezMode") == 1) PlayerPrefs.SetFloat("PIZZATIME", 480);
                else PlayerPrefs.SetFloat("PIZZATIME", 360);
            }
        }
    }
    void Scene()
    {
        SceneManager.LoadScene(scene);
    }
}
