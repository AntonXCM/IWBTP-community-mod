using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToPoint : MonoBehaviour
{
    [SerializeField] private Vector2[] points;
    [SerializeField] private float speed;
    private int numberPoint;
    [SerializeField] private bool spawnReturn;
    [SerializeField] private Vector2 pos;
    private void Start()
    {
        if (spawnReturn) pos = transform.position;
    }
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, points[numberPoint], speed * Time.deltaTime);
        if (transform.position.x == points[numberPoint].x && transform.position.y == points[numberPoint].y)
        {
            if (numberPoint < points.Length - 1) numberPoint++;
            else numberPoint = 0;
        }
    }
    private void OnEnable()
    {
        if (spawnReturn && (pos.x != 0 || pos.y != 0)) transform.position = pos;
    }
}
