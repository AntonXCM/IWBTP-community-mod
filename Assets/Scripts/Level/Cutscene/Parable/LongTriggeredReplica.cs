using System.Collections;
using UnityEngine;

public class LongTriggeredReplica : MonoBehaviour
{
	[SerializeField] private float delay = 5f;
    [SerializeField] private string key;
	private bool triggered;
	private Coroutine timer;
    private string trigger = "";
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (triggered) return;
        trigger = other.name;
		timer = StartCoroutine(WaitAndTrigger());
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (trigger == other.name && timer == null) return;
        print("Untriggering");
		StopCoroutine(timer);
		timer = null;
	}

	private IEnumerator WaitAndTrigger()
	{
        print("triggering");
		yield return new WaitForSecondsRealtime(delay);
        print("triggered");

		triggered = true;
		timer = null;
		OnTriggered();
	}

    private void OnTriggered() => FindObjectOfType<ParableNarrator>().Subtitles.text = Localization.GetText(key);
}