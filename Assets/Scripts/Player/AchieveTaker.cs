using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
enum TypeOfAchieve
{
    zone, time, deadSave, deadCheck, deadReset, timeResetRoom, timeResetQuit
}
public class AchieveTaker : MonoBehaviour
{
    [SerializeField] private TypeOfAchieve type;
    [SerializeField] private AchieveSystem achieveSystem;
    [SerializeField] private int number;
    [SerializeField] private int time;
    private void Awake()
    {
        if(type == TypeOfAchieve.timeResetRoom) PlayerPrefs.SetInt("Speedrun", 0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && type == TypeOfAchieve.zone)
        {
            achieveSystem.GetAchieve(number);
        }
        if (player != null && type == TypeOfAchieve.deadSave)
        {
            PlayerPrefs.SetInt("NoDeadMode", PlayerPrefs.GetInt("Deaths"));
        }
        if (player != null && type == TypeOfAchieve.deadCheck && PlayerPrefs.GetInt("Deaths") == PlayerPrefs.GetInt("NoDeadMode"))
        {
            achieveSystem.GetAchieve(number);
        }
        if(player != null && type == TypeOfAchieve.deadReset)
        {
            PlayerPrefs.SetInt("NoDeadMode", -1);
        }
    }
    private void OnEnable()
    {
        if(type == TypeOfAchieve.time && PlayerPrefs.GetFloat("TimeOfThisFuckingSpeedrun") <= time && PlayerPrefs.GetInt("Speedrun") == 1)
        {
            achieveSystem.GetAchieve(number);
        }
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("Speedrun", 0);
    }
}
