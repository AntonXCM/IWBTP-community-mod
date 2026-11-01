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
using System.Linq;
using UnityEngine;

// Линейные цепочки + ветвления.
//
// Сценарий по-прежнему плоский список шагов. Развилка — это шаг с несколькими «входами»:
// шаги каждой ветки лежат в списке подряд, а в конце каждой ветки стоит служебный «выход»,
// который перекидывает на общее продолжение:
//
//   0: Instant "Вступление"
//   1: Развилка ─┬─ вход A → 2
//                └─ вход B → 4
//   2: шаг ветки A
//   3: выход → 6
//   4: шаг ветки B
//   5: выход → 6
//   6: общее продолжение
//
// Поэтому позиция в истории — по-прежнему одно число (currentStoryStep), а для промотки при загрузке
// достаточно журнала выбранных веток (branchChoices): он говорит, куда сворачивать на каждой развилке.
public abstract class StorytellerBase : MonoBehaviour, ISavable {
	public void BeginStory() {
		storyStarted = true;
		currentStoryStep = 0;
		stepEventCounter = 0;
		branchChoices.Clear();
		branchCounters = null;
		StartCurrentStepWait();
	}

	public virtual void LoadData(SaveData data) {
		currentUnsubscribe?.Invoke();
		currentUnsubscribe = null;

		(currentStoryStep, stepEventCounter) = data.StorySteps.GetValueOrDefault(StoryId, (-1, 0));
		storyStarted = currentStoryStep != -1;

		branchChoices.Clear();
		branchCounters = null;
		if (data.StoryBranches != null && data.StoryBranches.TryGetValue(StoryId, out var branchState) && branchState != null) {
			if (branchState.Choices != null) branchChoices.AddRange(branchState.Choices);
			if (branchState.Counters != null && branchState.Counters.Length > 0)
				branchCounters = (int[])branchState.Counters.Clone();
		}

		storySequence.Clear();
		BuildStorySequence();

		if (storyStarted) currentStoryStep = FastForward(currentStoryStep);

		LoadCustomState(data);
		if (storyStarted) StartCurrentStepWait();
	}

	protected abstract void BuildStorySequence();
	protected virtual void LoadCustomState(SaveData data) { }

	protected abstract string StoryId { get; }
	public abstract int LoadPriority { get; }

	protected bool storyStarted = false;
	protected int currentStoryStep = -1;
	protected int stepEventCounter = 0;

	private readonly List<StoryStep> storySequence = new();
	private readonly List<int> branchChoices = new(); // выбранные ветки в порядке прохождения развилок
	private int[] branchCounters;                     // счётчики веток текущей развилки-гонки (иначе null)
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

		// --- ветвление ---
		// Развилка: подписывается на все свои триггеры и сообщает номер выигравшей ветки.
		// Возвращает (отписка, проверка «уже выполнено на момент входа в шаг»).
		public Func<Action<int>, int[], (Action disarm, Action checkNow)> Arm;
		public int[] BranchEntries;   // индекс первого шага каждой ветки
		public int JumpTo = -1;       // у служебных шагов «выход из ветки» / «конец истории»
		public bool IsBranching => Arm != null;
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

	// Завершает историю целиком (для веток-«концовок»): всё, что записано после, пропускается.
	protected void AddEndStep() =>
		storySequence.Add(new StoryStep { JumpTo = int.MaxValue });
	#endregion

	#region Branches

	// Одна ветка гонки: свой триггер + тело (шаги, которые выполняются, если ветка победила).
	protected class StoryBranch {
		public Action<Action> Subscribe;
		public Action<Action> Unsubscribe;
		public Func<int> GetTargetCount;
		public Func<bool> CheckCondition;
		public Action Build;
	}

	// Ветка срабатывает по событию (getTargetCount — сколько раз оно должно произойти, по умолчанию 1).
	protected static StoryBranch EventBranch(Action<Action> subscribe, Action<Action> unsubscribe, Action build, Func<int> getTargetCount = null) =>
		new StoryBranch {
			Subscribe = subscribe,
			Unsubscribe = unsubscribe,
			Build = build,
			GetTargetCount = getTargetCount ?? (() => 1)
		};

