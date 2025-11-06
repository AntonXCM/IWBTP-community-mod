using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CoolSave : MonoBehaviour, IDamageAble
{
    [SerializeField] private int countOfKarma;
    [SerializeField] private GameObject[] fireObjects;
    [SerializeField] private Transform firePositions;
    private Animator anim;
    private Transform player;
    private float timeAttack;
    private float rotateZ;
    [SerializeField] private HitPoints hitPoints;
    [SerializeField] private BossSave bossSave;
    [SerializeField] private Vector2 spawnCurrent;
    private void Start()
    {
        anim = GetComponent<Animator>();
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
    void IDamageAble.TakeDamage()
    {
        PlayerPrefs.SetFloat("SaveX", player.position.x + spawnCurrent.x);
        PlayerPrefs.SetFloat("SaveY", player.position.y + spawnCurrent.y);
        PlayerPrefs.SetFloat("SaveZ", player.rotation.eulerAngles.y);
        PlayerPrefs.SetInt("SaveScene", SceneManager.GetActiveScene().buildIndex);
        countOfKarma++;
        PlayerPrefs.SetInt("saveHit", (PlayerPrefs.GetInt("saveHit") + 1));
        if (hitPoints && bossSave)
        {
            hitPoints.TakeDamage();
            bossSave.TakeDamage();
        }
    }
}
