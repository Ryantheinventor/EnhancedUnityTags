using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTags.Data;
using RTags;

namespace RTagsEditor
{
    public class TagListWindow : EditorWindow
    {
        private TagListAsset tagList;
        private Editor tagListEditor;

        public static void InitWindow(TagListAsset tagList)
        {
            TagListWindow window = (TagListWindow)EditorWindow.GetWindow(typeof (TagListWindow));
            window.tagList = tagList;
            window.titleContent = new GUIContent("Tag List");
            window.SetInspector();
        }

        public void OnGUI() 
        {
            if(tagList)
            {
                tagListEditor.OnInspectorGUI(); 
                if(EditorUtility.IsDirty(tagList))
                {
                    titleContent = new GUIContent("Tag List*");
                }
                else
                {
                    titleContent = new GUIContent("Tag List");
                }
            }
            else
            {
                if(TagListEditor.OpenTagListCheck())
                {
                    TagListEditor.OpenTagList();
                }
                else
                {
                    GUILayout.Label("Missing Tag List");
                    if(TagListEditor.CreateNewTagListCheck())
                    {
                        if (GUILayout.Button("Create new tag list")) TagListEditor.CreateNewTagList();
                    }
                }
            }
        }

        private void SetInspector()
        {
            if (tagListEditor) { DestroyImmediate(tagListEditor); }
            if (tagList) { tagListEditor = Editor.CreateEditor(tagList); }
        }

        public void OnDisable() 
        {
            if (tagListEditor) { DestroyImmediate(tagListEditor); }
        }

    }
}
