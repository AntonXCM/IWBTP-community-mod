using System.Collections.Generic;
using UnityEngine;
//Code contibuted by AntonXCM!
public class Shadow : MonoBehaviour
{
    [SerializeField] private List<GameObject> lights;
    private int lightsCount;
    [SerializeField] private SpriteRenderer[] shadows;
    [SerializeField] private float shadowingSpeed;
    private float shadowA;
    private void Start()
    {
        lightsCount = lights.Count;
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < lightsCount;)
            if (!lights[i])
            {
                lights.RemoveAt(i);
                lightsCount--;
                if (lightsCount is 0)
                {
                    foreach (var shadow in shadows)
                        shadow.gameObject.SetActive(true);
                    return;
                }
            }
            else i++;
    }
    private void Update()
    {
        if (lightsCount != 0) return;
        shadowA += Time.deltaTime * shadowingSpeed;
        if (shadowA > 1) return;
        foreach (var shadow in shadows)
        {
            shadow.gameObject.SetActive(true);
            Color color = shadow.color;
            color.a = shadowA;
            shadow.color = color;
        }
    }
}
