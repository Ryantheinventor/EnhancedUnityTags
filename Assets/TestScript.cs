using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTags;

public class TestScript : MonoBehaviour
{
    public Tag velocityTag = (Tag)"DisplayVelocity";

    public GameObject testObject;

    private IEnumerator Start() 
    {
        StartCoroutine(HideNewTagedObjects());
        StartCoroutine(ChangeCacheMode());
        while(true)
        {
            yield return new WaitForSeconds(1.5f);
            Instantiate(testObject).transform.position = new Vector3(Random.Range(-0.5f,0.5f),5,Random.Range(-0.5f,0.5f));
        }
    }

    private IEnumerator HideNewTagedObjects()
    {
        while(true)
        {    
            yield return new WaitForSeconds(5);
            foreach(GameObject go in ObjectTags.GetAllGameObjectsWithTag("CreatedTag", true))
            {
                go.SetActive(!go.activeInHierarchy);
            }
        }
    }

    private IEnumerator ChangeCacheMode()
    {
        yield return new WaitForSeconds(20f);
        Debug.Log("Changing cache");
        Debug.Log(ObjectTags.IsTagTracked("CreatedTag"));
        ObjectTags.TrackNewTag("CreatedTag");
        Debug.Log(ObjectTags.IsTagTracked("CreatedTag"));
        ObjectTags.SetTagCacheMode("CreatedTag", true);
    }

    private void Update() 
    {
        Rigidbody[] rbs = ObjectTags.GetAllComponentsWithTag<Rigidbody>(velocityTag);
        Debug.Log(rbs.Length);
        foreach(Rigidbody rb in rbs)
        {
            Debug.DrawLine(rb.position, rb.position + rb.velocity, Color.red);
        }
    }

    void OnDrawGizmos() 
    {
        foreach(Rigidbody rb in ObjectTags.GetAllComponentsWithTag<Rigidbody>(velocityTag, true))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(rb.position, new Vector3(0.5f,0.5f,0.5f));
        }
    }

}