	// Ветка срабатывает, когда checkCondition() == true. Условие проверяется при каждом событии
	// и один раз сразу при входе в шаг (в том числе после загрузки), поэтому подходит для состояний.
	protected static StoryBranch ConditionBranch(Action<Action> subscribe, Action<Action> unsubscribe, Func<bool> checkCondition, Action build) =>
		new StoryBranch {
			Subscribe = subscribe,
			Unsubscribe = unsubscribe,
			CheckCondition = checkCondition,
			Build = build
		};

	// Гонка: побеждает ветка, чей триггер сработал первым. При одновременном выполнении —
	// та, что объявлена раньше. Остальные триггеры сразу отписываются.
	protected StoryStep AddRaceStep(params StoryBranch[] branches) {
		(Action, Action) Arm(Action<int> choose, int[] counters) {
			bool resolved = false;
			var handlers = new Action[branches.Length];

			void Check(int i) {
				if (resolved) return;
				StoryBranch b = branches[i];
				bool done = b.CheckCondition != null
					? b.CheckCondition()
					: counters[i] >= (b.GetTargetCount?.Invoke() ?? 1);
				if (!done) return;
				resolved = true;
				choose(i);
			}

			for (int i = 0; i < branches.Length; i++) {
				int index = i;
				handlers[index] = () => { counters[index]++; Check(index); };
				branches[index].Subscribe(handlers[index]);
			}

			return (
				() => { for (int i = 0; i < branches.Length; i++) branches[i].Unsubscribe(handlers[i]); },
				() => { for (int i = 0; i < branches.Length; i++) Check(i); });
		}

		return AddBranchStep(Arm, branches.Select(b => b.Build));
	}

