using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    [SerializeField] GameObject head;

    // Start is called before the first frame update
    void Start()
    {
        // Turn spawned dummy head randomly
        float randomAngle = Random.Range(-65f, 65f);
        head.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, randomAngle));
    }

    // Update is called once per frame
    void Update()
    {
        // If sun is up, move around, if sun is down, stay frozen.
    }
}
