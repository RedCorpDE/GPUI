#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI.Editor
{
    [CustomEditor(typeof(UiCombiLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class UiCombiLayoutGroupEditor : UnityEditor.Editor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_ChildAlignment;
        SerializedProperty m_Spacing;
        SerializedProperty m_ChildControlWidth;
        SerializedProperty m_ChildControlHeight;

        SerializedProperty m_Orientation;
        SerializedProperty m_Wrap;
        SerializedProperty m_LineSpacing;

        void OnEnable()
        {
            m_Padding           = serializedObject.FindProperty("m_Padding");
            m_ChildAlignment    = serializedObject.FindProperty("m_ChildAlignment");
            m_Spacing           = serializedObject.FindProperty("m_Spacing");
            m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            m_ChildControlHeight= serializedObject.FindProperty("m_ChildControlHeight");

            m_Orientation       = serializedObject.FindProperty("m_Orientation");
            m_Wrap              = serializedObject.FindProperty("m_Wrap");
            m_LineSpacing       = serializedObject.FindProperty("m_LineSpacing");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUILayout.PropertyField(m_ChildAlignment, true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Orientation, new GUIContent("Orientation"));
            EditorGUILayout.PropertyField(m_Wrap, new GUIContent("Wrap"));
            EditorGUILayout.PropertyField(m_Spacing, new GUIContent("Spacing (Main Axis)"));
            EditorGUILayout.PropertyField(m_LineSpacing, new GUIContent("Line Spacing (Cross Axis)"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Child Control", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ChildControlWidth, new GUIContent("Control Child Width"));
            EditorGUILayout.PropertyField(m_ChildControlHeight, new GUIContent("Control Child Height"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
