     /////// 
  ////     ////.    =================================
 ///          ///   Скрипты сделаны AntonXCM для проекта
//     /===,    //  The Sailor & his Garden и исключительно
/*    (         "/  предоставлены Kitket427 для использования
//     \==="    //  в любых проектах в качестве подарка.
 "//          ///   
  ////      ///*    Могу дать еще, мне не жалко ;)
     *//////"

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StorytellerBase : MonoBehaviour {
	public void BeginStory() {
        storyStarted = true;
		currentStoryStep = 0;
        StartCurrentStepWait();
    }
	public void Start() => LoadData();
	public virtual void LoadData() {
		storyStarted = currentStoryStep != -1;
		
		BuildStorySequence();
		for (int i = 0; i < currentStoryStep && i < storySequence.Count; i++)
			storySequence[i].ExecutePhase?.Invoke(true);
		
		LoadCustomState();
		if(storyStarted) StartCurrentStepWait();
	}
	protected virtual void LoadCustomState() { }

	protected abstract void BuildStorySequence();

	protected abstract string StoryId { get; }
	
	protected bool storyStarted = false;
	protected int currentStoryStep = -1;
	protected int stepEventCounter = 0;

	private readonly List<StoryStep> storySequence = new();
	private Action currentUnsubscribe;

	#region Steps

	protected class StoryStep {
		public Action<Action> Subscribe;
		public Action<Action> Unsubscribe;

		public Func<int> GetTargetCount;
		public Func<bool> CheckCondition; 

		public Func<bool> SkipCondition;
		public Action<bool> ExecutePhase;
		public StoryStep SkipBy(Func<bool> condition) {
			SkipCondition = condition;
			return this;
		} 
	}

	protected StoryStep AddStep(Action<Action> subscribe, Action<Action> unsubscribe, Action<bool> executePhase, Func<int> getTargetCount = null) {
		var step = new StoryStep {
			Subscribe = subscribe,
			Unsubscribe = unsubscribe,
			ExecutePhase = executePhase,
			GetTargetCount = getTargetCount ?? (() => 1)
		};
		storySequence.Add(step);
		return step;
	}

	protected void AddConditionStep(Action<Action> subscribe, Action<Action> unsubscribe, Action<bool> executePhase, Func<bool> checkCondition) =>
		storySequence.Add(new StoryStep {
			Subscribe = subscribe,
			Unsubscribe = unsubscribe,
			ExecutePhase = executePhase,
			CheckCondition = checkCondition
		});

	protected void AddInstantStep(Action<bool> executePhase) => 
		storySequence.Add(new StoryStep { ExecutePhase = executePhase });
	#endregion
	#region Life
	private void StartCurrentStepWait() {
		void OnEventTriggered() { stepEventCounter++; CheckCurrentStepCompletion(); }

		currentUnsubscribe?.Invoke();
		currentUnsubscribe = null;

		if (currentStoryStep >= storySequence.Count) 
			return;

		StoryStep step = storySequence[currentStoryStep];
		
		if (step.Subscribe != null) {
			currentUnsubscribe = () => step.Unsubscribe(OnEventTriggered);
			step.Subscribe(OnEventTriggered);
			CheckCurrentStepCompletion();
		} else {
			CompleteCurrentStep();
		}
	}

	private void CheckCurrentStepCompletion() {
		StoryStep step = storySequence[currentStoryStep];

		if (step.CheckCondition != null) {
			if (step.CheckCondition.Invoke()) CompleteCurrentStep();
		} else {
			int target = step.GetTargetCount?.Invoke() ?? 1;
			if (stepEventCounter >= target) CompleteCurrentStep();
		}
	}

	private void CompleteCurrentStep() {
		currentUnsubscribe?.Invoke();
		currentUnsubscribe = null;
		
		storySequence[currentStoryStep].ExecutePhase?.Invoke(false);
		
		currentStoryStep++;
		stepEventCounter = 0;
		
		StartCurrentStepWait();
	}
	#endregion
    public void AbortStory() {
        currentUnsubscribe?.Invoke();
        currentUnsubscribe = null;
        storyStarted = false;
		currentStoryStep = -1;
    }

	// public virtual void SaveData(ref SaveData data) {
	// 	data.StorySteps[StoryId] = (storyStarted ? currentStoryStep : -1, stepEventCounter);
	// 	SaveCustomState(ref data);
	// }
	// protected virtual void SaveCustomState(ref SaveData data) { }

	protected virtual void OnDestroy() => currentUnsubscribe?.Invoke();
}