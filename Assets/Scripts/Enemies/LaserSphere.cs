using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSphere : MonoBehaviour
{
    private Animator anim;
    private Laser[] lasers;
    [SerializeField] private int mode;
    [SerializeField] private float timeToChangeMode;
    private float time;
    [SerializeField] private Vector2[] points;
    [SerializeField] private float speed;
    private int numberPoint;
    private void Start()
    {
        anim = GetComponent<Animator>();
        lasers = GetComponentsInChildren<Laser>();
        Mode();
    }
    private void Update()
    {
        if(timeToChangeMode > 0)
        {
            time += Time.deltaTime;
            if (time > timeToChangeMode)
            {
                if (mode == 1) mode = 2;
                else mode = 1;
                Mode();
                time = 0;
            }
        }
        if (speed > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, points[numberPoint], speed * Time.deltaTime);
            if(transform.position.x == points[numberPoint].x)
            {
                if (numberPoint < points.Length - 1) numberPoint++;
                else numberPoint = 0;
            }
        }
    }
    void Mode()
    {
        if (mode == 0)
        {
            lasers[0].gameObject.SetActive(false);
            lasers[1].gameObject.SetActive(false);
            anim.SetBool("power", false);
        }
        if (mode == 1)
        {
            lasers[0].gameObject.SetActive(true);
            lasers[1].gameObject.SetActive(false);
            anim.SetBool("power", true);
            anim.SetBool("blue", true);
        }
        if (mode == 2)
        {
            lasers[0].gameObject.SetActive(false);
            lasers[1].gameObject.SetActive(true);
            anim.SetBool("power", true);
            anim.SetBool("blue", false);
        }
    }
}