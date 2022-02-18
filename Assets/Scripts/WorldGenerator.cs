using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WorldGenerator : MonoBehaviourPun
{
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public Transform[] spawnPoints;

    [SerializeField] public GameObject dummy;
    [SerializeField] public GameObject colorPotion;
    [SerializeField] public GameObject chicken;
    [SerializeField] public GameObject[] boundingBoxes;

    [Header("[Settings]")]
    public int numDummies;
    public float maxTimeToPotionSpawn;
    public float potionSpawnChance;
    public float maxTimeToFoodSpawn;
    public float foodSpawnChance;

    private float[] leftBoundaries;
    private float[] rightBoundaries;
    private float[] upperBoundaries;
    private float[] lowerBoundaries;
    private float timeToPotionSpawn;
    private float timeToFoodSpawn;
    private float padding = 2f;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayer();

        if (!PhotonNetwork.IsMasterClient) { return; }
   
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

        SpawnDummies(numDummies);
    }

    private void SpawnPlayer()
    {
        int randomNumber = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomNumber];
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(-3f, -0f, 0f), Quaternion.identity);
    }
    

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        timeToPotionSpawn -= Time.deltaTime;
        if (timeToPotionSpawn <= 0)
        {
            if (Random.Range(0f, 1f) < potionSpawnChance)
            {
                // Choose which room
                int boxIndex = Random.Range(0, boundingBoxes.Length);

                // Spawn within bounding box
                float x = Random.Range(leftBoundaries[boxIndex], rightBoundaries[boxIndex]);
                float y = Random.Range(upperBoundaries[boxIndex], lowerBoundaries[boxIndex]);

                // Assign random color to potion
                float randR = Random.Range(0f, 1f);
                float randG = Random.Range(0f, 1f);
                float randB = Random.Range(0f, 1f);
                
                GameObject spawnedPotion = PhotonNetwork.InstantiateRoomObject(colorPotion.name, new Vector3(x, y, 0), Quaternion.identity);
                PhotonView spawnedPotionPv = spawnedPotion.GetComponent<PhotonView>();
                photonView.RPC("SendGameObjectColorRpc", RpcTarget.All, spawnedPotionPv.ViewID, randR, randG, randB);
            }
            timeToPotionSpawn = maxTimeToPotionSpawn;
        }

        timeToFoodSpawn -= Time.deltaTime;
        if (timeToFoodSpawn <= 0)
        {
            if (Random.Range(0f, 1f) < foodSpawnChance)
            {
                // Choose which room
                int boxIndex = Random.Range(0, boundingBoxes.Length);

                // Spawn within bounding box
                float x = Random.Range(leftBoundaries[boxIndex], rightBoundaries[boxIndex]);
                float y = Random.Range(upperBoundaries[boxIndex], lowerBoundaries[boxIndex]);

                GameObject spawnedFood = PhotonNetwork.InstantiateRoomObject(chicken.name, new Vector3(x, y, 0), Quaternion.identity);
            }
            timeToFoodSpawn = maxTimeToFoodSpawn;
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
            //GameObject spawnedDummy = PhotonNetwork.Instantiate(dummy.name, new Vector3(x, y, 0), Quaternion.identity);

            // Turn spawned dummy to left or right randomly
            int randomDirection = Random.Range(0, 2) * 2 - 1;
            //spawnedDummy.transform.localScale = new Vector3(randomDirection, spawnedDummy.transform.localScale.y, spawnedDummy.transform.localScale.z);

            // Assign random colors to dummies
            float randR = Random.Range(0f, 1f);
            float randG = Random.Range(0f, 1f);
            float randB = Random.Range(0f, 1f);

            GameObject spawnedDummy = PhotonNetwork.InstantiateRoomObject(dummy.name, new Vector3(x, y, 0), Quaternion.identity);
            PhotonView spawnedDummyPv = spawnedDummy.GetComponent<PhotonView>();
            photonView.RPC("SendGameObjectColorRpc", RpcTarget.All, spawnedDummyPv.ViewID, randR, randG, randB);
            photonView.RPC("SendGameObjectDirectionRpc", RpcTarget.All, spawnedDummyPv.ViewID, randomDirection);
        }
    }

    [PunRPC]
    private void SendGameObjectColorRpc(int Id, float r, float g, float b)
    {
        PhotonView view = PhotonView.Find(Id);
        Color color = new Color(r, g, b, 1f);
        Utils.SetChildrenColor(view.gameObject, color);
    }

    [PunRPC]
    private void SendGameObjectDirectionRpc(int Id, int direction)
    {
        PhotonView view = PhotonView.Find(Id);
        view.gameObject.transform.localScale = new Vector3(direction, 1, 1);
    }
}
