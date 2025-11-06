using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ShakeTimerBW : MonoBehaviour
{
    [SerializeField] private float strenght, time, timeFull;
    [SerializeField] private GameObject timers;
    private Text[] timeText;
    [SerializeField] private GameObject target;
    [SerializeField] private AudioSource ost;

    private void Awake()
    {
        time = PlayerPrefs.GetFloat("PIZZATIME");
    }
    private void Start()
    {
        InvokeRepeating(nameof(Shaking), 0, 0.07f);
        timeText = timers.GetComponentsInChildren<Text>();
    }
    private void Update()
    {
        string minutes = Mathf.Floor((time % 3600) / 60).ToString("00");
        string seconds = Mathf.Floor(time % 60).ToString("00");
        if(target.activeInHierarchy) time -= Time.deltaTime;
        foreach (var timer in timeText)
        {
            timer.text = minutes + ":" + seconds;
        }
        strenght = 2 * (1 - time / timeFull);
        PlayerPrefs.SetFloat("PIZZATIME", time);
        if(time < 180)
        {
            ost.pitch = 1 + 0.2f * (1f - time / 180f);
        }
        if (time <= 0.1f) SceneManager.LoadScene(14);
    }
    void Shaking()
    {
        transform.position = new Vector2(Random.Range(-strenght, strenght), Random.Range(-strenght, strenght));
    }
}
