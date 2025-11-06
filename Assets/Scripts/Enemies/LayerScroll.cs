using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerScroll : MonoBehaviour
{
    private SpriteRenderer sprite;
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        sprite.sortingOrder--;
    }
}
