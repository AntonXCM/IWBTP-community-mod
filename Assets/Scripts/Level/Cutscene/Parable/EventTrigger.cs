using System;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    public event Action Event;
    public void OnTriggerEnter2D()
    {
        Event?.Invoke();  
    }
    public void OnCollisionEnter2D()
    {
        Event?.Invoke();
    }
}