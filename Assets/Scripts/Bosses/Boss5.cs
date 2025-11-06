using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Boss5 : MonoBehaviour
{
    private Animator anim;
    [SerializeField] float speed = 1, rotate, fireRate;
    [SerializeField] private GameObject bullet, exBullet;
    private int phase = 1;
    [SerializeField] private int attacks;
    [SerializeField] private Transform player;
    [SerializeField] private ParticleSystem particle;
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Next()
    {
        if (speed < 1.8f)
        {
            phase += Random.Range(1, attacks);
            if (phase > attacks)
            {
                phase -= attacks;
            }
        }
        else
        {
            phase = 77;
        }
        if (player.gameObject.activeInHierarchy) anim.Play("Attack" + phase);
    }
    void Final()
    {
        if (player.gameObject.activeInHierarchy) anim.Play("AttackFinal");
    }
    void FireRandom()
    {
        Instantiate(bullet, transform.position, Quaternion.Euler(0,0,Random.Range(0,360)));
    }
    void FireAim()
    {
        Vector3 difference = player.position - transform.position;
        var rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        Instantiate(bullet, transform.position, Quaternion.Euler(0, 0, rotateZ));
    }
    void FireRotate()
    {
        Instantiate(bullet, transform.position, Quaternion.Euler(0, 0, rotate));
    }
    void FireExtra()
    {
        Instantiate(exBullet, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
    }
    void FireMinigunRotate()
    {
        InvokeRepeating(nameof(FireRotate), 0, fireRate/speed);
    }
    void FireMinigunAim()
    {
        InvokeRepeating(nameof(FireAim), 0, fireRate/speed);
    }
    void FireEnd()
    {
        CancelInvoke();
    }
    public void TakeDamage()
    {
        speed += 0.004f;
        anim.speed = speed;
        transform.localScale = new Vector3(3f / speed, 3f / speed, 3f / speed);
        particle.playbackSpeed = speed - 0.3f;
        particle.emissionRate += 0.3f;
    }
}
