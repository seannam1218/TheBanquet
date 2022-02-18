using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.GetComponent<PlayerStatus>()?.ChangeHunger(50f);
            Destroy(gameObject);
        }
    }
}
