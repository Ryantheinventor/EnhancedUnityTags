using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTags
{
    public static class RTagsExtensions
    {
        public static bool IsTagged(this GameObject self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return false; }
            return ot.IsObjectTagged(tag);
        }

        public static bool IsTagged(this Component self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return false; }
            return ot.IsComponentTagged(tag, self);
        }

        public static void AddTag(this GameObject self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { ot = self.AddComponent<ObjectTags>(); }
            ot.AddTagToGO(tag);
        }

        public static void AddTag(this Component self, string tag)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { ot = self.gameObject.AddComponent<ObjectTags>(); }
            ot.AddTagToComponent(tag, self);
        }

        public static string[] GetAssignedTags(this GameObject self)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return new string[0]; }
            return ot.GetObjectTags();
        }

        public static string[] GetAssignedTags(this Component self)
        {
            ObjectTags ot = self.GetComponent<ObjectTags>();
            if(!ot) { return new string[0]; }
            return ot.GetComponentTags(self);
        }
    }
}
