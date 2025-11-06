using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour, IDamageAble
{
    [SerializeField] private GameObject effect;
    [SerializeField] private GameObject gameOver;
    bool active;
    [SerializeField] private GameObject[] objectsOff;
    [SerializeField] private GameObject[] objectsOn;
    [SerializeField] private bool dead;
    [SerializeField] private float lefthere;
    [SerializeField] private float righthere;
    void Update()
    {
        if (dead && (righthere < transform.position.x || lefthere > transform.position.x)) Damage();
    }
    void IDamageAble.TakeDamage()
    {
        if (active == false)
        {
            Damage();
        }
    }
    private void OnParticleCollision(GameObject other)
    {
        Damage();
    }
    void Damage()
    {
        PlayerPrefs.SetInt("Deaths", PlayerPrefs.GetInt("Deaths") + 1);
        Instantiate(effect, transform.position, Quaternion.Euler(0, 0, 0));
        foreach (var objectOff in objectsOff)
        {
            objectOff.SetActive(false);
        }
        foreach (var objectOn in objectsOn)
        {
            objectOn.SetActive(true);
        }
        gameOver.SetActive(true);
        gameObject.SetActive(false);
        active = true;
    }
}
