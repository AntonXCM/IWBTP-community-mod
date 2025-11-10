using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
enum Option
{
    none, counterOfDeaths, counterOfTime, randomText
}
[System.Serializable]
struct TextMenu
{
    public string english;
    public string russian;
}
public class Translator : MonoBehaviour
{
    private bool russian;
    [SerializeField] private string[] text;
    [SerializeField] private Option option;
    [SerializeField] private float timeFloat;
    private int timeTenHours;
    //private Timer timeOnSMH;
    private string[] oInTimer;
    private int counterTime;
    [SerializeField] private TextMenu[] textMenu;
    private int randomText;

    [SerializeField] private AchieveSystem achieveSystem;
    [SerializeField] private bool speedrunMode;
    private void Start()
    {
        if (option == Option.randomText)
        {
            randomText = Random.Range(0, textMenu.Length);
            text[0] = textMenu[randomText].english;
            text[1] = textMenu[randomText].russian;
        }
        if (option == Option.counterOfTime)
        {
            oInTimer = new string[3];
            if (speedrunMode == false) timeFloat = PlayerPrefs.GetFloat("TimeOfThisFuckingGame");
            else if (PlayerPrefs.GetInt("Speedrun") == 1) timeFloat = PlayerPrefs.GetFloat("TimeOfThisFuckingSpeedrun");
            else gameObject.SetActive(false);
            counterTime = 1;
        }
        OnEnable();
    }
    private void OnEnable()
    {
        ChangeLanguage();
    }
    private void Update()
    {
        if (option == Option.counterOfDeaths)
        {
            if (russian)
            {
                if (PlayerPrefs.GetInt("Deaths") < 1000000000) GetComponent<Text>().text = text[1] + ": " + PlayerPrefs.GetInt("Deaths");
                else if (PlayerPrefs.GetInt("Deaths") == 1000000000) GetComponent<Text>().text = text[1] + ": ��������";
                else GetComponent<Text>().text = text[1] + ": ��������+";
            }
            else
            {
                if (PlayerPrefs.GetInt("Deaths") < 1000000000) GetComponent<Text>().text = text[0] + ": " + PlayerPrefs.GetInt("Deaths");
                else if (PlayerPrefs.GetInt("Deaths") == 1000000000) GetComponent<Text>().text = text[0] + ": billion";
                else GetComponent<Text>().text = text[0] + ": billion+";
            }
            if (PlayerPrefs.GetInt("Deaths") >= 1) achieveSystem.GetAchieve(1);
            if (PlayerPrefs.GetInt("Deaths") >= 100) achieveSystem.GetAchieve(2);
            if (PlayerPrefs.GetInt("Deaths") >= 500) achieveSystem.GetAchieve(3);
            if (PlayerPrefs.GetInt("Deaths") >= 1000) achieveSystem.GetAchieve(4);
            if (PlayerPrefs.GetInt("saveDead") >= 28) achieveSystem.GetAchieve(5);
            if (PlayerPrefs.GetInt("saveHit") >= 427) achieveSystem.GetAchieve(6);
            if (PlayerPrefs.GetInt("saveDamage") >= 10000) achieveSystem.GetAchieve(31);
        }
        if(option == Option.counterOfTime)
        {
            timeFloat += Time.deltaTime;
            if(timeFloat > 36000)
            {
                timeFloat -= 36000;
                timeTenHours++;
            }
            string hours = Mathf.Floor((timeTenHours * 36000 + timeFloat) / 3600).ToString("00");
            string minutes = Mathf.Floor(((timeTenHours * 36000 + timeFloat) % 3600) / 60).ToString("00");
            string seconds = Mathf.Floor((timeTenHours * 36000 + timeFloat) % 60).ToString("00");
            GetComponent<Text>().text = hours + ":" + minutes + ":" + seconds + "";
            if (speedrunMode == false) PlayerPrefs.SetFloat("TimeOfThisFuckingGame", (timeTenHours * 36000 + timeFloat));
            else if (PlayerPrefs.GetInt("Speedrun") == 1) PlayerPrefs.SetFloat("TimeOfThisFuckingSpeedrun", (timeTenHours * 36000 + timeFloat));
            if (PlayerPrefs.GetFloat("TimeOfThisFuckingGame") >= 60 * 60) achieveSystem.GetAchieve(7);
            if (PlayerPrefs.GetFloat("TimeOfThisFuckingGame") >= 60 * 60 * 10) achieveSystem.GetAchieve(8);
            if (PlayerPrefs.GetFloat("TimeOfThisFuckingGame") >= 60 * 60 * 5) achieveSystem.GetAchieve(9);
        }
    }
    public void ChangeLanguage()
    {
        if (PlayerPrefs.GetInt("Language") == 1) russian = true;
        else russian = false;
        if (option == Option.none || option == Option.randomText)
        {
            if (russian) GetComponent<Text>().text = ProcessKeywords(text[1]);
            else GetComponent<Text>().text = ProcessKeywords(text[0]);
        }
    }
    public string ProcessKeywords(string str)
    {
        str = str.Replace("{USERNAME}", System.Environment.UserName);
        return str;
    }
}
