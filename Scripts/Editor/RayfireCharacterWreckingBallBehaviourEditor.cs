using UnityEditor;
using NeoFPS.Rayfire;
using NeoFPSEditor.CharacterMotion;

namespace NeoFPSEditor.Rayfire
{
    [MotionGraphBehaviourEditor(typeof(RayfireCharacterWreckingBallBehaviour))]
    public class RayfireCharacterWreckingBallBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            var onEnter = serializedObject.FindProperty("m_OnEnter");
            var onExit = serializedObject.FindProperty("m_OnExit");
            EditorGUILayout.PropertyField(onEnter);
            EditorGUILayout.PropertyField(onExit);

            if (onEnter.enumValueIndex == 0 || onExit.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RelativeSpeedThreshold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxForce"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Radius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Offset"));
            }
        }
    }
}
