using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switcher : MonoBehaviour
{
    private SwitchBW[] switchBWs;
    [SerializeField] private float time;
    [SerializeField] private Material[] materials;
    [SerializeField] private bool start;
    private void Start()
    {
        switchBWs = FindObjectsOfType<SwitchBW>();
        if(start) InvokeRepeating(nameof(Switch), 0, time);
        else InvokeRepeating(nameof(Switch), time, time);
    }

    void Switch()
    {
        switchBWs = FindObjectsOfType<SwitchBW>();
        foreach (var switchBW in switchBWs)
        {
            switchBW.Switcher();
        }
        if (materials[0].color == Color.white)
        {
            materials[1].color = Color.white;
            materials[0].color = Color.black;
        }
        else
        {
            materials[0].color = Color.white;
            materials[1].color = Color.black;
        }
    }
    private void OnDisable()
    {
        CancelInvoke();
    }
}
