using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EzMode : MonoBehaviour
{
    [SerializeField] private bool active;
    private void Start()
    {
        if(PlayerPrefs.GetInt("ezMode") == 1)
        {
            gameObject.SetActive(active);
        }
        else
        {
            gameObject.SetActive(!active);
        }
    }
}
