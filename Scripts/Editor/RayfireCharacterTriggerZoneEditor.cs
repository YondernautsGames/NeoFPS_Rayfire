using UnityEngine;
using UnityEditor;

namespace NeoFPS.Rayfire
{
    [CustomEditor(typeof(RayfireCharacterTriggerZone), true)]
    public class RayfireCharacterTriggerZoneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            
            var activatorAction = serializedObject.FindProperty("m_ActivatorAction");
            var collisionAction = serializedObject.FindProperty("m_CollisionsAction");
            var wreckingAction = serializedObject.FindProperty("m_WreckingBallAction");

            EditorGUILayout.PropertyField(activatorAction);
            if (activatorAction.enumValueIndex == 0)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ActivatorThickness"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ActivatorDelay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DisableActivatorOnExit"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(collisionAction);
            if (collisionAction.enumValueIndex == 0)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FlipCollisionsOnExit"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(wreckingAction);
            if (wreckingAction.enumValueIndex == 0)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RelativeSpeedThreshold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WreckingBallForce"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WreckingBallRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WreckingBallOffset"));
                --EditorGUI.indentLevel;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}