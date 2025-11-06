using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    private KeyCode key;
    [SerializeField] private float rotate;
    private Rigidbody2D rb;
    [SerializeField] private float speed;
    private float timer;
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
                if (rotate < 45) rotate += Time.deltaTime * 111;
                else rotate = 45;
            }
            else
            {
                if (rotate > -45) rotate -= Time.deltaTime * 111;
                else rotate = -45;
            }
        }
        else
        {
            if (Input.GetKey(key) ||
                    Input.GetKey(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
                    Input.GetKey(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2)
            {
                if (rotate > -45) rotate -= Time.deltaTime * 111;
                else rotate = -45;
            }
            else
            {
                if (rotate < 45) rotate += Time.deltaTime * 111;
                else rotate = 45;
            }
        }
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer >= 1) GetComponent<IDamageAble>().TakeDamage();
        }
        else
        {
            timer = 0;
        }
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speed, rotate*3);
        if(altGravity == false) transform.rotation = Quaternion.Euler(0, transform.rotation.y, rb.velocity.y / 3);
        else transform.rotation = Quaternion.Euler(180, transform.rotation.y, rb.velocity.y / -3);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        timer += Time.deltaTime * 3;
    }
    private void OnEnable()
    {
        rotate = 0;
    }
}
