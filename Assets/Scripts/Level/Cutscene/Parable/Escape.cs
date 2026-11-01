using System;
using System.Collections;
using UnityEngine;

namespace ParablePlot
{   
    public class Escape : StorytellerBase {
        [SerializeField] private ParableNarrator dialogue;
        protected override string StoryId => "ParableEscape";
        public override int LoadPriority => 2;

        protected override void BuildStorySequence()
        {
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