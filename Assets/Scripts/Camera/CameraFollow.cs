using System;
using UnityEngine;

[Serializable]
struct CameraMode
{
    public Vector2 position;
    public bool flipX, flipY;
    public float rotateSpeed;
}

public class CameraFollow : MonoBehaviour
{
    // Размер одного экрана в юнитах
    public const int ScreenWidth = 480;
    public const int ScreenHeight = 270;
    public const float Aspect = (float)ScreenWidth / (float)ScreenHeight;
    private float screenAspect = 0;
    public Transform player;
    [SerializeField] private CameraMode[] cameraMode;
    [SerializeField] private bool saveDisable;

    private Vector2 savePosition;
    public event Action OnMove;
    public Vector2Int CurrentScreen { get; private set; }

    private void Awake()
    {
        if (saveDisable == false)
        {
            transform.position = new Vector3(PlayerPrefs.GetFloat("CameraX"), PlayerPrefs.GetFloat("CameraY"), -10);
        }
        savePosition = transform.position;
        CurrentScreen = WorldToScreen(savePosition);
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        Move(0, 0);
    }

    private void Update()
    {
        float current = (float)Screen.width / Screen.height;
        if (current != screenAspect)
        {
            if (current > Aspect)
            {
                float width = Aspect / current;
                Camera.main.rect = new Rect((1 - width) / 2, 0, width, 1);
            }
            else
            {
                float height = current / Aspect;
                Camera.main.rect = new Rect(0, (1 - height) / 2, 1, height);
            }
            screenAspect = current;
        }
        
        if (transform.position.x - player.position.x > ScreenWidth / 2f)
        {
            Move(-ScreenWidth, 0);
        }
        if (player.position.x - transform.position.x > ScreenWidth / 2f)
        {
            Move(ScreenWidth, 0);
        }
        if (transform.position.y - player.position.y > ScreenHeight / 2f)
        {
            Move(0, -ScreenHeight);
        }
        if (player.position.y - transform.position.y > ScreenHeight / 2f)
        {
            Move(0, ScreenHeight);
        }
    }

    /// <summary>Переводит мировую позицию в координаты экрана.</summary>
    public static Vector2Int WorldToScreen(Vector2 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / ScreenWidth),
            Mathf.RoundToInt(worldPosition.y / ScreenHeight));
    }

    /// <summary>Переводит координаты экрана в мировую позицию его центра.</summary>
    public static Vector2 ScreenToWorld(Vector2Int screen)
    {
        return new Vector2(screen.x * ScreenWidth, screen.y * ScreenHeight);
    }

    protected virtual void Move(float moveX, float moveY)
    {
        transform.position = new Vector3(transform.position.x + moveX, transform.position.y + moveY, -10);
        savePosition = new Vector2(savePosition.x + moveX, savePosition.y + moveY);
        CurrentScreen = WorldToScreen(savePosition);

        PlayerPrefs.SetFloat("CameraX", savePosition.x);
        PlayerPrefs.SetFloat("CameraY", savePosition.y);

        for (int i = 0; i < cameraMode.Length; i++)
        {
            if (cameraMode[i].position == savePosition)
            {
                Mode(i);
            }
        }
        OnMove?.Invoke();
    }

    private void Mode(int number)
    {
        if (cameraMode[number].flipX && cameraMode[number].flipY == false)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            transform.position = new Vector3(transform.position.x, transform.position.y, 10);
        }
        else if (cameraMode[number].flipY && cameraMode[number].flipX == false)
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