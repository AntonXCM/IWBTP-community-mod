using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    float rotation = 0;
    [SerializeField] float speed = Mathf.PI;
    void FixedUpdate()
    {
        rotation += Time.fixedDeltaTime * speed;
        transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
    }
}
