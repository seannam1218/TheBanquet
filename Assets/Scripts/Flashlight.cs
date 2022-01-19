using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Photon.Pun;

public class Flashlight : MonoBehaviourPun
{
    Light2D light;
    float outerAngle;
    float innerAngle;
    float outerRadius;
    float innerRadius;
    float intensity;

    private float savedOuterAngle;
    private float savedInnerAngle;
    private float savedOuterRadius;
    private float savedInnerRadius;
    private float savedIntensity;

    private float networkOuterAngle;
    private float networkInnerAngle;
    private float networkOuterRadius;
    private float networkInnerRadius;
    private float networkIntensity;

    // Start is called before the first frame update
    void Awake()
    {
        light = transform.GetComponent<Light2D>();

        outerAngle = 20f;
        innerAngle = 10f;
        outerRadius = 12f;
        innerRadius = 6f;
        intensity = 1.8f;

        savedOuterAngle = outerAngle;
        savedInnerAngle = innerAngle;
        savedOuterRadius = outerRadius;
        savedInnerRadius = innerRadius;
        savedIntensity = intensity;

        networkOuterAngle = outerAngle;
        networkInnerAngle = innerAngle;
        networkOuterRadius = outerRadius;
        networkInnerRadius = innerRadius;
        networkIntensity = intensity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (!photonView.IsMine)
        {
            this.light.pointLightOuterAngle = networkOuterAngle;
            this.light.pointLightInnerAngle = networkInnerAngle;
            this.light.pointLightOuterRadius = networkOuterRadius;
            this.light.pointLightInnerRadius = networkInnerRadius;
            this.light.intensity = networkIntensity;
            return;
        }

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        float distMouseTarget = (float)Mathf.Sqrt(Mathf.Pow(mousePos.x / Screen.width - 0.5f, 2) + Mathf.Pow(mousePos.y / Screen.height - 0.5f, 2));
        distMouseTarget = Mathf.Max(distMouseTarget, 0.15f);

        var angle = Utils.GetAimAngle(transform.position, mouseWorldPos, false);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle - 90)), 2.5f * Time.deltaTime);

        float lightAngleModifier = distMouseTarget * 1.5f + 0.1f;
        savedOuterAngle = outerAngle / lightAngleModifier;
        savedInnerAngle = innerAngle / lightAngleModifier;
        savedOuterRadius = outerRadius * (0.2f + distMouseTarget * 3.2f);
        savedInnerRadius = innerRadius * (0.2f + distMouseTarget * 3.2f);
        savedIntensity = Mathf.Max(intensity - distMouseTarget * 4f, 0.2f);

        this.light.pointLightOuterAngle = savedOuterAngle;
        this.light.pointLightInnerAngle = savedInnerAngle;
        this.light.pointLightOuterRadius = savedOuterRadius;
        this.light.pointLightInnerRadius = savedInnerRadius;
        this.light.intensity = savedIntensity;

        photonView.RPC("AdjustLightRpc", RpcTarget.All, savedOuterAngle, savedInnerAngle, savedOuterRadius, savedInnerRadius, savedIntensity);
    }

    [PunRPC]
    public void AdjustLightRpc(float savedOuterAngle, float savedInnerAngle, float savedOuterRadius, float savedInnerRadius, float savedIntensity)
    {
        this.networkOuterAngle = savedOuterAngle;
        this.networkInnerAngle = savedInnerAngle;
        this.networkOuterRadius = savedOuterRadius;
        this.networkInnerRadius = savedInnerRadius;
        this.networkIntensity = savedIntensity;
    }
}

