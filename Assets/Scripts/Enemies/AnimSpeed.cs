using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSpeed : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float speed;
    private void Start()
    {
        if (GetComponent<Animator>())
        {
            anim = GetComponent<Animator>();
            anim.speed = speed;
        }
    }
}
