using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float speed;
    [SerializeField] private float randomSpeed;
    [SerializeField] private float time;
    [SerializeField] private GameObject effect;
    [SerializeField] private bool explosion;
    [SerializeField] private bool saveBullet;
    [SerializeField] private bool playerBullet;
    [SerializeField] private bool noDeadBullet;
    [SerializeField] private AudioSource sound;

    private void Start()
    {
        if (time != 0)
            Invoke(nameof(Dead), time);
        speed += Random.Range(-randomSpeed, randomSpeed);
    }
    private void FixedUpdate() => transform.Translate(Vector2.right * Time.fixedDeltaTime * speed);
    private void OnCollisionEnter2D(Collision2D collision) => OnTriggerEnter2D(collision.collider);
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool damaged = false;
        foreach (var damageAble in collision.GetComponents<IDamageAble>())
        {
            damaged = true;
            damageAble.TakeDamage(_damage);
            if (saveBullet) PlayerPrefs.SetInt("saveDead", PlayerPrefs.GetInt("saveDead") + 1);
            if (playerBullet) PlayerPrefs.SetInt("saveDamage", PlayerPrefs.GetInt("saveDamage") + 1);
        }
        if(damaged && sound) sound.Play();
        if((!collision.TryGetComponent<PreserveBullet>(out _)) && (time != 0) && (noDeadBullet == false))
            Dead();
    }
    private void Dead()
    {
        if (effect)
        {
            if (explosion == false) Instantiate(effect, transform.position, transform.rotation);
            else Instantiate(effect, transform.position, Quaternion.Euler(0, 0, 0));
        }
        Destroy(gameObject);
    }
}
