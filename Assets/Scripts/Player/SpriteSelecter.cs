using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSelecter : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private float time;
    private int count;
    private void OnEnable()
    {
        time = 0;
        count = 1;
    }
    private void Update()
    {
        time += Time.deltaTime;
        if(time >= count / 12f)
        {
            if(count < sprites.Length)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[count];
                count++;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = sprites[0];
                time = 0;
                count = 1;
            }
        }
    }
    private void OnDisable()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[0];
    }
}
