using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopSensor : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("WallCheck");
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        playerController.isBlockedOnTop = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playerController.isBlockedOnTop = false;
    }
}
