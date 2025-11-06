using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AchieveInfo : MonoBehaviour
{
    private static GameObject instance;
    [SerializeField] private GameObject[] achieves;
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = gameObject;
        else
            Destroy(gameObject);
    }
    public void TakeAchieve(int number)
    {
        CancelInvoke();
        Off();
        achieves[number].SetActive(true);
        Invoke(nameof(Off), 6f);
    }
    void Off()
    {
        foreach (var achieve in achieves)
        {
            achieve.SetActive(false);
        }
    }
}
