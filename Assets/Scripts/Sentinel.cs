using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Experimental.Rendering.Universal;

public class Sentinel : MonoBehaviour
{
    public AIPath aiPath;
    public Light2D light;
    public GameObject audioSource;

    private GameObject threat = null;
    private float time;
    private bool killMode = false; 

    private float killModeMaxSeconds = 9;
    private float killModeCountDown = 9;

    private float wanderMaxSeconds = 15;
    private float wanderCountDown = 15;

    private void Start()
    {
        aiPath = transform.GetComponent<AIPath>();
    }

    private void FixedUpdate()
    {
        if (threat) aiPath.destination = threat.transform.position;

        if (killModeCountDown < 0)
        {
            killMode = false;
            killModeCountDown = killModeMaxSeconds;
        }

        if (killMode)
        {
            audioSource.GetComponent<AudioManager>().Play("Angry Drone");
            Color red = new Color(255, 98, 66);
            Blink(red, 0f, 20);
            killModeCountDown -= Time.deltaTime;
        }
        else
        {
            audioSource.GetComponent<AudioManager>().Play("Drone");
            Color blue = new Color(66, 138, 255);
            Blink(blue, 0.8f, 5);
            Wander();
        }
    }

    private void Wander()
    {
        wanderCountDown -= Time.deltaTime;
        if (wanderCountDown < 0)
        {
            float newX = Random.Range(transform.position.x - 10, transform.position.x + 10);
            //newX = Mathf.Clamp(newX, )
            float newY = Random.Range(transform.position.y - 10, transform.position.y + 10);

            Vector3 newDestination = new Vector3(newX, newY, 0);
            aiPath.destination = newDestination;
            wanderCountDown = Random.Range(4f, wanderMaxSeconds);
        }
    }


    private void Blink(Color color, float blinkThreshold, float blinkSpeed)
    {
        light.color = color;

        time += Time.deltaTime;
        time = time % (20 * Mathf.PI);
        float y = Mathf.Sin(blinkSpeed * time);
        if (y > blinkThreshold)
        {
            light.intensity = 0.02f;
        }
        else
        {
            light.intensity = 0f;
        }
    }

    public void ActivateKillMode(GameObject obj)
    {
        killMode = true;
        threat = obj;
        StartCoroutine("ResetThreat");
    }

    IEnumerator ResetThreat()
    {
        yield return new WaitForSeconds(9f);
        threat = null;
        killMode = false;
    }
}
