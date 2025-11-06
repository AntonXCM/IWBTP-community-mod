using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRotate : MonoBehaviour
{
    private Transform player;
    private float rotateZ;
    [SerializeField] private Transform[] rotationObjects;
    void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();
    }
    void Update()
    {
        Vector3 difference = player.position - transform.position;
        rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        for (int i = 0; i < rotationObjects.Length; i++)
        {
            if (transform.position.x > player.position.x) rotationObjects[i].transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, -rotateZ - 180);
            else rotationObjects[i].transform.rotation = Quaternion.Euler(0, 0, rotateZ);
        }
        if (transform.position.x > player.position.x) transform.rotation = Quaternion.Euler(0, 180, 0);
        else transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
