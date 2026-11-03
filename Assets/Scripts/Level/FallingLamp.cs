using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLamp : MonoBehaviour
{
    [SerializeField] float Distance, Speed;
    public IEnumerator FallDown() =>FallDown(Distance, Speed);
    public IEnumerator FallDown(float distance, float speed)
    {
        while(distance > 0f)
        {
            float delta = Time.deltaTime * speed;
            distance -= Mathf.Abs(delta);
            transform.position += Vector3.up * delta;
            yield return new WaitForEndOfFrame();
        }
        GetComponent<Hideout>().TakeDamage(99);
    }
}
