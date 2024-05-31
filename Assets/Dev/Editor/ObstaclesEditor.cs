using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Obstacles)), CanEditMultipleObjects]
public class ObstaclesEditor : Editor
{
	public SerializedProperty
		state,
		AmountSlowness,
		AmountDuration,
		AmountDistance;

	void OnEnable()
	{
		// Setup the SerializedProperties
		state = serializedObject.FindProperty("obstacleType");
		AmountSlowness = serializedObject.FindProperty("slowness");
		AmountDuration = serializedObject.FindProperty("duration");
		AmountDistance = serializedObject.FindProperty("distance");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(state);

		Obstacles.WhichObstacle st = (Obstacles.WhichObstacle)state.enumValueIndex;

		switch (st)
		{
			case Obstacles.WhichObstacle.Snowpile:
				EditorGUILayout.PropertyField(AmountSlowness, new GUIContent("slowness"));
				EditorGUILayout.PropertyField(AmountDuration, new GUIContent("duration"));

				break;
			case Obstacles.WhichObstacle.Spike:
				EditorGUILayout.PropertyField(AmountSlowness, new GUIContent("slowness"));
				EditorGUILayout.PropertyField(AmountDuration, new GUIContent("duration"));
				EditorGUILayout.PropertyField(AmountDistance, new GUIContent("distance"));

				break;
			case Obstacles.WhichObstacle.Air:
				EditorGUILayout.PropertyField(AmountDistance, new GUIContent("distance"));
				break;
		}


		serializedObject.ApplyModifiedProperties();
	}
}