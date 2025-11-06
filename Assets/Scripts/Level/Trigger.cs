using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct ObjActivate
{
    public GameObject obj;
    public bool active;
}
public class Trigger : MonoBehaviour
{
    [SerializeField] private Transform obj;
    [SerializeField] private bool withoutObj;
    [SerializeField] private Vector2 positionToMove;
    [SerializeField] private float speed;
    private bool triggerActive;
    [SerializeField] private ObjActivate[] objActivate;
    private AudioSource audioSource;
    [SerializeField] private bool stopAudioWhenTheEnd;
    [SerializeField] private float time;
    [SerializeField] private AudioMixerGroup audioMixer;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void FixedUpdate()
    {
        if (triggerActive) obj.position = Vector2.MoveTowards(obj.position, positionToMove, speed);
    }
    private void Update()
    {
        if (obj.position == new Vector3(positionToMove.x, positionToMove.y, 0) && objActivate.Length > 0 || withoutObj && triggerActive)
        {
            for (int i = 0; i < objActivate.Length; i++) objActivate[i].obj.SetActive(objActivate[i].active);
            if (stopAudioWhenTheEnd) audioSource.Stop();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        triggerActive = true;
        audioSource.Play();
        GetComponent<Collider2D>().enabled = false;
        Invoke(nameof(End), time);
        if (audioMixer) audioMixer.audioMixer.SetFloat("ostVolume", 0);
    }
    void End()
    {
        gameObject.SetActive(false);
    }
}
