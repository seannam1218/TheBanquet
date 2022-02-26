using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    [SerializeField] public GameObject head;
    [HideInInspector] public GameObject threat;
    [HideInInspector] public bool isAttacked = false;
    public GameObject audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // Turn spawned dummy head randomly
        float randomAngle = Random.Range(-65f, 65f);
        head.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, randomAngle));
    }

    // Update is called once per frame
    void Update()
    {
        // If sun is up, move around, if sun is down, stay frozen.

    }

    public void SetThreat(GameObject newThreat)
    {
        isAttacked = true;
        threat = newThreat;
        audioSource.GetComponent<AudioManager>().Play("Threat Detected");
        StartCoroutine("ResetThreat");
    }

    IEnumerator ResetThreat()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacked = false;
        threat = null;
    }
}
