using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTags.Data;
using RTags.Utils;

namespace RTags
{

    public class ObjectTags : MonoBehaviour
    {
        public static readonly string tagListPath = "Assets/RTags/Resources/TagList";
        public static readonly string tagListName = "TagList";
        private static Dictionary<string, List<ObjectTags>> cachedTags = new Dictionary<string, List<ObjectTags>>();
        private static List<string> needsCacheTags = new List<string>();//tracks tags that have had their cache mode changed and need to be cached on their next call
        private static List<TagListAsset.TagInfo> _trackedTags;
        private static List<TagListAsset.TagInfo> TrackedTags
        {
            get
            {
                ConfirmTagListLoaded();
                return _trackedTags;
            }
        }

        //The only reason these are public is so that the custom inspector can see them
        public List<string> _objectTags = new List<string>();
        public List<ComponentTags> _componentTags = new List<ComponentTags>();

        [System.Serializable]
        public struct ComponentTags
        {
            public Component targetComponent;
            public List<string> componentTags;
        }

        #region Instance Methods

        void Awake() 
        {
            foreach(string tag in _objectTags)
            {
                if(tag != "" && IsTagCached(tag))
                {
                    if(cachedTags.ContainsKey(tag) && !cachedTags[tag].Contains(this))
                    {
                        cachedTags[tag].Add(this);
                    }
                    else
                    {
                        cachedTags.Add(tag, new List<ObjectTags>() { this });
                    }
                }
            }
            foreach(ComponentTags tagSet in _componentTags)
            {
                foreach(string tag in tagSet.componentTags)
                {
                    if (tag != "" && IsTagCached(tag))
                    {    
                        if(cachedTags.ContainsKey(tag) && !cachedTags[tag].Contains(this))
                        {
                            cachedTags[tag].Add(this);
                        }
                        else
                        {
                            cachedTags.Add(tag, new List<ObjectTags>() { this });
                        }
                    }
                }
            }
        }

