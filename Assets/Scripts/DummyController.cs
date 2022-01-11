using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonoBehaviour
{
    public void SetDummyColor(Color c)
    {
        foreach (SpriteRenderer child in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            child.color = c;
        }
    }
}
