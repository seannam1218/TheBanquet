using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FovCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.parent.position;
        GetComponent<Camera>().orthographicSize = transform.parent.GetComponent<Camera>().orthographicSize;
    }
}
