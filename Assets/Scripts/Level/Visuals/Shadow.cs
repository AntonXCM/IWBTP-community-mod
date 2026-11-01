using System.Collections.Generic;
using UnityEngine;
//Code contibuted by AntonXCM!
public class Shadow : MonoBehaviour
{
    [SerializeField] private List<GameObject> lights;
    [SerializeField] private bool invert = false;
    private int lightsCount;
    [SerializeField] private SpriteRenderer[] shadows;
    [SerializeField] private float shadowingSpeed;
    private float shadowA = 0;
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
                shadowA = 0;
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
        if (shadowA > 1) return;
        shadowA += Time.deltaTime * shadowingSpeed;
        foreach (var shadow in shadows)
        {
            shadow.gameObject.SetActive(true);
            Color color = shadow.color;
            color.a = invert ? 1 - shadowA : shadowA;
            shadow.color = color;
        }
    }
}
