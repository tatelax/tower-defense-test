using UnityEditor;
using UnityEngine;
using UI;

[CustomEditor(typeof(CharacterButton))]
public class CharacterButtonEditor : Editor
{
  public override void OnInspectorGUI()
  {
    // Exclude "unitData" from default drawing so we can draw it manually
    DrawPropertiesExcluding(serializedObject, "unitData");

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Character Button Settings", EditorStyles.boldLabel);

    SerializedProperty unitDataProp = serializedObject.FindProperty("unitData");
    EditorGUILayout.PropertyField(unitDataProp, new GUIContent("Unit Data"));

    serializedObject.ApplyModifiedProperties();
  }
}