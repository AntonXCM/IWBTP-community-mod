using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchBW : MonoBehaviour
{
    [SerializeField] private GameObject[] obj;
    [SerializeField] private Material material;

    private void Awake()
    {
        if (material.color == Color.white)
        {
            obj[0].SetActive(true);
            obj[1].SetActive(false);
        }
        else
        {
            obj[1].SetActive(true);
            obj[0].SetActive(false);
        }
    }
    public void Switcher()
    {
        if (material.color == Color.white)
        {
            obj[0].SetActive(true);
            obj[1].SetActive(false);
        }
        else
        {
            obj[1].SetActive(true);
            obj[0].SetActive(false);
        }
    }
}
