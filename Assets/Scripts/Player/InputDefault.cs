using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputDefault : MonoBehaviour
{
    private KeyCode[] keys;
    [SerializeField] private Text[] text;
    void Start()
    {
        keys = new KeyCode[6];
        if (PlayerPrefs.GetInt("DefaultControl") != 1)
        {
            keys[0] = KeyCode.RightArrow;
            keys[1] = KeyCode.LeftArrow;
            keys[2] = KeyCode.Z;
            keys[3] = KeyCode.X;
            keys[4] = KeyCode.C;
            keys[5] = KeyCode.R;
            for (int k = 0; k < 6; k++) PlayerPrefs.SetInt("Key" + k, (int)keys[k]);
            PlayerPrefs.SetInt("DefaultControl", 1);
        }
    }

    void Update()
    {
        for (int k = 0; k < 6; k++) keys[k] = (KeyCode)PlayerPrefs.GetInt("Key" + k);
        for (int i = 0; i < 6; i++) text[i].text = "" + keys[i];
    }
}
