using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTags;
public class InheritenceTest : MonoBehaviour
{
    public Tag iTag;//don't let apple see this

    public Level iLevel = Level.A;
    public enum Level
    {
        A,B,C,
    }

    // Start is called before the first frame update
    void Start()
    {
        switch(iLevel)
        {
            case Level.A:
                Debug.Log(ObjectTags.GetAllComponentsWithTag<A>(iTag).Length);
                break;
            case Level.B:
                Debug.Log(ObjectTags.GetAllComponentsWithTag<B>(iTag).Length);
                break;
            case Level.C:
                Debug.Log(ObjectTags.GetAllComponentsWithTag<C>(iTag).Length);
                break;

        }
    }
}
