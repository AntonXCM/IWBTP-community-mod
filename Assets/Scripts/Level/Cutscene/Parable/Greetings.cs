using System;
using System.Collections;
using UnityEngine;

namespace ParablePlot
{   
    public class Greetings : StorytellerBase {
        [SerializeField] private ParableNarrator dialogue;
        [SerializeField] private EventTrigger startTrigger, cupTrigger, preShopTrigger, shopTrigger, coolestSave, iriskiKoritsaTrigger;
        [SerializeField] private GameObject meetingEnterDoor, meetingExitDoor, shopDoor;
        protected override string StoryId => "ParableGreetings";
        public override int LoadPriority => 1;

        protected override void BuildStorySequence()
        {
            meetingExitDoor.SetActive(true);
            meetingEnterDoor.SetActive(false);
            shopDoor.SetActive(false);
            iriskiKoritsaTrigger.gameObject.SetActive(false);
            AddTriggerStep(startTrigger,     _ => { dialogue.Subtitles.text = Localization.GetTextFormatted(fullKey:"p_greetings_1", Environment.UserName); meetingEnterDoor.SetActive(true); });
            AddDelayStep(4f,    _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_2"));
            AddDelayStep(4f,  _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_3"));
            AddDelayStep(0.6f,    _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_4"));
            AddTriggerStep(cupTrigger, _ =>
            {
                Destroy(cupTrigger.gameObject);
                dialogue.Subtitles.text = Localization.GetText("p_greetings_5");
                meetingExitDoor.SetActive(false);
            });
            AddRaceStep(new StoryBranch[] { 
                EventBranch(c => StartCoroutine(RealtimeDelayRoutine(25f, c)), _ => {}, BuildWaitSequence),
                EventBranch(c => preShopTrigger.Event += c, c => preShopTrigger.Event -= c, BuildShopSequence)
            });
            if(currentStoryStep == -1) storyStarted = true;
        }

        private void BuildShopSequence()
        {
            AddInstantStep(_ => dialogue.Subtitles.text = string.Empty);
            AddTriggerStep(shopTrigger,  _ => { shopDoor.SetActive(true); dialogue.Subtitles.text = Localization.GetText("p_greetings_6"); });
            AddDelayStep(2f, _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_7"));
            AddDelayStep(2f, _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_8"));
            AddTriggerStep(coolestSave, _=> {});
            AddDelayStep(2f, _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_9"));
        }
        private void BuildWaitSequence()
        {
            AddInstantStep(_ => dialogue.Subtitles.text = Localization.GetText("p_greetings_waitTooLong"));
            AddDelayStep(300, _ => { dialogue.Subtitles.text = Localization.GetText("p_greetings_waitLonger"); iriskiKoritsaTrigger.gameObject.SetActive(true); });
            AddTriggerStep(iriskiKoritsaTrigger, _ => { iriskiKoritsaTrigger.gameObject.SetActive(false); dialogue.Subtitles.text = Localization.GetText("p_greetings_waitLonger2"); });
            AddDelayStep(300, _ => dialogue.Subtitles.text = Localization.GetText("p_greetings_waitLonger"));
        }

        IEnumerator RealtimeDelayRoutine(float seconds, Action onComplete) {
            yield return new WaitForSecondsRealtime(seconds);
            onComplete?.Invoke();
        }
        protected void AddDelayStep(float seconds, Action<bool> executePhase) {
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