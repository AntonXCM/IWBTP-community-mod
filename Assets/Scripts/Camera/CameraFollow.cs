using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct CameraMode
{
    public Vector2 position;
    public bool flipX, flipY;
    public float rotateSpeed;
}
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    [SerializeField] private CameraMode[] cameraMode;
    [SerializeField] private bool saveDisable;
    private Vector2 savePosition;
    private void Awake()
    {
        if (saveDisable == false)
        {
            transform.position = new Vector3(PlayerPrefs.GetFloat("CameraX"), PlayerPrefs.GetFloat("CameraY"), -10);
            savePosition = transform.position;
        }
    }
    private void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        Move(0, 0);
    }
    private void Update()
    {
        if (transform.position.x - player.position.x > 240)
        {
            Move(-480, 0);
        }
        if (player.position.x - transform.position.x > 240)
        {
            Move(480, 0);
        }
        if (transform.position.y - player.position.y > 135)
        {
            Move(0, -270);
        }
        if (player.position.y - transform.position.y > 135)
        {
            Move(0, 270);
        }
    }
    private void Move(float moveX, float moveY)
    {
        transform.position = new Vector3(transform.position.x + moveX, transform.position.y + moveY, -10);
        savePosition = new Vector3(savePosition.x + moveX, savePosition.y + moveY, -10);
        PlayerPrefs.SetFloat("CameraX", savePosition.x);
        PlayerPrefs.SetFloat("CameraY", savePosition.y);
        for(int i = 0; i < cameraMode.Length; i++)
        {
            if(cameraMode[i].position.x == savePosition.x && cameraMode[i].position.y == savePosition.y)
            {
                Mode(i);
            }
        }
    }
    void Mode(int number)
    {
        if (cameraMode[number].flipX && cameraMode[number].flipY == false)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            transform.position = new Vector3(transform.position.x, transform.position.y, 10);
        }
        else if(cameraMode[number].flipY && cameraMode[number].flipX == false)
        {
            transform.rotation = Quaternion.Euler(180, 0, 0);
            transform.position = new Vector3(transform.position.x, transform.position.y, 10);
        }
        else if (cameraMode[number].flipY && cameraMode[number].flipX)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
    }
}