        void OnDestroy() 
        {
            foreach(string tag in _objectTags)
            {
                if(tag != "" && IsTagCached(tag))
                {
                    if(cachedTags.ContainsKey(tag) && cachedTags[tag].Contains(this))
                    {
                        cachedTags[tag].Remove(this);
                    }
                }
            }
            foreach(ComponentTags tagSet in _componentTags)
            {
                foreach(string tag in tagSet.componentTags)
                {
                    if (tag != "" && IsTagCached(tag))
                    {    
                        if(cachedTags.ContainsKey(tag) && cachedTags[tag].Contains(this))
                        {
                            cachedTags[tag].Remove(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the specified object is tagged
        /// </summary>
        /// <returns>True if the tag is present on the object</returns>
        public bool IsObjectTagged(string tag)
        {
            return _objectTags.Contains(tag);
        }

        /// <summary>
        /// Checks if the specified component is tagged
        /// </summary>
        /// <returns>True if the tag is present on the cosmetic</returns>
        public bool IsComponentTagged(string tag, Component component)
        {
            if(_componentTags.ContainsTagsForComponent(component))
            {
                return _componentTags.GetTagListByComponent(component).Contains(tag);
            }
            return false;
        }

        /// <summary>
        /// Adds the specified tag to the attached GameObject
        /// </summary>
        public void AddTagToGO(string tag) 
        {
            if (tag == "") { return; }
            if (_objectTags.Contains(tag)) { return; }
            _objectTags.Add(tag);
            if(!IsTagCached(tag)){ return; }
            if (cachedTags.ContainsKey(tag) && !cachedTags[tag].Contains(this))
            {
                cachedTags[tag].Add(this);
            }
            else 
            {
                cachedTags.Add(tag, new List<ObjectTags> { this });
            }
        }

        /// <summary>
        /// Adds the specified tag to the specified component
        /// </summary>
        public void AddTagToComponent(string tag, Component c) 
        {
            if (tag == "") { return; }
            if (!_componentTags.ContainsTagsForComponent(c)) 
            {
                if (c.gameObject == gameObject) { _componentTags.Add(new ComponentTags() { targetComponent = c, componentTags = new List<string>()}); }
                else { return; }
            }
            if (_componentTags.GetTagListByComponent(c).Contains(tag)) { return; }
            _componentTags.GetTagListByComponent(c).Add(tag);
            if (cachedTags.ContainsKey(tag) && !cachedTags[tag].Contains(this))
            {
                cachedTags[tag].Add(this);
            }
            else
            {
                cachedTags.Add(tag, new List<ObjectTags>() { this });
            }
        }

        /// <summary>
        /// Removes specified tag to the attached GameObject
        /// </summary>
        public void RemoveTagFromGO(string tag) 
        {
            if (tag == "") { return; }
            if (!_objectTags.Contains(tag)) { return; }
            _objectTags.Remove(tag);
            Component[] components = GetComponents<Component>();
            foreach (Component c in components) 
            {
                if (IsComponentTagged(tag, c)) { return; }
            }
            cachedTags[tag].Remove(this);
        }

        /// <summary>
        /// Removes specified tag to the specified Component
        /// </summary>
        public void RemoveTagFromComponent(string tag, Component c)
        {
            if (tag == "") { return; }
            if (!_componentTags.ContainsTagsForComponent(c))
            {
                if (c.gameObject == gameObject) { _componentTags.Add(new ComponentTags() {targetComponent = c, componentTags = new List<string>()}); }
                else { return; }
            }
            if (!_componentTags.GetTagListByComponent(c).Contains(tag)) { return; }
            _componentTags.GetTagListByComponent(c).Remove(tag);
            if (_objectTags.Contains(tag)) { return; }
            Component[] components = GetComponents<Component>();
            foreach (Component otherCs in components)
            {
                if (IsComponentTagged(tag, otherCs)){ return; }
            }
            cachedTags[tag].Remove(this);
        }

        #endregion

        #region Retrievers

        /// <summary>
        /// Get the first GameObject tagged with the specified tag
        /// </summary>
        public static GameObject GetFirstGameObjectWithTag(string tag, bool includeInactive)
        {
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                if(!cachedTags.ContainsKey(tag)){ return null; }
                foreach(ObjectTags ot in cachedTags[tag])
                {
                    if(!ot.gameObject.activeInHierarchy && !includeInactive) { continue; }
                    if(ot.IsObjectTagged(tag)) { return ot.gameObject; }
                }
            }
            else
            {
                foreach(ObjectTags ot in FindObjectsOfType<ObjectTags>(includeInactive))
                {
                    if(ot.IsObjectTagged(tag)) { return ot.gameObject; }
                }
            }
            return null;
        }

        /// <summary>
        /// Get all GameObjects tagged with the specified tag
        /// </summary>
        public static GameObject[] GetAllGameObjectsWithTag(string tag, bool includeInactive = false)
        {
            List<GameObject> results = new List<GameObject>();
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                if(!cachedTags.ContainsKey(tag)) { return results.ToArray(); }
                foreach(ObjectTags ot in cachedTags[tag])
                {
                    if(!ot.gameObject.activeInHierarchy && !includeInactive) { continue; }
                    if(ot.IsObjectTagged(tag)) { results.Add(ot.gameObject); }
                }
            }
            else
            {
                foreach(ObjectTags ot in FindObjectsOfType<ObjectTags>(includeInactive))
                {
                    if(ot.IsObjectTagged(tag)) { results.Add(ot.gameObject); }
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Gets the first component that is tagged with the specified tag
        /// </summary>
        public static T GetFirstComponentWithTag<T>(string tag, bool includeInactive = false) where T : Component
        {
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                if(!cachedTags.ContainsKey(tag)){ return null; }
                foreach(ObjectTags ot in cachedTags[tag])
                {
                    if(!ot.gameObject.activeInHierarchy && !includeInactive) { continue; }
                    foreach(ComponentTags cTags in ot._componentTags)
                    {
                        var cType = cTags.targetComponent.GetType();
                        bool componentEnabled = !(typeof(Behaviour).IsAssignableFrom(cType) && !((Behaviour)cTags.targetComponent).enabled);
                        if(typeof(T).IsAssignableFrom(cType) && (componentEnabled || includeInactive)) //keep disabled behaviors from showing up if includeInactive is false
                        {
                            if(cTags.componentTags.Contains(tag))
                            {
                                return (T)cTags.targetComponent;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach(ObjectTags ot in FindObjectsOfType<ObjectTags>(includeInactive))
                {
                    foreach(ComponentTags cTags in ot._componentTags)
                    {
                        var cType = cTags.targetComponent.GetType();
                        bool componentEnabled = !(typeof(Behaviour).IsAssignableFrom(cType) && !((Behaviour)cTags.targetComponent).enabled);
                        if(typeof(T).IsAssignableFrom(cType) && (componentEnabled || includeInactive)) //keep disabled behaviors from showing up if includeInactive is false
                        {
                            if(cTags.componentTags.Contains(tag))
                            {
                                return (T)cTags.targetComponent;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all components that are tagged with the specified tag
        /// </summary>
        public static T[] GetAllComponentsWithTag<T>(string tag, bool includeInactive = false) where T : Component
        {
            List<T> results = new List<T>();
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                if(!cachedTags.ContainsKey(tag)){ return results.ToArray(); }
                foreach(ObjectTags ot in cachedTags[tag])
                {
                    if(!ot.gameObject.activeInHierarchy && !includeInactive) { continue; }
                    foreach(ComponentTags cTags in ot._componentTags)
                    {
                        var cType = cTags.targetComponent.GetType();
                        bool componentEnabled = !(typeof(Behaviour).IsAssignableFrom(cType) && !((Behaviour)cTags.targetComponent).enabled);
                        if(typeof(T).IsAssignableFrom(cType) && (componentEnabled || includeInactive)) //keep disabled behaviors from showing up if includeInactive is false
                        {


                            if(cTags.componentTags.Contains(tag))
                            {
                                results.Add((T)cTags.targetComponent);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach(ObjectTags ot in FindObjectsOfType<ObjectTags>(includeInactive))
                {
                    foreach(ComponentTags cTags in ot._componentTags)
                    {
                        var cType = cTags.targetComponent.GetType();
                        bool componentEnabled = !(typeof(Behaviour).IsAssignableFrom(cType) && !((Behaviour)cTags.targetComponent).enabled);
                        if(typeof(T).IsAssignableFrom(cType) && (componentEnabled || includeInactive)) //keep disabled behaviors from showing up if includeInactive is false
                        {
                            if(cTags.componentTags.Contains(tag))
                            {
                                results.Add((T)cTags.targetComponent);
                            }
                        }
                    }
                }
            }
            return results.ToArray();
        }

        #endregion 

        #region Static Cache Handler Methods
        private static void ConfirmTagListLoaded()
        {
            if(_trackedTags == null)
            {
                string rPath = tagListPath;
                rPath = rPath.Substring(rPath.IndexOf("/Resources/") + "/Resources/".Length);
                TagListAsset tagList = Resources.Load<TagListAsset>($"{rPath}/{tagListName}");
                _trackedTags = new List<TagListAsset.TagInfo>();
                if(tagList)
                {
                    foreach(TagListAsset.TagInfo tagInfo in tagList.tags)
                    {
                        _trackedTags.Add(tagInfo);
                    }
                    Debug.Log($"RTags: Loaded Tag List {tagList} from {tagListPath}/{tagListName}");
                }
            }
        }

        public static bool IsTagCached(string tag)
        {
            foreach(TagListAsset.TagInfo tagInfo in TrackedTags)
            {
                if(tagInfo.tagName == tag)
                {
                    return tagInfo.isPreCached;
                }
            }
            return false;
        }

        public static bool IsTagTracked(string tag)
        {
            foreach(TagListAsset.TagInfo tagInfo in TrackedTags)
            {
                if(tagInfo.tagName == tag)
                {
                    return true;
                }
            }
            return false;
        }

        public static void SetTagCacheMode(string tag, bool useCache)
        {
            ConfirmTagListLoaded();
            for (int i = 0; i < _trackedTags.Count; i++)
            {
                if(_trackedTags[i].tagName == tag)
                {
                    bool wasCached = _trackedTags[i].isPreCached;
                    _trackedTags[i] = new TagListAsset.TagInfo() {tagName = tag, isPreCached = useCache};
                    if(_trackedTags[i].isPreCached && !wasCached)
                    {
                        needsCacheTags.Add(tag);
                    }
                    else if(!_trackedTags[i].isPreCached)
                    {
                        if(wasCached)
                        {
                            needsCacheTags.Remove(tag);
                        }
                        DumpCacheForTag(tag);
                    }
                    return;
                }
            }
            Debug.LogError("Trying to change the cache mode on a non tracked tag. Make sure the tag is tracked before trying to modify it's cache mode.");
        }

        private static void DumpCacheForTag(string tag)
        {
            if(cachedTags.ContainsKey(tag))
            {
                cachedTags.Remove(tag);
            }
        }

        //Makes sure the tag is cached if needed and return true if the tag is cached
        private static bool ConfirmTagCacheState(string tag)
        {
            if(!IsTagCached(tag)) { return false; }
            CacheNewTag(tag, true);
            return true;
        }

        private static void CacheNewTag(string tag, bool skipCachedCheck = false)
        {
            if(skipCachedCheck || !IsTagCached(tag)){ return; }
            ObjectTags[] oTagsLoaded = GameObject.FindObjectsOfType<ObjectTags>(true);
            if(!cachedTags.ContainsKey(tag)){ cachedTags.Add(tag, new List<ObjectTags>()); }
            foreach(ObjectTags ot in oTagsLoaded)
            {
                if(cachedTags[tag].Contains(ot)){ continue; }
                if(ot._objectTags.Contains(tag))
                {
                    cachedTags[tag].Add(ot); 
                    continue;
                }
                foreach(ComponentTags cTags in ot._componentTags)
                {
                    if(cTags.componentTags.Contains(tag))
                    {
                        cachedTags[tag].Add(ot);
                        break;
                    }
                }
            }
        }

        public static void TrackNewTag(string tag, bool useCache = false)
        {
            ConfirmTagListLoaded();
            if(IsTagTracked(tag))
            {
                SetTagCacheMode(tag, useCache);
            }
            else
            {
                _trackedTags.Add(new TagListAsset.TagInfo() {tagName = tag, isPreCached = useCache});
            }
        }
        #endregion
    }
}
