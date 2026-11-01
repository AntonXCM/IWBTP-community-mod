
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathMover))]
public class PathMoverEditor : Editor {
	public override void OnInspectorGUI() {
		serializedObject.Update();
		DrawPropertiesExcluding(serializedObject, "m_Script");
		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.HelpBox(
			"durationToNext — время перехода от текущей точки к следующей.\n" +
			"LoopToStart использует durationToNext последней точки для возврата.\n" +
			"PingPong использует длительность исходного сегмента и при движении назад.",
			MessageType.Info
		);
	}

	private void OnSceneGUI() {
		serializedObject.Update();

		var points = serializedObject.FindProperty("points");
		var loop = serializedObject.FindProperty("loopMode");
		int count = points.arraySize;

		if (count == 0) {
			serializedObject.ApplyModifiedProperties();
			return;
		}

		var positions = new Vector3[count];
		var durations = new float[count];

		// Редактирование точек
		for (int i = 0; i < count; i++) {
			var point = points.GetArrayElementAtIndex(i);
			var position = point.FindPropertyRelative("position");
			var duration = point.FindPropertyRelative("durationToNext");

			EditorGUI.BeginChangeCheck();
			Vector3 newPosition = Handles.PositionHandle(
				position.vector3Value,
				Quaternion.identity
			);

			if (EditorGUI.EndChangeCheck())
				position.vector3Value = newPosition;

			positions[i] = position.vector3Value;
			durations[i] = duration.floatValue;
		}

		// Точки и время прибытия
		float arrivalTime = 0f;

		for (int i = 0; i < count; i++) {
			float size = HandleUtility.GetHandleSize(positions[i]);

			Handles.SphereHandleCap(
				0,
				positions[i],
				Quaternion.identity,
				size * 0.12f,
				EventType.Repaint
			);

			Handles.Label(
				positions[i] + Vector3.up * size * 0.2f,
				$"P{i} | {arrivalTime:0.##}s"
			);

			if (i < count - 1)
				arrivalTime += durations[i];
		}

		// Сегменты и длительность переходов
		for (int i = 0; i < count - 1; i++)
			DrawSegment(positions[i], positions[i + 1], durations[i]);

		// Замыкающий сегмент
		var mode = (PathMover.LoopMode)loop.enumValueIndex;

		if (mode == PathMover.LoopMode.LoopToStart && count > 1)
			DrawSegment(positions[count - 1], positions[0], durations[count - 1]);

		serializedObject.ApplyModifiedProperties();
	}

	private static void DrawSegment(Vector3 from, Vector3 to, float duration) {
		Handles.DrawAAPolyLine(2f, from, to);

		Vector3 middle = (from + to) * 0.5f;
		float size = HandleUtility.GetHandleSize(middle);

		Handles.Label(
			middle + Vector3.up * size * 0.15f,
			$"{duration:0.##}s"
		);
	}
}
