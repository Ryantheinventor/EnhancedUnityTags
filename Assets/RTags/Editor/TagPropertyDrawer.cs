using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTags;
using RTags.Data;

namespace RTagsEditor
{
    [CustomPropertyDrawer(typeof(Tag))]
    public class TagPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);
            TagListAsset tagList = AssetDatabase.LoadAssetAtPath<TagListAsset>(ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset");

            Rect labelRect = new Rect(position);
            Rect valueRect = new Rect(position);
            labelRect.width = EditorGUIUtility.labelWidth;
            valueRect.width = position.width - labelRect.width;
            valueRect.position += new Vector2(labelRect.width, 0);

            GUI.Label(labelRect, property.displayName);

            if(!tagList) 
            {
                if(GUI.Button(valueRect, "Create Tag List"))
                {
                    TagListEditor.CreateNewTagList();
                }
            }
            else
            {
                List<string> options = tagList.GetTagNames();
                List<string> displayedOptions = tagList.GetTagNamesWithCacheStatus();
                string curTag = property.FindPropertyRelative("tagName").stringValue;
                displayedOptions.Insert(0,"None");
                if(!options.Contains(curTag) && curTag != "") 
                {
                    options.Add(curTag);
                    displayedOptions.Add($"{curTag}(Not Cached)(Script Defined)");
                }
                if(options.Count == 0)
                {
                    if(GUI.Button(valueRect, "Add tag in tag list."))
                    {
                        Selection.activeObject = tagList;
                    }
                }
                else
                {
                    bool changed = false;
                    int curSelection = options.IndexOf(curTag);
                    curSelection++;
                    int selected = EditorGUI.Popup(valueRect, curSelection, displayedOptions.ToArray());
                    if(selected != curSelection)
                    {
                        if(selected == 0) 
                        {
                            curTag = ""; 
                            changed = true;
                        }
                        else
                        {
                            selected--;
                            curTag = options[selected];
                            changed = true;
                        }
                    }
                    if(changed) 
                    {
                        property.FindPropertyRelative("tagName").stringValue = curTag;
                    }
                }

            }
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
