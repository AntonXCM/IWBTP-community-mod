using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [SerializeField] private int progress;
    [SerializeField] private Elevator elevator;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageAble damageAble = collision.GetComponent<IDamageAble>();
        if (damageAble != null && progress > PlayerPrefs.GetInt("Progress"))
        {
            PlayerPrefs.SetInt("Progress", progress);
            elevator.Progress();
        }
    }
}
