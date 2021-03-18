using UnityEditor;
using NeoFPS.Rayfire;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPSEditor.Rayfire
{
    [MotionGraphBehaviourEditor(typeof(RayfireCharacterCollisionBehaviour))]
    public class RayfireCharacterCollisionBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExit"));
        }
    }
}
