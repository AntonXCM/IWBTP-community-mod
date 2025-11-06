using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupEffect : MonoBehaviour
{
    [SerializeField] private GameObject effect;
    void FixedUpdate()
    {
        Instantiate(effect, transform.position, transform.rotation);
    }
}
