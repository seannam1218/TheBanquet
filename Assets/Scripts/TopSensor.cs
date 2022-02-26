using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopSensor : MonoBehaviour
{
    private PlayerController playerController;

    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("WallCheck");
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable") || other.gameObject.CompareTag("Sentinel")) return;
        playerController.isBlockedOnTop = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable") || other.gameObject.CompareTag("Sentinel")) return;
        playerController.isBlockedOnTop = false;
    }
}
