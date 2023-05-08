using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTags;
using RTags.Data;

namespace RTagsEditor
{
    [CustomPropertyDrawer(typeof(TagListAsset.TagInfo))]
    public class TagInfoEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            SerializedProperty tagName = property.FindPropertyRelative("tagName");
            SerializedProperty preCached = property.FindPropertyRelative("isPreCached");
            Rect left = new Rect(position);
            Rect mid = new Rect(position);
            Rect right = new Rect(position);
            if(position.width > 150)
            {
                mid.width = 70;
                right.width = 10;
                left.width = left.width - mid.width - right.width;
                mid.position += new Vector2(left.width, 0);
                right.position += new Vector2(left.width + mid.width, 0);

                EditorGUI.PropertyField(left, tagName, new GUIContent(""));
                EditorGUI.LabelField(mid, "Cached");
                EditorGUI.PropertyField(right, preCached, new GUIContent(""));
            }
            else
            {
                right.width = 10;
                left.width = left.width - right.width;
                right.position += new Vector2(left.width, 0);

                EditorGUI.PropertyField(left, tagName, new GUIContent(""));
                EditorGUI.PropertyField(right, preCached, new GUIContent(""));
            }
            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }
    }
}
