using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimBullet : MonoBehaviour
{
    private float rotate;
    [SerializeField] private float speed;
    private Transform target;
    private float rotateZ;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform noRotate;
    private void Start()
    {
        rotate = transform.eulerAngles.z;
        target = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        if (sprites.Length > 0) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if(noRotate) noRotate.rotation = Quaternion.identity;
        Vector3 difference = target.position - transform.position + offset;
        rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        if (rotateZ > rotate) rotate += Time.deltaTime * speed;
        if (rotate > 0 && rotate - rotateZ > 180) rotate -= 360;
        if (rotateZ < rotate) rotate -= Time.deltaTime * speed;
        if (rotate < 0 && rotateZ - rotate > 180) rotate += 360;
        transform.rotation = Quaternion.Euler(0, 0, rotate);
        if (sprites.Length > 0)
        {
            if (target.position.x > transform.position.x)
            {
                spriteRenderer.sprite = sprites[0];
                spriteRenderer.flipY = false;
            }
            if (target.position.x < transform.position.x)
            {
                spriteRenderer.sprite = sprites[1];
                spriteRenderer.flipY = true;
            }
        }
    }
}
