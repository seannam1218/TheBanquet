using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertRadius : MonoBehaviour
{
    private GameObject threat;
    private bool isAttacked;

    // Update is called once per frame
    void FixedUpdate()
    {
        threat = transform.parent.gameObject.GetComponent<Dummy>().threat;
        isAttacked = transform.parent.gameObject.GetComponent<Dummy>().isAttacked;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isAttacked && collision.gameObject.GetComponent<Sentinel>())
        {
            collision.gameObject.GetComponent<Sentinel>()?.ActivateKillMode(threat);
        }
    }
}
