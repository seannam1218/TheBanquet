using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public Transform[] spawnPoints;

    [SerializeField] public GameObject dummy;
    [SerializeField] public GameObject colorPotion;
    [SerializeField] public GameObject[] boundingBoxes;

    [Header("[Settings]")]
    [SerializeField] public int numDummies;
    public float maxTimeToPotionSpawn;
    public float potionSpawnChance;

    private float[] leftBoundaries;
    private float[] rightBoundaries;
    private float[] upperBoundaries;
    private float[] lowerBoundaries;
    private float timeToPotionSpawn;


    // Start is called before the first frame update
    void Start()
    {
        // Determine the boundaries of each bounding box.
        leftBoundaries = new float[boundingBoxes.Length];
        rightBoundaries = new float[boundingBoxes.Length];
        upperBoundaries = new float[boundingBoxes.Length];
        lowerBoundaries = new float[boundingBoxes.Length];

        for (int i = 0; i < boundingBoxes.Length; i++)
        {
            leftBoundaries[i] = boundingBoxes[i].transform.position.x - boundingBoxes[i].transform.localScale.x / 2;
            rightBoundaries[i] = boundingBoxes[i].transform.position.x + boundingBoxes[i].transform.localScale.x / 2;
            upperBoundaries[i] = boundingBoxes[i].transform.position.y + boundingBoxes[i].transform.localScale.y / 2;
            lowerBoundaries[i] = boundingBoxes[i].transform.position.y - boundingBoxes[i].transform.localScale.y / 2;
        }

        SpawnPlayer();
        SpawnDummies(numDummies);
    }

    private void SpawnPlayer()
    {
        int randomNumber = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomNumber];
        Debug.Log(spawnPoint.position);
        Debug.Log(Quaternion.identity);
        Debug.Log(playerPrefab.name);
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(-3f, -0f, 0f), Quaternion.identity);
    }
    

    private void Update()
    {
        timeToPotionSpawn -= Time.deltaTime;
        if (timeToPotionSpawn <= 0)
        {
            if (Random.Range(0f, 1f) < potionSpawnChance)
            {
                // Choose which room
                int boxIndex = Random.Range(0, boundingBoxes.Length);

                // Spawn dummy within bounding box
                float x = Random.Range(leftBoundaries[boxIndex], rightBoundaries[boxIndex]);
                float y = Random.Range(upperBoundaries[boxIndex], lowerBoundaries[boxIndex]);
                GameObject spawnedPotion = PhotonNetwork.Instantiate(colorPotion.name, new Vector3(x, y, 0), Quaternion.identity);

                // Assign random color to potion
                Color randColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
                Utils.SetChildrenColor(spawnedPotion, randColor);
            }
            timeToPotionSpawn = maxTimeToPotionSpawn;
        }
    }


    void SpawnDummies(int num)
    {
        for (int i =0; i<num; i++)
        {
            // Choose which room
            int boxIndex = Random.Range(0, boundingBoxes.Length);

            // Spawn dummy within bounding box
            float x = Random.Range(leftBoundaries[boxIndex], rightBoundaries[boxIndex]);
            float y = Random.Range(upperBoundaries[boxIndex], lowerBoundaries[boxIndex]);
            GameObject spawnedDummy = PhotonNetwork.Instantiate(dummy.name, new Vector3(x, y, 0), Quaternion.identity);

            // Turn spawned dummy to left or right randomly
            int randomDirection = Random.Range(0, 2) * 2 - 1;
            spawnedDummy.transform.localScale = new Vector3(randomDirection, spawnedDummy.transform.localScale.y, spawnedDummy.transform.localScale.z);

            // Assign random colors to dummies
            Color randColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            Utils.SetChildrenColor(spawnedDummy, randColor);
        }
    }
}
