using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
public class CoolSave : MonoBehaviour, IDamageAble
{
    [SerializeField] private int countOfKarma;
    [SerializeField] private new int animation = 0;
    [SerializeField] private GameObject[] fireObjects;
    [SerializeField] private Transform firePositions;
    private Animator anim;
    private Transform player;
    private float timeAttack;
    private float rotateZ;
    [SerializeField] private HitPoints hitPoints;
    [SerializeField] private BossSave bossSave;
    [SerializeField] private Vector2 spawnCurrent;
    public static SaveData loadedSaveData;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSaveData()
    {
        string text;
        try
        {
            text = File.ReadAllText(Path.Join(Application.persistentDataPath,"SaveData.JSON"));
            _ = text[0];
        } catch
        {
            text = "{}";
        }
        
        loadedSaveData = JsonConvert.DeserializeObject<SaveData>(text);
        SceneManager.activeSceneChanged += (_,_) => LoadSavables(); 
    }
    private static void LoadSavables()
    {
        if(SceneManager.GetActiveScene().name != "LevelParable") return;
        foreach(var i in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(j => j as ISavable).Where(j => j != null).OrderBy(j=>j.LoadPriority))
                i.LoadData(loadedSaveData);
    }
    private static void SaveSavables()
    {
        if(SceneManager.GetActiveScene().name != "LevelParable") return;
        foreach(var i in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(j => j as ISavable).Where(j => j != null).OrderBy(j=>j.LoadPriority))
                i.SaveData(ref loadedSaveData);
        File.WriteAllText(Path.Join(Application.persistentDataPath,"SaveData.JSON"), JsonConvert.SerializeObject(loadedSaveData));
    }
    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetInteger("Idle", animation);
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();

    }
    void Update()
    {
        Vector3 difference = player.position - transform.position;
        rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        if (countOfKarma > 0)
        {
            timeAttack += Time.deltaTime;
            anim.SetBool("Attack", true);
            if (timeAttack >= 0.3f)
            {
                for (int i = 0; i < fireObjects.Length; i++) Instantiate(fireObjects[i], firePositions.position, Quaternion.Euler(0, 0, rotateZ));
                countOfKarma--;
                timeAttack = 0.2f;
            }
        }
        else
        {
            anim.SetBool("Attack", false);
            timeAttack = 0;
        }
    }
    void IDamageAble.TakeDamage(int damage = 1)
    {
        PlayerPrefs.SetFloat("SaveX", player.position.x + spawnCurrent.x);
        PlayerPrefs.SetFloat("SaveY", player.position.y + spawnCurrent.y);
        PlayerPrefs.SetFloat("SaveZ", player.rotation.eulerAngles.y);
        PlayerPrefs.SetInt("SaveScene", SceneManager.GetActiveScene().buildIndex);
        SaveSavables();
        countOfKarma++;
        PlayerPrefs.SetInt("saveHit", PlayerPrefs.GetInt("saveHit") + 1);
        if (hitPoints && bossSave)
        {
            hitPoints.TakeDamage();
            bossSave.TakeDamage();
        }
    }
}

// Йоу, Кита, если ты это читаешь, я изобрёл крутую штуку.
// Кароч, ты можешь на любом объекте реализовать ISavable,
// а потом сохранять и загружать ЛЮБЫЕ ДАННЫЕ из SaveData!
// Каким-то случайным образом 4/4 строчек равны по длине:)

public interface ISavable
{
    void LoadData(SaveData data);
    void SaveData(ref SaveData data);
    public int LoadPriority { get;}
}

public class SaveData
{
	public Dictionary<string, (int, int)> StorySteps = new();
	public Dictionary<string, StoryBranchState> StoryBranches = new();
}