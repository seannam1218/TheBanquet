using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : MonoBehaviour
{
    public void SetChildrenColor(Color c)
    {
        foreach (SpriteRenderer child in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (child.transform.name != gameObject.name) child.color = c;
        }
    }
}
