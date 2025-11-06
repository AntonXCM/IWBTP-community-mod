using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    private KeyCode key;
    [SerializeField] private float gravity;
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
            rb.gravityScale = gravity;
            transform.Rotate(new Vector3(0, 0, -12 * speed * Time.deltaTime));
        }
        else
        {
            rb.gravityScale = -gravity;
            transform.Rotate(new Vector3(0, 0, 12 * speed * Time.deltaTime));
        }
        if ((Input.GetKeyDown(key) ||
                    Input.GetKeyDown(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
                    Input.GetKeyDown(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2) && rb.velocity.y < 1 && rb.velocity.y > -1)
        {
            altGravity = !altGravity;
            rb.velocity = new Vector2(rb.velocity.x, 7 * rb.gravityScale);
        }
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speed, rb.velocity.y);
    }
}
