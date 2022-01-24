using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightSensor : MonoBehaviour
{
    public PlayerController playerController;

    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        playerController.isBlockedOnRight = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playerController.isBlockedOnRight = false;
    }
}
