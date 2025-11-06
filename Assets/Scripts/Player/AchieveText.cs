using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchieveText : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if(player != null) obj.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null) obj.SetActive(false);
    }
}
