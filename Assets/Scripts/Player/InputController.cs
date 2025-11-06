using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    private KeyCode key;
    [SerializeField] private int number;
    [SerializeField] private Text text;
    private void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            Debug.Log("Detected key code: " + e.keyCode);
            key = e.keyCode;
            PlayerPrefs.SetInt("Key" + number, (int)key);
            text.text = "" + key;
            gameObject.SetActive(false);
        }
    }
}
