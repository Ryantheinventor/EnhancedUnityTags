using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTags;
using RTags.Data;

namespace RTagsEditor
{
    public class TagListEditor : MonoBehaviour
    {
        
        [MenuItem("Assets/Create/RTags/Tag List")]
        public static void CreateNewTagList()
        {
            ConfirmAssetPath(ObjectTags.tagListPath);
            string combinedPath = ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset";
            TagListAsset tagListAsset = AssetDatabase.LoadAssetAtPath<TagListAsset>(combinedPath);
            if(!tagListAsset)
            {
                tagListAsset = ScriptableObject.CreateInstance<TagListAsset>();
                AssetDatabase.CreateAsset(tagListAsset, combinedPath);
            }
            Selection.activeObject = tagListAsset;
        }

        [MenuItem("Assets/Create/RTags/Tag List", true)]
        public static bool CreateNewTagListCheck()
        {
            if (!AssetDatabase.IsValidFolder(ObjectTags.tagListPath)) { return true; }
            string combinedPath = ObjectTags.tagListPath + "/" + ObjectTags.tagListName + ".asset";
            if (!AssetDatabase.LoadAssetAtPath<TagListAsset>(combinedPath)) { return true; }
            return false;
        }

        //Checks if a path exists if it doesn't it creates it
        private static void ConfirmAssetPath(string path)
        {
            string[] folders = path.Split("/");
            string currentPath = "Assets";
            foreach(string folder in folders)
            {
                if(folder == "Assets") {continue;}
                if(!AssetDatabase.IsValidFolder(currentPath + "/" + folder))
                {
                    AssetDatabase.CreateFolder(currentPath, folder);
                }
                currentPath += "/" + folder;
            }
        }
    }
}
