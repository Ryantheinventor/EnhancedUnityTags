using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTags;

public class SecondaryTestScript : MonoBehaviour
{
    private IEnumerator Start()
    {
        GetComponent<ObjectTags>().AddTagToGO("CreatedTag");
        yield return new WaitForSeconds(5);
    }
}
