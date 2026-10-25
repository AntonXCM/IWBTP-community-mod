using System;
using UnityEngine;

public class TriggeredReplica : EventTrigger
{
    [SerializeField] bool Oneshot = true;
    [SerializeField] private string key;
    public void Start()
    {
        Event += () => { FindObjectOfType<ParableNarrator>().Subtitles.text = string.IsNullOrEmpty(key) ? string.Empty : Localization.GetText(key); if(Oneshot)Destroy(this); };
    }
}