using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    private PlayerController playerController;
    private GameObject platform;

    void Awake()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable")) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("PlatformOneWay"))
        {
            platform = other.gameObject;
        }
        playerController.isGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable")) return;
        platform = null;
        playerController.isGrounded = false;
    }

    private void Update()
    {
        // Dropping from one way platforms
        if (platform && Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine("DisablePlatform");
        }
    }

    IEnumerator DisablePlatform()
    {
        GameObject savedPlatform = platform;
        savedPlatform.SetActive(false);
        yield return new WaitForSeconds(0.4f);
        savedPlatform.SetActive(true);
    }
}
