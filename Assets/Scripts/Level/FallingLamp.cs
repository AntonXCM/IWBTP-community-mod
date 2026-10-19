using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLamp : MonoBehaviour
{
    public IEnumerator FallDown(float distance, float speed)
    {
        while(distance > 0f)
        {
            float delta = Time.deltaTime * speed;
            distance -= Mathf.Abs(delta);
            transform.position += Vector3.up * delta;
            yield return new WaitForEndOfFrame();
        }
        GetComponent<Hideout>().TakeDamage();
    }
}
