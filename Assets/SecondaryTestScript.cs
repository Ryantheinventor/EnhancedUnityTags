using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTags;

public class SecondaryTestScript : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
        //GetComponent<ObjectTags>().AddTagToGO("CreatedTag");
    }
}
