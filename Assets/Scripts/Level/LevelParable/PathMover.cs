
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathMover : MonoBehaviour {
	public enum LoopMode {
		Once,
		LoopToStart,
		PingPong
	}

	[Serializable]
	public class PathPoint {
		public Vector3 position;
		[Min(0.01f)]
		[Tooltip("Время движения от этой точки до следующей.")]
		public float durationToNext = 1f;
	}

	[SerializeField] private List<PathPoint> points = new();
	[SerializeField] private LoopMode loopMode = LoopMode.Once;

	private int fromIndex;
	private int toIndex;
	private int direction = 1;
	private float elapsed;
	private float segmentDuration;
	private bool finished;

	private void Start() => Restart();

	private void Update() {
		if (finished || points.Count < 2) return;

		elapsed += Time.deltaTime;

		while (elapsed >= segmentDuration && !finished) {
			elapsed -= segmentDuration;
			transform.position = points[toIndex].position;
			AdvanceSegment();
		}

		if (finished) return;

		float t = Mathf.Clamp01(elapsed / segmentDuration);
		t = t * t * (3f - 2f * t);

		transform.position = Vector3.Lerp(
			points[fromIndex].position,
			points[toIndex].position,
			t
		);
	}

	public void Restart() {
		finished = true;
		elapsed = 0f;
		direction = 1;

		if (points.Count == 0) return;

		transform.position = points[0].position;

		if (points.Count < 2) return;

		fromIndex = 0;
		toIndex = 1;
		segmentDuration = GetDuration(0);
		finished = false;
	}

	private float GetDuration(int index) =>
		Mathf.Max(0.01f, points[index].durationToNext);

	private void AdvanceSegment() {
		int last = points.Count - 1;

		if (direction > 0) {
			if (toIndex < last) {
				fromIndex = toIndex;
				toIndex++;
				segmentDuration = GetDuration(fromIndex);
				return;
			}

			switch (loopMode) {
				case LoopMode.Once:
					finished = true;
					break;

				case LoopMode.LoopToStart:
					fromIndex = last;
					toIndex = 0;
					segmentDuration = GetDuration(last);
					break;

				case LoopMode.PingPong:
					direction = -1;
					fromIndex = last;
					toIndex = last - 1;
					segmentDuration = GetDuration(toIndex);
					break;
			}
		}
		else {
			if (toIndex > 0) {
				fromIndex = toIndex;
				toIndex--;
				segmentDuration = GetDuration(toIndex);
			}
			else {
				direction = 1;
				fromIndex = 0;
				toIndex = 1;
				segmentDuration = GetDuration(0);
			}
		}
	}

	private void OnValidate() {
		if (points == null) points = new List<PathPoint>();

		foreach (var point in points) {
			if (point == null) continue;
			point.durationToNext = Mathf.Max(0.01f, point.durationToNext);
		}
	}
}
