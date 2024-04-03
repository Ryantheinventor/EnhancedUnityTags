using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTags.Utils
{
    /// <summary>
    /// A set of extensions made to assist in interacting with lists of ComponentTags objects
    /// </summary>
    public static class ComponentTagListUtils
    {
        public static bool ContainsTagsForComponent(this List<ObjectTags.ComponentTags> sets, Component c)
        {
            foreach(ObjectTags.ComponentTags set in sets)
            {
                if(set.targetComponent == c)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<string> GetTagListByComponent(this List<ObjectTags.ComponentTags> sets, Component c)
        {
            foreach(ObjectTags.ComponentTags set in sets)
            {
                if(set.targetComponent == c)
                {
                    return set.componentTags;
                }
            }
            return null;
        }
    }
}
