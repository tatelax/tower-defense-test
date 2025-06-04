#if UNITY_EDITOR
using UI;
using UnityEditor;

namespace Editor
{
  [CustomEditor(typeof(CharacterButton))]
  public class CharacterButtonEditor : UnityEditor.UI.ButtonEditor
  {
    private SerializedProperty _characterProp;

    protected override void OnEnable()
    {
      base.OnEnable();
      _characterProp = serializedObject.FindProperty("_character");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      EditorGUILayout.PropertyField(_characterProp);
      serializedObject.ApplyModifiedProperties();
      EditorGUILayout.Space();
      
      base.OnInspectorGUI();
    }
  }
}
#endif