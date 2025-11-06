using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HitPoints : MonoBehaviour, IDamageAble
{
    [SerializeField] private int hp;
    [SerializeField] private float maxHp;
    [SerializeField] private Image bar;
    [SerializeField] private Material[] materials;
    [SerializeField] private GameObject[] effects;
    [SerializeField] private bool effect;
    [SerializeField] private ObjActivate[] objActivates;
    private OstSpeedHit ostSpeed;
    [SerializeField] SpriteRenderer[] spriteFix;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private Boss5 damageAble;
    void Start()
    {
        if (PlayerPrefs.GetInt("ezMode") == 1) hp /= 2;
        if(maxHp == 0) maxHp = hp;
        if (bar)
        {
            GetUI();
            ResetMaterial();
        }
        if (GetComponent<OstSpeedHit>()) ostSpeed = GetComponent<OstSpeedHit>();
    }
    void IDamageAble.TakeDamage()
    {
        TakeDamage();
        if (damageAble != null) damageAble.TakeDamage();
    }
    void GetUI()
    {
        bar.fillAmount = hp * 1f / maxHp;
    }
    void ResetMaterial()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        if (spriteFix.Length > 0)
        {
            for (int i = 0; i < spriteFix.Length; i++) spriteFix[i].material = materials[0];
        }
        else
        {
            for (int i = 0; i < sprites.Length; i++) sprites[i].material = materials[0];
        }
        if (bar && hp < maxHp/2)
        {
            if(hp * 1.2f / maxHp > 0.12f) Invoke(nameof(Indicator), hp * 1.2f / maxHp);
            else Invoke(nameof(Indicator), 0.12f);
        }
    }
    void Indicator()
    {
        sfx.Play();
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        if (spriteFix.Length > 0)
        {
            for (int i = 0; i < spriteFix.Length; i++) spriteFix[i].material = materials[2];
        }
        else
        {
            for (int i = 0; i < sprites.Length; i++) sprites[i].material = materials[2];
        }
        if(hp * 0.2f / maxHp > 0.07f) Invoke(nameof(ResetMaterial), hp * 0.2f / maxHp);
        else Invoke(nameof(ResetMaterial), 0.07f);
    }
    public void TakeDamage()
    {
        hp--;
        if (bar) GetUI();
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        if (spriteFix.Length > 0)
        {
            for (int i = 0; i < spriteFix.Length; i++) spriteFix[i].material = materials[1];
        }
        else
        {
            for (int i = 0; i < sprites.Length; i++) sprites[i].material = materials[1];
        }
        CancelInvoke();
        Invoke(nameof(ResetMaterial), 0.03f);
        GetComponent<AudioSource>().Play();
        if (hp <= 0)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effect == false) Instantiate(effects[i], transform.position, transform.rotation);
                else Instantiate(effects[i], transform.position, Quaternion.Euler(0, 0, 0));
            }
            for (int i = 0; i < objActivates.Length; i++) objActivates[i].obj.SetActive(objActivates[i].active);
            Destroy(gameObject);
        }
        if (ostSpeed) ostSpeed.Take();
    }
}