	// Выбор по коду события: событие несёт значение (id варианта диалога и т.п.),
	// значение сопоставляется с веткой. Неизвестные коды игнорируются.
	protected StoryStep AddChoiceStep<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe,
		params (T key, Action build)[] cases) =>
		AddChoiceStep(subscribe, unsubscribe, null, cases);

	// То же, но неизвестные коды уводят в ветку otherwise.
	protected StoryStep AddChoiceStep<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe,
		Action otherwise, params (T key, Action build)[] cases) {

		(Action, Action) Arm(Action<int> choose, int[] _) {
			bool resolved = false;
			Action<T> handler = value => {
				if (resolved) return;
				int index = Array.FindIndex(cases, c => EqualityComparer<T>.Default.Equals(c.key, value));
				if (index < 0 && otherwise != null) index = cases.Length;
				if (index < 0) return;
				resolved = true;
				choose(index);
			};
			subscribe(handler);
			return (() => unsubscribe(handler), null);
		}

		var builders = cases.Select(c => c.build).ToList();
		if (otherwise != null) builders.Add(otherwise);
		return AddBranchStep(Arm, builders);
	}

	// Общая сборка развилки. Ветки строятся тем же AddStep/AddInstantStep/... — они просто
	// дописывают шаги в общий список, поэтому ветки можно вкладывать друг в друга.
	// Тела веток должны строиться детерминированно: индексы шагов попадают в сохранение.
	private StoryStep AddBranchStep(Func<Action<int>, int[], (Action disarm, Action checkNow)> arm, IEnumerable<Action> builders) {
		var step = new StoryStep { Arm = arm };
		storySequence.Add(step);

		var entries = new List<int>();
		var exits = new List<StoryStep>();
		foreach (Action build in builders) {
			entries.Add(storySequence.Count);
			build?.Invoke();

			var exit = new StoryStep();      // «выход из ветки» → общее продолжение
			storySequence.Add(exit);
			exits.Add(exit);
		}
		step.BranchEntries = entries.ToArray();

		int merge = storySequence.Count;
		foreach (StoryStep exit in exits) exit.JumpTo = merge;
		return step;
	}
	#endregion

	#region Life
	private void StartCurrentStepWait() {
		void OnEventTriggered() { stepEventCounter++; CheckCurrentStepCompletion(); }

		currentUnsubscribe?.Invoke();
		currentUnsubscribe = null;

		if (currentStoryStep >= storySequence.Count)
			return;

		StoryStep step = storySequence[currentStoryStep];

		if (step.IsBranching) {
			ArmBranchStep(step);
			return;
		}

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

		StoryStep step = storySequence[currentStoryStep];
		step.ExecutePhase?.Invoke(false);
		if (!storyStarted) return; // ExecutePhase мог вызвать AbortStory

		currentStoryStep = NextIndex(step, currentStoryStep);
		stepEventCounter = 0;

		StartCurrentStepWait();
	}

	private void ArmBranchStep(StoryStep step) {
		int count = step.BranchEntries.Length;
		if (branchCounters == null || branchCounters.Length != count)
			branchCounters = new int[count];

		var (disarm, checkNow) = step.Arm(branch => CompleteBranchStep(step, branch), branchCounters);
		currentUnsubscribe = disarm;
		checkNow?.Invoke();
	}

	private void CompleteBranchStep(StoryStep step, int branch) {
		currentUnsubscribe?.Invoke();
		currentUnsubscribe = null;

		step.ExecutePhase?.Invoke(false);
		if (!storyStarted) return;

		// Выбор и переход фиксируем вместе: сохранение не должно увидеть одно без другого.
		branchChoices.Add(branch);
		currentStoryStep = step.BranchEntries[branch];
		stepEventCounter = 0;
		branchCounters = null;

		StartCurrentStepWait();
	}

	private int NextIndex(StoryStep step, int index) =>
		Math.Min(step.JumpTo >= 0 ? step.JumpTo : index + 1, storySequence.Count);

	// Промотка: проходит путь от начала до сохранённой позиции, на каждой развилке сворачивая
	// по журналу выборов, и выполняет ExecutePhase(true) у пройденных шагов.
	// Возвращает позицию, где остановилась (для корректного сохранения она равна target).
	private int FastForward(int target) {
		int index = 0, cursor = 0;

		while (index < target && index < storySequence.Count) {
			StoryStep step = storySequence[index];
			step.ExecutePhase?.Invoke(true);

			if (step.IsBranching) {
				if (cursor >= branchChoices.Count || branchChoices[cursor] < 0 || branchChoices[cursor] >= step.BranchEntries.Length) {
					Debug.LogError($"[{StoryId}] Нет корректного сохранённого выбора для развилки на шаге {index}: сохранение не соответствует сценарию.");
					break;
				}
				index = step.BranchEntries[branchChoices[cursor++]];
			} else {
				index = NextIndex(step, index);
			}
		}

		if (cursor < branchChoices.Count)
			branchChoices.RemoveRange(cursor, branchChoices.Count - cursor);
		return index;
	}
	#endregion

	public void AbortStory() {
		currentUnsubscribe?.Invoke();
		currentUnsubscribe = null;
		storyStarted = false;
		currentStoryStep = -1;
		branchChoices.Clear();
		branchCounters = null;
	}

	public virtual void SaveData(ref SaveData data) {
		data.StorySteps[StoryId] = (storyStarted ? currentStoryStep : -1, stepEventCounter);

		data.StoryBranches ??= new();
		data.StoryBranches[StoryId] = new StoryBranchState {
			Choices = branchChoices.ToArray(),
			Counters = branchCounters?.ToArray() ?? Array.Empty<int>()
		};

		SaveCustomState(ref data);
	}

	protected virtual void SaveCustomState(ref SaveData data) { }

	protected virtual void OnDestroy() => currentUnsubscribe?.Invoke();
}
[Serializable]
public class StoryBranchState {
	public int[] Choices = new int[0];
	public int[] Counters = new int[0];
}