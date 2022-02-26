using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightStand : MonoBehaviour
{
    public GameObject innerLightObject;
    public GameObject outerLightObject;
    public GameObject audioSource;
    Light2D innerLight;
    Light2D outerLight;
    float innerLightIntensity;
    float outerLightIntensity;
    int OnOff;

    // Start is called before the first frame update
    void Start()
    {
        innerLight = innerLightObject.GetComponent<Light2D>();
        outerLight = outerLightObject.GetComponent<Light2D>();
        innerLightIntensity = innerLight.intensity;
        outerLightIntensity = outerLight.intensity;

        OnOff = 1; //turned on by default when game starts
    }

    public void Interact()
    {
        audioSource.GetComponent<AudioManager>().Play("Switch");
        OnOff = -(OnOff - 1);
        innerLight.intensity = OnOff * innerLightIntensity;
        outerLight.intensity = OnOff * outerLightIntensity;
    }
}
