using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Project.Scripts.Fractures
{
    [Flags]
    public enum Anchor
    {
        None = 0,
        Left = 1,
        Right = 2,
        Bottom = 4,
        Top = 8,
        Front = 16,
        Back = 32
    }
        
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Anchor))]
    public class AnchorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            property.intValue = (int)(Anchor)EditorGUI.EnumFlagsField(position, label, (Anchor)property.intValue);
            EditorGUI.EndProperty();
        }
    }
#endif
}