using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + (float)Mathf.Exp(-value));
    }

    public static float GetAimAngle(Vector3 originPos, Vector3 mouseWorldPos, bool turningEnabled)
    {
        Vector2 originPos2 = originPos;
        Vector2 mouseWorldPos2 = mouseWorldPos;
        Vector2 hypotenuse = mouseWorldPos - originPos;

        float newAngle = Mathf.Atan(hypotenuse.y / hypotenuse.x) * Mathf.Rad2Deg;
        if (hypotenuse.x < 0)
            if (!turningEnabled)
            {
                newAngle += 180f;
            }
            else {
                newAngle = -newAngle;
            }

        return newAngle;
    }

    public static void SetChildrenColor(GameObject gameObject, Color c)
    {
        foreach (SpriteRenderer child in gameObject.transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (child.transform.name != gameObject.name)
            {
                if (child.transform.parent.transform.name == "Weapon") continue;
                child.color = c;
            }
        }
    }
}
