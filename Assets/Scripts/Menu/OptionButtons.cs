using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionButtons : MonoBehaviour
{
    [SerializeField] private Text text;
    private void Start()
    {
        if (PlayerPrefs.GetInt("Gamepad") != 1 && PlayerPrefs.GetInt("Gamepad") != 2) PlayerPrefs.SetInt("Gamepad", 1);
        if(text)Gamepad();
    }
    public void Translate()
    {
        if (PlayerPrefs.GetInt("Language") == 1) PlayerPrefs.SetInt("Language", 0);
        else PlayerPrefs.SetInt("Language", 1);
        var TranstateText = FindObjectsOfType<Translator>();
        for (int i = 0; i < TranstateText.Length; i++) TranstateText[i].ChangeLanguage();
    }
    public void FullScreen()
    {
        if(Screen.fullScreen == false) Screen.SetResolution(Screen.mainWindowDisplayInfo.width, Screen.mainWindowDisplayInfo.height, true);
        else Screen.SetResolution(1280, 720, false);
    }
    public void URL(string url)
    {
        Application.OpenURL(url);
    }
    void Gamepad()
    {
        if (PlayerPrefs.GetInt("Gamepad") == 1) text.text = "xbox";
        if (PlayerPrefs.GetInt("Gamepad") == 2) text.text = "ps";
    }
    public void GamepadChanger()
    {
        if (PlayerPrefs.GetInt("Gamepad") == 1) PlayerPrefs.SetInt("Gamepad", 2);
        else PlayerPrefs.SetInt("Gamepad", 1);
        Gamepad();
    }
    public void RandomCustomisation()
    {
        for(int c = 0; c < 6; c++)
        {
            PlayerPrefs.SetFloat("ColorR" + c, Random.Range(0f, 1f));
            PlayerPrefs.SetFloat("ColorG" + c, Random.Range(0f, 1f));
            PlayerPrefs.SetFloat("ColorB" + c, Random.Range(0f, 1f));
        }
        var colorsSlider = FindObjectsOfType<CustomColor>();
        for (int i = 0; i < colorsSlider.Length; i++)
        {
            colorsSlider[i].ChangeColor();
        }
        var colorsPlayer = FindObjectsOfType<PlayerColor>();
        for(int i = 0; i < colorsPlayer.Length; i++)
        {
            colorsPlayer[i].ChangeColor();
        }
        var decor = FindObjectOfType<DecorItems>();
        for (int d = 0; d < Random.Range(6, 16); d++)
        {
            decor.SelectItem(Random.Range(0, decor.itemCount));
        }
    }
    public void Graphics()
    {
        FindObjectOfType<PostProcessing>().Graphics();
    }
}
