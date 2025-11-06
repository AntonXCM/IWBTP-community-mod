using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomColor : MonoBehaviour
{
    [SerializeField] private float[] colors;
    [SerializeField] private Slider[] sliders;
    [SerializeField] private int numberOfSprite;
    [SerializeField] private PlayerColor[] playerColors;
    [SerializeField] private float[] defaultColors;
    private void Start()
    {
        colors = new float[3];
        colors[0] = PlayerPrefs.GetFloat("ColorR" + numberOfSprite);
        colors[1] = PlayerPrefs.GetFloat("ColorG" + numberOfSprite);
        colors[2] = PlayerPrefs.GetFloat("ColorB" + numberOfSprite);
        for (int i = 0; i < 3; i++) sliders[i].value = colors[i];
    }
    private void Update()
    {
        for(int i = 0; i < 3; i++) colors[i] = sliders[i].value;
        PlayerPrefs.SetFloat("ColorR" + numberOfSprite, colors[0]);
        PlayerPrefs.SetFloat("ColorG" + numberOfSprite, colors[1]);
        PlayerPrefs.SetFloat("ColorB" + numberOfSprite, colors[2]);
        for (int c = 0; c < playerColors.Length; c++) playerColors[c].ChangeColor();
    }
    public void DefaultColor()
    {
        for (int i = 0; i < 3; i++)
        {
            sliders[i].value = defaultColors[i];
        }
    }
    public void ChangeColor()
    {
        Start();
    }
}
