using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftSensor : MonoBehaviour
{
    public PlayerController playerController;

    void Start()
    {
        gameObject.layer = 6;
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        playerController.isBlockedOnLeft = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playerController.isBlockedOnLeft = false;
    }
}
