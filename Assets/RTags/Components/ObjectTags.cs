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
        //private static Dictionary<string, List<ObjectTags>> cachedTags = new Dictionary<string, List<ObjectTags>>();
        private static Dictionary<string, Dictionary<System.Type, List<Component>>> cache = new Dictionary<string, Dictionary<System.Type, List<Component>>>();
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
                    var workingType = typeof(ObjectTags);
                    if(!cache.ContainsKey(tag)){cache.Add(tag, new Dictionary<System.Type, List<Component>>());}
                    if(!cache[tag].ContainsKey(workingType)){cache[tag].Add(workingType, new List<Component>());}
                    if(!cache[tag][workingType].Contains(this))
                    {
                        cache[tag][workingType].Add(this);
                    }
                }
            }
            foreach(ComponentTags tagSet in _componentTags)
            {
                foreach(string tag in tagSet.componentTags)
                {
                    if (tag != "" && IsTagCached(tag))
                    {    
                        var workingType = tagSet.targetComponent.GetType();
                        if(!cache.ContainsKey(tag)){cache.Add(tag, new Dictionary<System.Type, List<Component>>());}
                        if(!cache[tag].ContainsKey(workingType)){cache[tag].Add(workingType, new List<Component>());}
                        if(!cache[tag][workingType].Contains(tagSet.targetComponent))
                        {
                            cache[tag][workingType].Add(tagSet.targetComponent);
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
                    var workingType = typeof(ObjectTags);
                    if(!cache.ContainsKey(tag)){continue;}
                    if(!cache[tag].ContainsKey(workingType)){continue;}
                    cache[tag][workingType].Remove(this);
                }
            }
            foreach(ComponentTags tagSet in _componentTags)
            {
                foreach(string tag in tagSet.componentTags)
                {
                    if (tag != "" && IsTagCached(tag))
                    {    
                        var workingType = tagSet.targetComponent.GetType();
                        if(!cache.ContainsKey(tag)){continue;}
                        if(!cache[tag].ContainsKey(workingType)){continue;}
                        cache[tag][workingType].Remove(tagSet.targetComponent);
                    }
                }
            }
        }

        /// <summary>
        /// Gets an array of all tags assigned to the object
        /// </summary>
        public string[] GetObjectTags()
        {
            return _objectTags.ToArray();
        }

        public string[] GetComponentTags(Component component)
        {
            if(_componentTags.ContainsTagsForComponent(component))
            {
                return _componentTags.GetTagListByComponent(component).ToArray();
            }
            return new string[0];
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
            var workingType = typeof(ObjectTags);
            if(!cache.ContainsKey(tag)){cache.Add(tag, new Dictionary<System.Type, List<Component>>());}
            if(!cache[tag].ContainsKey(workingType)){cache[tag].Add(workingType, new List<Component>());}
            if(!cache[tag][workingType].Contains(this))
            {
                cache[tag][workingType].Add(this);
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
            var workingType = c.GetType();
            if(!cache.ContainsKey(tag)){cache.Add(tag, new Dictionary<System.Type, List<Component>>());}
            if(!cache[tag].ContainsKey(workingType)){cache[tag].Add(workingType, new List<Component>());}
            if(!cache[tag][workingType].Contains(this))
            {
                cache[tag][workingType].Add(this);
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
            var workingType = typeof(ObjectTags);
            if(!cache.ContainsKey(tag)){return;}
            if(!cache[tag].ContainsKey(workingType)){return;}
            cache[tag][workingType].Remove(this);
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
            var workingType = c.GetType();
            if(!cache.ContainsKey(tag)){return;}
            if(!cache[tag].ContainsKey(workingType)){return;}
            cache[tag][workingType].Remove(c);
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
                var workingType = typeof(ObjectTags);
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                if(!cache.ContainsKey(tag)){ return null; }
                if(!cache[tag].ContainsKey(workingType)){ return null; }
                if(cache[tag][workingType].Count == 0) { return null; }
                return cache[tag][workingType][0].gameObject;
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
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                var workingType = typeof(ObjectTags);
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                if(!cache.ContainsKey(tag)) { return new GameObject[0]; }
                if(!cache[tag].ContainsKey(workingType)){ return new GameObject[0]; }
                if(cache[tag][workingType].Count == 0) { return new GameObject[0]; }
                List<GameObject> results = new List<GameObject>();
                for (int i = 0; i < cache[tag][workingType].Count; i++)
                {
                    GameObject go = cache[tag][workingType][i].gameObject;
                    if(go.activeInHierarchy || includeInactive)
                    {
                        results.Add(cache[tag][workingType][i].gameObject);
                    }
                }
                return results.ToArray();
            }
            else
            {
                List<GameObject> results = new List<GameObject>();
                foreach(ObjectTags ot in FindObjectsOfType<ObjectTags>(includeInactive))
                {
                    if(ot.IsObjectTagged(tag)) { results.Add(ot.gameObject); }
                }
                return results.ToArray();
            }
            
        }

        /// <summary>
        /// Gets the first component that is tagged with the specified tag
        /// </summary>
        public static T GetFirstComponentWithTag<T>(string tag, bool includeInactive = false) where T : Component
        {
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                return GetAllValidMatchesFromCache<T>(tag)[0] as T;
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
            
            if(Application.isPlaying && ConfirmTagCacheState(tag))
            {
                var workingType = typeof(T);
                if(includeInactive) {Debug.LogWarning("Objects that have not been active before can not be in the cache and will not show up in the results, if you need to get said objects, you should use a non cached tag"); }
                List<T> results = new List<T>();
                foreach(Component c in GetAllValidMatchesFromCache<T>(tag))
                {
                    results.Add(c as T);
                }
                return results.ToArray();
            }
            else
            {
                List<T> results = new List<T>();
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
                return results.ToArray();
            }
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
            if(cache.ContainsKey(tag))
            {
                cache.Remove(tag);
            }
        }

        /// <summary>
        /// When a new tag is set to cache it does not automaticaly get cached calling this will force all waiting cached tags to have their cache loaded
        /// </summary>
        public static void ForceNewCacheLoad()
        {
            foreach(string tag in needsCacheTags)
            {
                CacheNewTag(tag, true);
            }
            needsCacheTags.Clear();
        }

        //Makes sure the tag is cached if needed and return true if the tag is cached
        private static bool ConfirmTagCacheState(string tag)
        {
            if(!IsTagCached(tag)) { return false; }
            if(needsCacheTags.Contains(tag))
            {
                CacheNewTag(tag, true);
                needsCacheTags.Remove(tag);
            }
            return true;
        }

        private static void CacheNewTag(string tag, bool skipCachedCheck = false)
        {
            if(!skipCachedCheck && !IsTagCached(tag)){ return; }
            ObjectTags[] oTagsLoaded = GameObject.FindObjectsOfType<ObjectTags>(true);
            if(!cache.ContainsKey(tag)){ cache.Add(tag, new Dictionary<System.Type, List<Component>>()); }
            foreach(ObjectTags ot in oTagsLoaded)
            {
                if(ot._objectTags.Contains(tag))
                {
                    var workingType = typeof(ObjectTags);
                    if(!cache[tag].ContainsKey(workingType)) { cache[tag].Add(workingType, new List<Component>()); }
                    cache[tag][workingType].Add(ot);
                }
                foreach(ComponentTags cTags in ot._componentTags)
                {
                    if(cTags.componentTags.Contains(tag))
                    {
                        var workingType = cTags.targetComponent.GetType();
                        if(!cache[tag].ContainsKey(workingType)) { cache[tag].Add(workingType, new List<Component>()); }
                        cache[tag][workingType].Add(cTags.targetComponent);
                    }   
                }
            }
        }

        private static List<Component> GetAllValidMatchesFromCache<T>(string tag) where T : Component
        {
            List<Component> results = new List<Component>();
            if(!cache.ContainsKey(tag)){return results;}
            foreach(KeyValuePair<System.Type, List<Component>> dict in cache[tag])
            {
                if(typeof(T).IsAssignableFrom(dict.Key))
                {
                    results.AddRange(dict.Value);
                }
            }
            return results;
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
                if(useCache) { needsCacheTags.Add(tag); }
            }
        }
        #endregion
    }
}
