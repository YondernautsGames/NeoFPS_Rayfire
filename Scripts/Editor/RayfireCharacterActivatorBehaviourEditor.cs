using UnityEditor;
using NeoFPS.Rayfire;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPSEditor.Rayfire
{
    [MotionGraphBehaviourEditor(typeof(RayfireCharacterActivatorBehaviour))]
    public class RayfireCharacterActivatorBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            var onEnter = serializedObject.FindProperty("m_OnEnter");
            var onExit = serializedObject.FindProperty("m_OnExit");
            EditorGUILayout.PropertyField(onEnter);
            EditorGUILayout.PropertyField(onExit);

            if (onEnter.enumValueIndex == 0 || onExit.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Thickness"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Delay"));
            }
        }
    }
}
