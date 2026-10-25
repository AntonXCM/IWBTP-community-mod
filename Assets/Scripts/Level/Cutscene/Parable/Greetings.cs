using System;
using System.Collections;
using UnityEngine;

namespace ParablePlot
{   
    public class Greetings : StorytellerBase {
        [SerializeField] private int peremotka = 0;
        [SerializeField] private ParableNarrator dialogue;
        [SerializeField] private FallingLamp lamp;
        [SerializeField] private Transform chair, evilChair;
        [SerializeField] private GameObject door;
        [SerializeField] private Shadow shadow;
        [SerializeField] private SpriteRenderer[] shadows2, shadows3;
        [SerializeField] private GameObject[] stanleycases;
        [SerializeField] private EventTrigger exitTrigger, invisibleWall;

        protected override string StoryId => "ParableIntro";

        protected override void BuildStorySequence()
        {
            foreach(var s in stanleycases) s.SetActive(false);
            AddDelayStep(0,     _ => dialogue.Subtitles.text = Localization.GetTextFormatted(fullKey:"p_intro_1", Environment.UserName));
            AddDelayStep(3f,    _ => dialogue.Subtitles.text = Localization.GetTextFormatted(fullKey:"p_intro_2", Environment.UserName));
            AddDelayStep(3.5f,  _ => dialogue.Subtitles.text = Localization.GetText("p_intro_3"));
            AddDelayStep(6f,    _ => dialogue.Subtitles.text = Localization.GetText("p_intro_4"));
            AddDelayStep(10f,   _ =>
            {
                if(lamp)
                {
                    dialogue.Subtitles.text = Localization.GetTextFormatted(fullKey:"p_intro_5b", Environment.UserName);  
                    StartCoroutine(lamp.FallDown(50, -100));
                    if ((FindAnyObjectByType<PlayerController>().transform.position - chair.position).sqrMagnitude > 1000)
                    {
                        Instantiate(evilChair, chair.position, Quaternion.identity);
                        Destroy(chair.gameObject);
                    }
                } else {
                    dialogue.Subtitles.text = Localization.GetTextFormatted(fullKey:"p_intro_5a", Environment.UserName);
                }
            });
            AddDelayStep(4.5f,    _ => {
                dialogue.Subtitles.text = Localization.GetText("p_intro_6");
                Destroy(door);
                Destroy(shadow);
                foreach(var s in shadows2) { s.gameObject.SetActive(true); s.color = new(s.color.r,s.color.g,s.color.b, 1f); }
                foreach(var s in shadows3) Destroy(s);
                foreach(var s in stanleycases) s.SetActive(true);
            });
            AddTriggerStep(exitTrigger, _ =>
            {
                dialogue.Subtitles.text = Localization.GetTextFormatted(fullKey:"p_intro_7", Environment.UserName);
                invisibleWall.gameObject.SetActive(true);
            });
            AddTriggerStep(invisibleWall, _ =>
            {
                dialogue.Subtitles.text = Localization.GetText("p_intro_wall");
            });
            storyStarted = true;
            currentStoryStep = peremotka;
        }
        protected void AddDelayStep(float seconds, Action<bool> executePhase) {
            IEnumerator RealtimeDelayRoutine(float seconds, Action onComplete) {
                yield return new WaitForSecondsRealtime(seconds);
                onComplete?.Invoke();
            }
            AddStep(
                subscribe: (callback) => StartCoroutine(RealtimeDelayRoutine(seconds, callback)),
                unsubscribe: _=>{},
                executePhase: executePhase
            );
        }

        protected void AddTriggerStep(EventTrigger trigger, Action<bool> executePhase)
        {
            AddStep(
                subscribe: (callback) => trigger.Event += callback,
                unsubscribe: (callback) => trigger.Event -= callback,
                executePhase: executePhase
            );
        }
	}
}