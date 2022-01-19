using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Agent")
        {
            Vector4 potionColor = (Vector4)gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color;
            Utils.SetChildrenColor(collision.gameObject, potionColor);
            Destroy(gameObject);
        }
    }
}
