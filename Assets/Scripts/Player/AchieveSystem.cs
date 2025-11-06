using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[SerializeField]
enum AchieveAction
{
    check, get, none
}
public class AchieveSystem : MonoBehaviour
{
    [SerializeField] private AchieveAction achieveAction;
    [SerializeField] private SpriteRenderer[] sprites;
    private AchieveInfo achieveInfo;
    private void Start()
    {
        if (achieveAction == AchieveAction.check) LoadAchieves();
        if (achieveAction == AchieveAction.get) achieveInfo = FindObjectOfType<AchieveInfo>();
    }
    public void Reload()
    {
        for(int i = 0; i < 50; i++)
        {
            PlayerPrefs.SetInt("achieve" + i, 0);
        }
        PlayerPrefs.DeleteKey("Deaths");
        PlayerPrefs.DeleteKey("TimeOfThisFuckingGame");
        PlayerPrefs.DeleteKey("Progress");
        for (int i = 1; i <= 30; i++) PlayerPrefs.DeleteKey("SecretItem" + i);
        PlayerPrefs.DeleteKey("SaveScene");
        PlayerPrefs.DeleteKey("saveDead");
        PlayerPrefs.DeleteKey("saveHit");
        PlayerPrefs.DeleteKey("saveDamage");
        SceneManager.LoadScene(0);
    }
    public void GetAchieve(int number)
    {
        if (PlayerPrefs.GetInt("achieve" + number) == 0 && achieveAction == AchieveAction.get)
        {
            if(achieveInfo)achieveInfo.TakeAchieve(number);
            PlayerPrefs.SetInt("achieve" + number, 1);
        }
    }
    public void LoadAchieves()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (PlayerPrefs.GetInt("achieve" + i) == 1) sprites[i].color = Color.white;
        }
    }
}
