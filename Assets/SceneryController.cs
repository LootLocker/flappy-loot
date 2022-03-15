using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryController : MonoBehaviour
{
    // Reference to the grass prefab
    public GameObject grassPrefab;

    // A list with all grass
    private List<Transform> grassTransforms = new List<Transform>();

    // How fast does the grass move on screen
    public float grassSpeed = -2f;

    // The X value where the objects will start and reset
    public float maxX = 8f;

    // How many grass objects
    public int grassAmount = 16;

    // Vector to store movement of the grass
    Vector3 movementVector;

    // Vector to store movement of the grass
    Vector3 movementVectorClouds;

    // How many grass objects
    public int cloudAmount = 7;

    // How fast does the grass move on screen
    public float cloudSpeed = -0.5f;

    // Reference to the cloud prefab
    public GameObject cloudPrefab;

    // A list with all clouds
    private List<Transform> cloudTransforms = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        // Set up the movement vectors
        movementVector.x = grassSpeed;
        movementVectorClouds.x = cloudSpeed;

        // Create the grass and clouds
        CreateGrass();
        CreateClouds();
    }
    void CreateGrass()
    {
        // Spawn position
        Vector3 spawnPos = new Vector3(-maxX, -2.75f, 0);

        for (int i = 0; i < grassAmount; i++)
        {
            GameObject newGrass = Instantiate(grassPrefab, spawnPos, Quaternion.identity);
            // Create a new grass object and add it to the grass-list
            grassTransforms.Add(newGrass.transform);

            // Increase the spawn position
            spawnPos.x += 1f;
        }
    }

    void CreateClouds()
    {
        // Spawn position with random y value
        Vector3 spawnPos = new Vector3(-maxX, Random.Range(1.5f, 3f), 0);
        Vector3 randomScale = Vector3.one * Random.Range(1f, 2f);

        for (int i = 0; i < grassAmount; i++)
        {
            // Create a new grass object and add it to the grass-list
            GameObject newGrass = Instantiate(cloudPrefab, spawnPos, Quaternion.identity);
            newGrass.transform.localScale = randomScale;
            cloudTransforms.Add(newGrass.transform);

            // Increase the spawn position
            spawnPos.x += Random.Range(1f, 3f);
            // New random y value
            spawnPos.y = Random.Range(1.5f, 3f);
            // New random scale
            randomScale = Vector3.one * Random.Range(1f, 2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move the grass unless the bird is dead
        if (GameController.instance.currentGameState != GameController.GameState.Dead && GameController.instance.currentGameState != GameController.GameState.Dying)
        {
            MoveGrass();
            MoveClouds();
        }
    }
    void MoveGrass()
    {
        for (int i = 0; i < grassTransforms.Count; i++)
        {
            // If the grass is outside of the screen, reset the position
            if (grassTransforms[i].localPosition.x <= -maxX)
            {
                grassTransforms[i].localPosition += new Vector3(maxX*2, 0, 0);
            }
            // Move the grass
            grassTransforms[i].localPosition += movementVector * Time.deltaTime;
        }
    }

    void MoveClouds()
    {
        for (int i = 0; i < cloudTransforms.Count; i++)
        {
            // If the grass is outside of the screen, reset the position
            if (cloudTransforms[i].localPosition.x <= -maxX)
            {
                // Reset the position and add some random values to it
                cloudTransforms[i].localPosition += new Vector3(maxX * 2 + 1.5f + Random.Range(-1.5f, 1.5f), 0, 0);

                // Set the y position separetely, since we do not want to add it, we want to change it directly
                cloudTransforms[i].localPosition = new Vector3(cloudTransforms[i].localPosition.x, Random.Range(1.5f, 3f), cloudTransforms[i].localPosition.z);

                // New random scale
                cloudTransforms[i].localScale = Vector3.one * Random.Range(1f, 2f);
            }
            // Move the clouds faster if they are bigger
            cloudTransforms[i].localPosition += movementVectorClouds * Time.deltaTime * cloudTransforms[i].localScale.x;
        }
    }
}
