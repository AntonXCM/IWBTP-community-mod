using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelected : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject oldButton;
    private void Start()
    {
        if (PlayerPrefs.GetInt("SaveScene") > 0)
        {
            if(continueButton)continueButton.SetActive(true);
            //if(oldButton)oldButton.SetActive(false);
        }
    }
    public void NewGame()
    {
        PlayerPrefs.DeleteKey("Deaths");
        PlayerPrefs.DeleteKey("TimeOfThisFuckingGame");
        PlayerPrefs.DeleteKey("Progress");
        for(int i = 1; i <= 30; i++) PlayerPrefs.DeleteKey("SecretItem" + i);
        PlayerPrefs.SetInt("ezMode", 0);
        SceneManager.LoadScene(1);
    }
    public void NewEzGame()
    {
        PlayerPrefs.DeleteKey("Deaths");
        PlayerPrefs.DeleteKey("TimeOfThisFuckingGame");
        PlayerPrefs.DeleteKey("Progress");
        for (int i = 1; i <= 30; i++) PlayerPrefs.DeleteKey("SecretItem" + i);
        PlayerPrefs.SetInt("ezMode", 1);
        SceneManager.LoadScene(1);
    }
    public void Continue()
    {
        PlayerPrefs.SetFloat("ostTime", 0);
        SceneManager.LoadScene(PlayerPrefs.GetInt("SaveScene"));
    }
    public void Level(int scene)
    {
        PlayerPrefs.SetFloat("SaveX", 0);
        PlayerPrefs.SetFloat("SaveY", 300);
        PlayerPrefs.SetFloat("SaveZ", 0);
        PlayerPrefs.SetInt("SaveScene", scene);
        PlayerPrefs.SetFloat("ostTime", 0);
        SceneManager.LoadScene(scene);
    }
    public void Hub()
    {
        PlayerPrefs.SetFloat("SaveX", -680);
        PlayerPrefs.SetFloat("SaveY", -60);
        PlayerPrefs.SetFloat("SaveZ", 0);
        PlayerPrefs.SetInt("SaveScene", 5);
        PlayerPrefs.SetFloat("ostTime", 0);
        SceneManager.LoadScene(5);
    }
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
}
