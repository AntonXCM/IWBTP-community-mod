using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
enum TypeSecrets
{
    secretItem, secretActivator, detector
}
public class Items : MonoBehaviour
{
    [SerializeField] private TypeSecrets type;
    [SerializeField] private int numberOfSecret;
    [SerializeField] private Text text;
    [SerializeField] private bool active;
    [SerializeField] private int countOfSecrets;
    private int currentCount;

    [SerializeField] private AchieveSystem achieveSystem;

    private void Start()
    {
        if (PlayerPrefs.GetInt("SecretItem" + numberOfSecret) == 1 && type == TypeSecrets.secretActivator) gameObject.SetActive(active);
        if (type == TypeSecrets.secretItem) Item();
        if (type == TypeSecrets.detector) Detector();
    }
    private void Item()
    {
        if (PlayerPrefs.GetInt("SecretItem" + numberOfSecret) == 1) gameObject.SetActive(active);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageAble damageAble = collision.GetComponent<IDamageAble>();
        if (damageAble != null && PlayerPrefs.GetInt("SecretItem" + numberOfSecret) == 0 && type == TypeSecrets.secretItem)
        {
            PlayerPrefs.SetInt("SecretItem" + numberOfSecret, 1);
            GetComponent<AudioSource>().Play();
            GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
            if(text) text.GetComponent<Items>().Detector();
            achieveSystem.GetAchieve(30);
        }

    }
    public void Detector()
    {
        currentCount = 0;
        for(int i = 0; i < countOfSecrets; i++)
        {
            if (PlayerPrefs.GetInt("SecretItem" + (numberOfSecret + i)) == 1) currentCount++;
        }
        text.text = currentCount + "/" + countOfSecrets;
    }
}
