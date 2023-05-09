using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTags;
using System.Diagnostics;

public class NSizeSpeedTest : MonoBehaviour
{
    public int nSize = 100;
    public int unTaggedNoiseSize = 100;
    public int taggedNoiseSize = 100;

    public int attempts = 5;


    // Start is called before the first frame update
    void Start()
    {
        ObjectTags.TrackNewTag("NSizeTest", false);
        for (int i = 0; i < nSize; i++)
        {
            GameObject newGO = new GameObject();
            newGO.AddTag("NSizeTest");
            newGO.tag = "NSizeTest";
        }
        for (int i = 0; i < unTaggedNoiseSize; i++)
        {
            GameObject newGO = new GameObject();
        }
        for (int i = 0; i < taggedNoiseSize; i++)
        {
            GameObject newGO = new GameObject();
            newGO.AddTag("noiseTag");
            newGO.tag = "noiseTag";
        }
        for (int i = 0; i < attempts; i++)
        {
            TestDefault();
            TestRTags(false);
            TestRTags(true);
            
        }
    }

    private void TestDefault()
    {
        //Stopwatch sw = new Stopwatch();
        long startTicks = System.DateTime.Now.Ticks;
        //sw.Start();
        
        GameObject[] objs =  GameObject.FindGameObjectsWithTag("NSizeTest");

        long endTicks = System.DateTime.Now.Ticks;
        //sw.Stop();
        if(objs.Length != nSize) {UnityEngine.Debug.Log("Failed");}
        UnityEngine.Debug.Log("Built in tag time:" + (endTicks - startTicks));
    }

    private void TestRTags(bool cached)
    {
        ObjectTags.SetTagCacheMode("NSizeTest", cached);
        ObjectTags.ForceNewCacheLoad();
        // Stopwatch sw = new Stopwatch();
        long startTicks = System.DateTime.Now.Ticks;
        // sw.Start();
        
        GameObject[] objs =  ObjectTags.GetAllGameObjectsWithTag("NSizeTest");

        long endTicks = System.DateTime.Now.Ticks;
        // sw.Stop();
        if(objs.Length != nSize) {UnityEngine.Debug.Log("Failed");}
        string cacheMode = cached ? "cached" : "notCached";
        UnityEngine.Debug.Log($"RTags time ({cacheMode}):" + (endTicks - startTicks));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
