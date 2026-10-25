using System;
using UnityEngine;

public class TriggeredNameReplica : EventTrigger
{
    [SerializeField] bool Oneshot = true;
    [SerializeField] private string key;
    public void Start()
    {
        Event += () => { FindObjectOfType<ParableNarrator>().Subtitles.text = string.IsNullOrEmpty(key) ? string.Empty : Localization.GetTextFormatted(fullKey:key, Environment.UserName); if(Oneshot)Destroy(this); };
    }
}