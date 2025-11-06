using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private Text[] help;
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private float rotateY;
    [SerializeField] private int scene;
    [SerializeField] private AudioSource[] audioSource;
    [SerializeField] private int progress;
    [SerializeField] private bool secret;
    private int currentCount;
    [SerializeField] private int secretsCount;
    [SerializeField] private bool speedrun;
    private void Start()
    {
        if (PlayerPrefs.GetInt("ezMode") == 1) gameObject.SetActive(false);
        Secret();
        anim = GetComponent<Animator>();
        anim.SetBool("active", false);
        help[0].enabled = false;
        if (progress > PlayerPrefs.GetInt("Progress") || secret && currentCount != secretsCount) help[1].color = Color.red;
        else help[1].color = Color.green;
    }
    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton3)) && help[1].color == Color.green && help[0].enabled) Enter();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageAble damageAble = collision.GetComponent<IDamageAble>();
        if (damageAble != null && help[1].color == Color.green)
        {
            anim.SetBool("active", true);
            help[0].enabled = true;
            audioSource[0].Play();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        IDamageAble damageAble = collision.GetComponent<IDamageAble>();
        if (damageAble != null && help[1].color == Color.green)
        {
            anim.SetBool("active", false);
            help[0].enabled = false;
            audioSource[1].Play();
        }
    }
    private void Enter()
    {
        PlayerPrefs.SetFloat("SaveX", playerPosition.x);
        PlayerPrefs.SetFloat("SaveY", playerPosition.y);
        PlayerPrefs.SetFloat("SaveZ", rotateY);
        PlayerPrefs.SetInt("SaveScene", scene);
        PlayerPrefs.SetFloat("ostTime", 0);
        if (speedrun)
        {
            PlayerPrefs.SetInt("Speedrun", 1);
            PlayerPrefs.SetFloat("TimeOfThisFuckingSpeedrun", 0);
        }
        SceneManager.LoadScene(scene);
    }
    public void Progress()
    {
        if (progress > PlayerPrefs.GetInt("Progress")) help[1].color = Color.red;
        else help[1].color = Color.green;
        audioSource[2].Play();
    }
    public void Secret()
    {
        currentCount = 0;
        for (int i = 0; i < secretsCount; i++)
        {
            if (PlayerPrefs.GetInt("SecretItem" + (1 + i)) == 1) currentCount++;
        }
        if (currentCount == secretsCount && secret) help[2].enabled = false;
    }
}
