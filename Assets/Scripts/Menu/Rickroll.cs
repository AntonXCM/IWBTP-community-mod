using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rickroll : MonoBehaviour
{
    private bool canSkip;
    [SerializeField] private GameObject text;
    [SerializeField] private Transform[] penguin;
    private float time;
    private void Start()
    {
        Invoke(nameof(Can), Random.Range(9f, 18f));
        InvokeRepeating(nameof(Pen), 0, 1);
    }
    private void Update()
    {
        time += Time.deltaTime;
        if (Input.anyKeyDown && canSkip || time >= 120)
        {
            Invoke(nameof(Scene), 1.2f);
            GetComponent<Animator>().SetTrigger("y");
        }
    }
    private void Can()
    {
        canSkip = true;
        text.SetActive(true);
    }
    void Scene()
    {
        SceneManager.LoadScene(0);
    }
    void Pen()
    {
        penguin[0].localScale = new Vector3(penguin[0].localScale.x * -1, 1, 1);
        penguin[1].rotation = Quaternion.Euler(0, 0, Random.Range(-90f, 0) * penguin[0].localScale.x);
        penguin[2].rotation = Quaternion.Euler(0, 0, Random.Range(-90f, 0) * penguin[0].localScale.x);
    }
}