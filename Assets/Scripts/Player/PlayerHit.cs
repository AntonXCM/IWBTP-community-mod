using System;
using UnityEngine;

public class PlayerHit : MonoBehaviour, IDamageAble
{
    public static PlayerHit instance;
    public void Awake() => instance = this;

    public static event Action<Vector2> OnDeath; 
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
    void IDamageAble.TakeDamage(int damage = 1)
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
    public void Damage()
    {
        PlayerPrefs.SetInt("Deaths", PlayerPrefs.GetInt("Deaths") + 1);
        Instantiate(effect, transform.position, Quaternion.Euler(0, 0, 0));
        OnDeath?.Invoke(transform.position);
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
