using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Flashlight : MonoBehaviour
{
    Light2D light;
    float outerAngle;
    float innerAngle;
    float outerRadius;
    float innerRadius;
    float intensity;

    // Start is called before the first frame update
    void Start()
    {
        light = transform.GetComponent<Light2D>();
        outerAngle = light.pointLightOuterAngle;
        innerAngle = light.pointLightInnerAngle;
        outerRadius = light.pointLightOuterRadius;
        innerRadius = light.pointLightInnerRadius;
        intensity = light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        float distMouseTarget = (float)Mathf.Sqrt(Mathf.Pow(mousePos.x / Screen.width - 0.5f, 2) + Mathf.Pow(mousePos.y / Screen.height - 0.5f, 2));
        distMouseTarget = Mathf.Max(distMouseTarget, 0.15f);

        var angle = Utils.GetAimAngle(transform.position, mouseWorldPos, false);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle - 90)), 2.5f*Time.deltaTime);

        float lightAngleModifier = distMouseTarget * 1.5f + 0.1f;
        light.pointLightOuterAngle = outerAngle / lightAngleModifier;
        light.pointLightInnerAngle = innerAngle / lightAngleModifier;
        light.pointLightOuterRadius = outerRadius * (0.2f + distMouseTarget*3.2f);
        light.pointLightInnerRadius = innerRadius * (0.2f + distMouseTarget*3.2f);
        light.intensity = Mathf.Max(intensity - distMouseTarget*4f, 0.2f);
    }
}
