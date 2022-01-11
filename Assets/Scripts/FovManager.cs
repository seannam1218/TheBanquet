using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FovManager : NetworkBehaviour
{
    [SerializeField] GameObject fov;
    FieldOfView fovScript;

    // Start is called before the first frame update
    void Start()
    {
        fovScript = fov.transform.GetComponent<FieldOfView>();
        if (IsClient && IsOwner)
        {
            fov.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);

        fovScript.SetOrigin(transform.parent.position);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        fovScript.SetAimAngle(transform.parent.position, mouseWorldPos);
    }
}
