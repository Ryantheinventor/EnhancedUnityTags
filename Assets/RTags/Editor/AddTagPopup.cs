using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTags;
using RTags.Data;

namespace RTagsEditor
{
    public class AddTagPopup : PopupWindowContent
    {
        private static Vector2 popupSize = new Vector2(400, 85);
        public static bool Open { get; private set; }
        public static List<GameObject> targetGameObjects = new List<GameObject>();
        public static List<Component> targetComponents = new List<Component>();

        private TagListAsset tagList;

        private string curTag = "";

        public override void OnGUI(Rect rect)
        {
            editorWindow.minSize = popupSize;
            editorWindow.maxSize = popupSize;
            editorWindow.position = new Rect(EditorGUIUtility.GetMainWindowPosition().width / 2 - popupSize.x / 2, 
                                            EditorGUIUtility.GetMainWindowPosition().height / 2 - popupSize.y / 2, 
                                            popupSize.x, 
                                            popupSize.y);

            GUILayout.Label($"Adding Tag to {targetGameObjects.Count} GameObjects and {targetComponents.Count} Components");
            if(!tagList)
            {
                GUILayout.Label($"No tag list found.");
                if (GUILayout.Button("Create a tag list.")) 
                {
                    TagListEditor.CreateNewTagList(); 
                    tagList = AssetDatabase.LoadAssetAtPath<TagListAsset>(ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset");
                    editorWindow.Close();
                }
                GUILayout.Space(20);
                if (GUILayout.Button("Cancel")) editorWindow.Close();
                return;
            }
            else if(tagList.tags.Count == 0)
            {
                GUILayout.Label($"No tags found in list.");
                if (GUILayout.Button("Create a tag.")) 
                {
                    TagListEditor.OpenTagList();
                    tagList = AssetDatabase.LoadAssetAtPath<TagListAsset>(ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset");
                    editorWindow.Close();
                }
                GUILayout.Space(20);
                if (GUILayout.Button("Cancel")) editorWindow.Close();
            }

            List<string> options = tagList.GetTagNames();
            List<string> displayedOptions = tagList.GetTagNamesWithCacheStatus();
            displayedOptions.Insert(0, "None");
            int curSelection = options.IndexOf(curTag) + 1;
            int selected = EditorGUILayout.Popup(curSelection, displayedOptions.ToArray());
            if(selected != curSelection)
            {
                if(selected == 0) 
                {
                    curTag = ""; 
                }
                else
                {
                    selected--;
                    curTag = options[selected];
                }
            }
            if (curTag != "")
            {
                if (GUILayout.Button("Add"))
                {
                    foreach(GameObject go in targetGameObjects)
                    {
                        go.AddTag(curTag);
                        EditorUtility.SetDirty(go);
                    }
                    foreach(Component c in targetComponents)
                    {
                        c.AddTag(curTag);
                        EditorUtility.SetDirty(c);
                    }
                    editorWindow.Close();
                }
            }
            else
            {
                GUILayout.Space(20);
            }
            if (GUILayout.Button("Cancel")) editorWindow.Close();
            
        }

        public override void OnOpen()
        {

            tagList = AssetDatabase.LoadAssetAtPath<TagListAsset>(ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset");

            Open = true;
            Debug.Log("Open");
            foreach(GameObject go in targetGameObjects)
            {
                Debug.Log(go.name);
            }
            foreach(Component c in targetComponents)
            {
                Debug.Log(c.gameObject.name + "/" + c.GetType());
            }
        }

        public override void OnClose()
        {
            Open = false;
            Debug.Log("Close");
            targetGameObjects.Clear();
            targetComponents.Clear();;
        }
    }
}
