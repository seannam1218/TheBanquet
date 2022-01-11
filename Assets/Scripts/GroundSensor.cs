using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    public PlayerController playerController;

    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        playerController.isGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playerController.isGrounded = false;
    }
}
