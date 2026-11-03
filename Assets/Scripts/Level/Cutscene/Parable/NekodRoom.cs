using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NekodRoom : MonoBehaviour
{
    public EventTrigger triggerEnter, triggerPortal;
    public GameObject portalDucks, portalExit;
    public Text narrator, nekod;
    string[] texts;
    public void Start()
    {
        portalDucks.SetActive(false);
        portalExit.SetActive(false);
        texts = Localization.GetTextArray("p_nekod");
        triggerEnter.Event += () => narrator.text = texts[0];
        triggerPortal.Event += () => StartCoroutine(Play());
    }
    public IEnumerator Play()
    {
        Destroy(triggerPortal.gameObject);
        nekod.text = texts[1];
        narrator.text = texts[2];
        yield return new WaitForSeconds(1.5f);
        nekod.text = texts[3];
        yield return new WaitForSeconds(2f);
        narrator.text = texts[4];
        yield return new WaitForSeconds(1.5f);
        nekod.text = texts[5];
        yield return new WaitForSeconds(1.4f);
        narrator.text = texts[6];
        yield return new WaitForSeconds(1f);
        narrator.text = texts[7];
        yield return new WaitForSeconds(2f);
        portalDucks.SetActive(true);
        nekod.text = texts[8];
        yield return new WaitForSeconds(2.5f);
        nekod.text = texts[9];
        yield return new WaitForSeconds(2f);
        narrator.text = texts[10];
        yield return new WaitForSeconds(4f);
        nekod.text = texts[11];
        yield return new WaitForSeconds(5f);
        narrator.text = texts[12];
        yield return new WaitForSeconds(4f);
        narrator.text = texts[13];
        yield return new WaitForSeconds(4f);
        nekod.text = texts[14];
        yield return new WaitForSeconds(2f);
        portalDucks.SetActive(false);
        portalExit.SetActive(true);
    }
}