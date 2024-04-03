using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTags.Data
{    
    /// <summary>
    /// This asset stores the preconfigured tag list
    /// </summary>
    public class TagListAsset : ScriptableObject
    {
        public List<TagInfo> tags = new List<TagInfo>();

        [System.Serializable]
        public struct TagInfo 
        {
            public string tagName;
            public bool isPreCached;
        }

        /// <summary>
        /// all the tag names
        /// </summary>
        /// <returns></returns>
        public List<string> GetTagNames() 
        {
            List<string> results = new List<string>();
            foreach(TagInfo t in tags)
            {
                results.Add(t.tagName);
            }
            return results;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns>all the tag names with an added part to display it's cached status</returns>
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