using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTags
{
    public static class RTagsExtensions
    {
        /// <summary>
        /// Is the GameObject tagged with the specified tag
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsTagged(this GameObject self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return false; }
            return ot.IsObjectTagged(tag);
        }

        /// <summary>
        /// Is the Component tagged with the specified tag
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsTagged(this Component self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return false; }
            return ot.IsComponentTagged(tag, self);
        }

        /// <summary>
        /// Adds the specified tag to the GameObject
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tag"></param>
        public static void AddTag(this GameObject self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { ot = self.AddComponent<ObjectTags>(); }
            ot.AddTagToGO(tag);
        }

        /// <summary>
        /// Adds the specified tag to the Component
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tag"></param>
        public static void AddTag(this Component self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { ot = self.gameObject.AddComponent<ObjectTags>(); }
            ot.AddTagToComponent(tag, self);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns>An array of all the tags currently assigned to the GameObject</returns>
        public static string[] GetAssignedTags(this GameObject self)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return new string[0]; }
            return ot.GetObjectTags();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns>An array of all the tags currently assigned to the Component</returns>
        public static string[] GetAssignedTags(this Component self)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return new string[0]; }
            return ot.GetComponentTags(self);
        }
    }
}
