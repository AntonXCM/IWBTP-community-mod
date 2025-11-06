using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWave : MonoBehaviour
{
    private KeyCode key;
    [SerializeField] private float rotate;
    private Rigidbody2D rb;
    [SerializeField] private float speed;
    public bool altGravity;
    void Start()
    {
        key = (KeyCode)PlayerPrefs.GetInt("Key" + 2);
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (altGravity == false)
        {
            if (Input.GetKey(key) ||
                    Input.GetKey(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
                    Input.GetKey(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2)
            {
                rotate = 45;
            }
            else
            {
                rotate = -45;
            }
        }
        else
        {
            if (Input.GetKey(key) ||
                    Input.GetKey(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
                    Input.GetKey(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2)
            {
                rotate = -45;
            }
            else
            {
                rotate = 45;
            }
        }
        transform.rotation = Quaternion.Euler(0, 0, rotate);
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speed, rotate * 3 * speed / 135);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        GetComponent<IDamageAble>().TakeDamage();
    }
}