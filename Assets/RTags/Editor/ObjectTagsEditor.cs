using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTags;
using RTags.Data;
using RTags.Utils;

namespace RTagsEditor
{
    [CustomEditor(typeof(ObjectTags))]
    public class ObjectTagsEditor : Editor
    {
        private Component[] components = new Component[0];
        private static bool componentFoldouts;
        private GUIStyle foldOutStyle;
        private static Dictionary<System.Type, bool> componentListFoldouts = new Dictionary<System.Type, bool>();
        private TagListAsset tagList;

        private void OnEnable() 
        {
            if(!target) { return; }
            tagList = AssetDatabase.LoadAssetAtPath<TagListAsset>(ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset");
            components = ((ObjectTags)target).GetComponents<Component>();
            try
            {
                foldOutStyle = EditorStyles.foldout;
            }
            catch
            {
                //This unity static class parameter has a null reference exception for some reason on recompile in engine
            }
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            bool targetEdited = false;

            if(CreateTagList(((ObjectTags)target), ((ObjectTags)target)._objectTags))
            {
                targetEdited = true;
            }

            foldOutStyle.fontStyle = FontStyle.Bold;
            componentFoldouts = EditorGUILayout.Foldout(componentFoldouts, new GUIContent("Component Tags"), true, foldOutStyle);

            if(componentFoldouts)
            {
                EditorGUI.indentLevel++;
                foreach(var component in components)
                {
                    if(component.GetType() == typeof(ObjectTags)) { continue; }
                    if(!((ObjectTags)target)._componentTags.ContainsTagsForComponent(component))
                    {
                        ((ObjectTags)target)._componentTags.Add(new ObjectTags.ComponentTags() {targetComponent = component, componentTags = new List<string>()});
                    }
                    if(CreateTagList(component, ((ObjectTags)target)._componentTags.GetTagListByComponent(component)))
                    {
                        targetEdited = true;
                    }
                }
            }

            if(targetEdited)
            {
                EditorUtility.SetDirty(target);
            }
        }


        public bool CreateTagList(Component component, List<string> tags)
        {
            bool targetEdited = false;
            EditorGUILayout.BeginHorizontal();
            System.Type cType = component.GetType();
            if(!componentListFoldouts.ContainsKey(cType)) { componentListFoldouts.Add(cType, false); }

            GUIContent label = component.GetType() == typeof(ObjectTags) ? new GUIContent("Object Tags") : new GUIContent(cType.ToString());

            componentListFoldouts[cType] = EditorGUILayout.Foldout(componentListFoldouts[cType], label, true, foldOutStyle);
            if(componentListFoldouts[cType])
            {
                if(GUILayout.Button("+", GUILayout.MaxHeight(19), GUILayout.MaxWidth(19)))
                {
                    tags.Add("");
                    targetEdited = true;
                }
                if(GUILayout.Button("-", GUILayout.MaxHeight(19), GUILayout.MaxWidth(19)) && tags.Count > 0)
                {
                    tags.RemoveAt(tags.Count - 1);
                    targetEdited = true;
                }
                GUILayout.Space(20);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(1);
                if(tags.Count == 0) { EditorGUILayout.LabelField("List Is Empty"); }
                List<string> options = tagList.GetTagNames();
                List<string> displayedOptions = tagList.GetTagNamesWithCacheStatus();
                displayedOptions.Insert(0, "None");
                for(int i = 0; i < tags.Count; i++)
                {
                    bool changed = false;
                    string curTag = tags[i];
                    int curSelection = options.IndexOf(curTag);
                    curSelection++;
                    int selected = EditorGUILayout.Popup(curSelection, displayedOptions.ToArray());
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
                        tags[i] = curTag;
                        targetEdited = true;
                    }
                }
            }
            else
            {
                EditorGUILayout.EndHorizontal();
            }

            return targetEdited;
        }
    }
}
