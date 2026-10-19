using System;
using UnityEngine;

public class TriggeredReplica : EventTrigger
{
    [SerializeField] private string key;
    public void Start()
    {
        Event += () => { FindObjectOfType<ParableNarrator>().Subtitles.text = Localization.GetText(key); Destroy(this); };
    }
}