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


            if(!tagList)
            {
                if(GUILayout.Button("No Tag List, Create New Tag List"))
                {
                    TagListEditor.CreateNewTagList();
                    tagList = AssetDatabase.LoadAssetAtPath<TagListAsset>(ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset");
                }
                else
                {
                    return;
                }
            }
            else
            {
                if(GUILayout.Button("Open Tag List"))
                {
                    TagListEditor.OpenTagList();
                }
            }


            List<ObjectTags.ComponentTags> newSets = new List<ObjectTags.ComponentTags>();
            for (int i = ((ObjectTags)target)._componentTags.Count - 1; i >= 0 ; i--)
            {
                var ct = ((ObjectTags)target)._componentTags[i];
                if(!ct.targetComponent) 
                { 
                    ((ObjectTags)target)._componentTags.RemoveAt(i);
                    targetEdited = true;
                    continue;
                }
                else if(ct.targetComponent.gameObject != ((ObjectTags)target).gameObject)
                {
                    bool foundMatch = false;
                    foreach(Component c in components)
                    {
                        if(c.GetType() == ct.targetComponent.GetType())
                        {
                            var newCT = new ObjectTags.ComponentTags();
                            newCT.targetComponent = c;
                            newCT.componentTags = new List<string>(ct.componentTags);
                            newSets.Add(newCT);
                            foundMatch = true;
                            targetEdited = true;
                        }
                    }
                    if(!foundMatch)
                    {
                        ((ObjectTags)target)._componentTags.RemoveAt(i);
                        targetEdited = true;
                    }
                }
            }
            foreach(var newSet in newSets)
            {
                bool foundMatch = false;
                foreach(var oldSet in ((ObjectTags)target)._componentTags)
                {
                    if(oldSet.targetComponent == newSet.targetComponent)
                    {
                        foreach(string tag in newSet.componentTags)//we gettin deep
                        {
                            if(!oldSet.componentTags.Contains(tag))
                            {
                                oldSet.componentTags.Add(tag);
                            }
                        }
                        foundMatch = true;
                    }
                }
                if(!foundMatch)
                {
                    ((ObjectTags)target)._componentTags.Add(newSet);
                }
            }

            if(CreateTagList(((ObjectTags)target), ((ObjectTags)target)._objectTags))
            {
                targetEdited = true;
            }

            foldOutStyle.fontStyle = FontStyle.Bold;
            int tagCount = 0;
            foreach (ObjectTags.ComponentTags ct in ((ObjectTags)target)._componentTags) 
            {
                tagCount += ct.componentTags.Count;
            }
            componentFoldouts = EditorGUILayout.Foldout(componentFoldouts, new GUIContent($"Component Tags ({tagCount})"), true, foldOutStyle);

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

            GUIContent label = component.GetType() == typeof(ObjectTags) ? new GUIContent("Object Tags" + $" ({tags.Count})") : new GUIContent(cType.ToString() + $" ({tags.Count})");

            componentListFoldouts[cType] = EditorGUILayout.Foldout(componentListFoldouts[cType], label, true, foldOutStyle);
            if(componentListFoldouts[cType])
            {
                if(GUILayout.Button("+", GUILayout.MaxHeight(19), GUILayout.MaxWidth(19)))
                {
                    tags.Add("");
                    targetEdited = true;
                }
                //if(GUILayout.Button("-", GUILayout.MaxHeight(19), GUILayout.MaxWidth(19)) && tags.Count > 0)
                //{
                //    tags.RemoveAt(tags.Count - 1);
                //    targetEdited = true;
                //}
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
                    Rect popupRect = EditorGUILayout.GetControlRect();
                    Rect removeButtonRect = popupRect;


                    
                    popupRect.width = popupRect.width - 42;
                    int selected = EditorGUI.Popup(popupRect, curSelection, displayedOptions.ToArray());

                    removeButtonRect.x = popupRect.x + popupRect.width + 2;
                    removeButtonRect.width = 20;
                    if(GUI.Button(removeButtonRect, "-")) 
                    {
                        tags.RemoveAt(i);
                        return targetEdited;
                    }
                    
                    //int selected = EditorGUILayout.Popup(curSelection, displayedOptions.ToArray());
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

        [MenuItem("CONTEXT/Component/Add Tag")]
        public static void AddTagToComponent(MenuCommand command)
        {
            // Debug.Log($"Add tag to comp:{((Component)command.context).gameObject.name}/{command.context.GetType()}");
            AddTagPopup.targetComponents.Add((Component)command.context);
            if(!AddTagPopup.Open)
            {
                PopupWindow.Show(GetPopupRect(), new AddTagPopup());
            }
        }

        [MenuItem("CONTEXT/Component/Add Tag", true)]
        public static bool AddTagToComponentCheck(MenuCommand command)
        {
            return command.context.GetType() != typeof(ObjectTags);
        }

        [MenuItem("GameObject/Add Tag")]
        public static void AddTagToGameObject(MenuCommand command)
        {
            if(Selection.objects.Length > 0 && command.context != null)
            {
                if(command.context == Selection.objects[0])//this method *sometimes*(thanks Unity) gets called on each selected object, we need to only run it once
                {
                    foreach(Object o in Selection.objects)
                    {
                        AddTagPopup.targetGameObjects.Add((GameObject)o);
                    }
                    PopupWindow.Show(GetPopupRect(), new AddTagPopup());
                }
            }
            else if(command.context == null)
            {
                foreach(Object o in Selection.objects)
                    {
                        AddTagPopup.targetGameObjects.Add((GameObject)o);
                    }
                    PopupWindow.Show(GetPopupRect(), new AddTagPopup());
            }
        }

        [MenuItem("GameObject/Add Tag", true)]
        public static bool AddTagToGameObjectCheck(MenuCommand command)
        {
            Object[] selectedObjects = Selection.objects;
            if(selectedObjects.Length == 0) { return false; }
            foreach(Object o in selectedObjects)
            {
                if(o.GetType() != typeof(GameObject))
                {
                    return false;
                }
            }
            return true;
        }

        private static Rect GetPopupRect()
        {
            return new Rect(1, 1, 1, 1);
        }
    }
}
