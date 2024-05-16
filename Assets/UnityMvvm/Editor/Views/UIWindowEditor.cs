using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fusion.Mvvm
{
    [CustomEditor(typeof(Window), true)]

    public class UIWindowEditor : UnityEditor.Editor
    {
        [SerializeField]
        private bool foldout = true;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            var windowTypeProperty = serializedObject.FindProperty("windowType");

            WindowType windowType = (WindowType)windowTypeProperty.enumValueIndex;
            foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Window Settings", ""));

            List<GUIContent> windowSettings = new List<GUIContent>()
            {
                new GUIContent("windowType","Type of Window"),
                new GUIContent("windowPriority","When pop-up windows are queued to open, windows with higher priority will be opened first."),
                new GUIContent("stateBroadcast","If true, the window state broadcasting feature is enabled.")
            };

            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                {
                    if ("m_Script" == property.propertyPath)
                        continue;

                    GUIContent propertyContent = windowSettings.Find(c => c.text.Equals(property.propertyPath));
                    if (propertyContent != null)
                    {
                        if (foldout)
                        {
                            if ("windowPriority" == property.propertyPath && windowType != WindowType.QUEUED_POPUP)
                                continue;

                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(property, propertyContent);
                            EditorGUI.indentLevel--;
                        }
                        continue;
                    }

                    EditorGUILayout.PropertyField(property, true);
                }
                expanded = false;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}