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

    List<GameObject> testGOs = new List<GameObject>();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        ObjectTags.TrackNewTag("NSizeTest", false);
        
        for (int i = 0; i < attempts; i++)
        {
            GenerateTestGOS();
            yield return null;
            TestDefault();
            yield return null;
            TestRTags(false);
            yield return null;
            TestRTags(true);
            
        }
        yield return null;
    }

    private void GenerateTestGOS()
    {
        for (int i = 0; i < nSize; i++)
        {
            GameObject newGO = new GameObject($"{Random.Range(int.MinValue, int.MaxValue)}");
            newGO.AddComponent<SpriteRenderer>();
            newGO.AddComponent<BoxCollider>();
            newGO.AddComponent<Rigidbody>().AddTag("NSizeTest");
            newGO.tag = "NSizeTest";
            testGOs.Add(newGO);
        }
        for (int i = 0; i < unTaggedNoiseSize; i++)
        {
            GameObject newGO = new GameObject($"{Random.Range(int.MinValue, int.MaxValue)}");
            newGO.AddComponent<SpriteRenderer>();
            newGO.AddComponent<BoxCollider>();
            newGO.AddComponent<Rigidbody>();
            testGOs.Add(newGO);
        }
        for (int i = 0; i < taggedNoiseSize; i++)
        {
            GameObject newGO = new GameObject($"{Random.Range(int.MinValue, int.MaxValue)}");
            newGO.AddComponent<SpriteRenderer>();
            newGO.AddComponent<BoxCollider>();
            newGO.AddComponent<Rigidbody>().AddTag("noiseTag");
            newGO.tag = "noiseTag";
            testGOs.Add(newGO);
        }
    }

    private void TestDefault()
    {
        //Stopwatch sw = new Stopwatch();
        long startTicks = System.DateTime.Now.Ticks;
        //sw.Start();
        
        GameObject[] objs =  GameObject.FindGameObjectsWithTag("NSizeTest");
        List<Vector3> velocities = new List<Vector3>();
        foreach(GameObject obj in objs)
        {
            velocities.Add(obj.GetComponent<Rigidbody>().velocity);
        }


        long endTicks = System.DateTime.Now.Ticks;
        //sw.Stop();
        if(objs.Length != nSize) {UnityEngine.Debug.Log($"Failed-Found {objs.Length}/{nSize}");}
        UnityEngine.Debug.Log("Built in tag time:" + (endTicks - startTicks));
    }

    private void TestRTags(bool cached)
    {
        ObjectTags.SetTagCacheMode("NSizeTest", cached);
        ObjectTags.ForceNewCacheLoad();
        // Stopwatch sw = new Stopwatch();
        long startTicks = System.DateTime.Now.Ticks;
        // sw.Start();
        
        Rigidbody[] objs =  ObjectTags.GetAllComponentsWithTag<Rigidbody>("NSizeTest");
        List<Vector3> velocities = new List<Vector3>();
        foreach(Rigidbody rb in objs)
        {
            velocities.Add(rb.velocity);
        }

        long endTicks = System.DateTime.Now.Ticks;
        // sw.Stop();
        if(objs.Length != nSize) {UnityEngine.Debug.Log($"Failed-Found {objs.Length}/{nSize}");}
        string cacheMode = cached ? "cached" : "notCached";
        UnityEngine.Debug.Log($"RTags time ({cacheMode}):" + (endTicks - startTicks));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
