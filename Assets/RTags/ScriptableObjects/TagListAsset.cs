using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTags.Data
{    
    //[CreateAssetMenu(menuName = "RTags/New Tag List", fileName = "Tag List")]
    public class TagListAsset : ScriptableObject
    {
        public List<TagInfo> tags = new List<TagInfo>();

        [System.Serializable]
        public struct TagInfo 
        {
            public string tagName;
            public bool isPreCached;
        }

        public List<string> GetTagNames() 
        {
            List<string> results = new List<string>();
            foreach(TagInfo t in tags)
            {
                results.Add(t.tagName);
            }
            return results;
        }

        public List<string> GetTagNamesWithCacheStatus() 
        {
            List<string> results = new List<string>();
            foreach(TagInfo t in tags)
            {
                if(t.tagName == "") { continue; }
                if(t.isPreCached)
                {
                    results.Add($"{t.tagName}(Cached)");
                }
                else
                {
                    results.Add($"{t.tagName}(Not Cached)");
                }
            }
            return results;
        }
    }
}