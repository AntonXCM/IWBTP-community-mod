using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OstSpeedHit : MonoBehaviour
{
    [SerializeField] private AudioSource ost;
    [SerializeField] private int whenSpeedUp;
    [SerializeField] private float speedUp;
    public void Take()
    {
        whenSpeedUp--;
        if (whenSpeedUp <= 0) ost.pitch += speedUp;
    }
}
