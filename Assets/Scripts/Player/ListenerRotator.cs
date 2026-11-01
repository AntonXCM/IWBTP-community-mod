using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenerRotator : MonoBehaviour
{
    Transform listener;
    void Start() => listener = transform;
    void Update() => listener.rotation = Quaternion.identity;
}
