using _Project.Scripts.UI.Rendering;
using UnityEditor;
using UnityEditor.UI;

namespace _Project.Scripts.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(NonDrawingGraphic), false)]
    public class NonDrawingGraphicEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}