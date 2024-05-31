using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCollidables)), CanEditMultipleObjects]
public class PlayerCollidablesEditor : Editor
{
	public SerializedProperty
		state,
		checkpointNumber;

	void OnEnable()
	{
		// Setup the SerializedProperties
		state = serializedObject.FindProperty("whichColType");
		checkpointNumber = serializedObject.FindProperty("checkpointNumber");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(state);

		PlayerCollidables.WhichColliderType st = (PlayerCollidables.WhichColliderType)state.enumValueIndex;

		switch (st)
		{
			case PlayerCollidables.WhichColliderType.Checkpoint:
				EditorGUILayout.PropertyField(checkpointNumber, new GUIContent("checkpointNumber"));
				break;
		}


		serializedObject.ApplyModifiedProperties();
	}
}