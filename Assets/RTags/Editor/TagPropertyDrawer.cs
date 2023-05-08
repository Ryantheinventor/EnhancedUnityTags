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
            if(!tagList) 
            {
                if(GUI.Button(position, "Create Tag List"))
                {
                    TagListEditor.CreateNewTagList();
                }
            }
            else
            {
                List<string> options = tagList.GetTagNames();
                List<string> displayedOptions = tagList.GetTagNamesWithCacheStatus();
                displayedOptions.Insert(0,"None");
                if(options.Count == 0)
                {
                    if(GUI.Button(position, "Add tag in tag list."))
                    {
                        Selection.activeObject = tagList;
                    }
                }
                else
                {
                    bool changed = false;
                    string curTag = property.FindPropertyRelative("tagName").stringValue;
                    int curSelection = options.IndexOf(curTag);
                    curSelection++;
                    int selected = EditorGUI.Popup(position, curSelection, displayedOptions.ToArray());
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
