using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    private float[] color;
    [SerializeField] private int numberOfSprite;
    [SerializeField] private float[] defaultColors;
    private void Awake()
    {
        if (PlayerPrefs.GetInt("AnotherTime" + numberOfSprite) != 1)
        {
            PlayerPrefs.SetFloat("ColorR" + numberOfSprite, defaultColors[0]);
            PlayerPrefs.SetFloat("ColorG" + numberOfSprite, defaultColors[1]);
            PlayerPrefs.SetFloat("ColorB" + numberOfSprite, defaultColors[2]);
            PlayerPrefs.SetInt("AnotherTime" + numberOfSprite, 1);
        }
    }
    private void Start()
    {
        color = new float[3];
        color[0] = PlayerPrefs.GetFloat("ColorR" + numberOfSprite);
        color[1] = PlayerPrefs.GetFloat("ColorG" + numberOfSprite);
        color[2] = PlayerPrefs.GetFloat("ColorB" + numberOfSprite);
        GetComponent<SpriteRenderer>().color = new Color(color[0], color[1], color[2]);
    }
    public void ChangeColor()
    {
        Start();
    }
}
