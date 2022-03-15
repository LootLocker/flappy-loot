using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Class for our pipes
public class Pipe
{
    public Pipe(Transform upper, Transform lower)
    {
        upperPipe = upper;
        upperPipe.localEulerAngles = new Vector3(0, 0, 180f);
        lowerPipe = lower;
    }
    // Control to see if the pipe has given score or not
    public bool canGiveScore = true;
    // Upper pipe
    public Transform upperPipe;
    // Lower pipe
    public Transform lowerPipe;

    public void Move(Vector3 movement)
    {
        // Increase position with the movement vector
        upperPipe.position += movement;
        lowerPipe.position += movement;
    }

    // Reset position and set a new random y-value
    public void Reset(Vector3 resetPosition)
    {
        canGiveScore = true;
        float randomY = Random.Range(-PipeController.randomYMax, PipeController.randomYMax);
        resetPosition.y = PipeController.verticalDistanceBetweenPipes + randomY;
        upperPipe.position = resetPosition;
        resetPosition.y = -PipeController.verticalDistanceBetweenPipes + randomY;
        lowerPipe.position = resetPosition;
    }

    public float X
    {
        // upperPipe and lowerPipe are both on the same x position, so we can return just one of the positions
        get { return upperPipe.position.x; }
    }
}
[RequireComponent(typeof(AudioSource))]
public class PipeController : MonoBehaviour
{
    // Reference to the pipe prefab
    public GameObject pipePrefab;
    // A list with all pipes
    private List<Pipe> pipes = new List<Pipe>();

    // How fast does the pipes move on screen
    public float pipeSpeed = -4f;

    // The X value where the pipes will start and reset
    public float maxX = 8f;

    // Horizontal distance between the pipes
    public float horizontalDistanceBetweenPipes = 4f;
    // Vertical distance between the pipes
    public static float verticalDistanceBetweenPipes = 3f;

    // How many pipes
    public int pipeAmount = 3;

    // How much the random value can be for the Y-position of the pipes
    public static float randomYMax = 1f;

    // Vector to store movement of the pipes
    Vector3 movementVector;

    // AudioSource to play score audio
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // Set up the movement vector
        movementVector.x = pipeSpeed;

        // Create all the pipes
        CreatePipes();

        // Get the audio source
        audioSource = GetComponent<AudioSource>();
    }
    void CreatePipes()
    {
        // Spawn position
        Vector3 pipeSpawnPos = new Vector3(maxX, 0, 0);
        // Random Y-position
        float randomYPosition = Random.Range(-1.5f, 1.5f);

        for (int i = 0; i < pipeAmount; i++)
        {
            // Create upper pipe
            pipeSpawnPos.y = 3f + randomYPosition;
            GameObject upperPipe = Instantiate(pipePrefab, pipeSpawnPos, Quaternion.identity);
            // Create lower pipe
            pipeSpawnPos.y = -3f + randomYPosition;
            GameObject lowerPipe = Instantiate(pipePrefab, pipeSpawnPos, Quaternion.identity);

            // Create a new Pipe object and add it to the pipes-list
            pipes.Add(new Pipe(upperPipe.transform, lowerPipe.transform));

            // Increase the spawn position and make a new random y-position for the next pipe in the loop
            pipeSpawnPos.x += horizontalDistanceBetweenPipes;
            randomYPosition = Random.Range(-1f, 2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only move the pipes if we are in play mode
        if (GameController.instance.currentGameState == GameController.GameState.Playing)
        {
            MovePipes();
        }
    }
    void MovePipes()
    {
        for (int i = 0; i < pipes.Count; i++)
        {
            // Move all pipes
            pipes[i].Move( movementVector * Time.deltaTime);
            // If the pipe is outside of the screen, reset its position
            if (pipes[i].X <= -maxX)
            {
                pipes[i].Reset(new Vector3(maxX, 0, 0));
            }
            if(pipes[i].canGiveScore == true && pipes[i].X <= 0)
            {
                // Give the player one score point
                // and make sure that this pipe can not
                // give a new score until it has been reset
                pipes[i].canGiveScore = false;
                GameController.instance.Score++;
                audioSource.Play();
            }
        }
    }

    public void ResetPipes()
    {
        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].Reset(new Vector3(maxX+(i * horizontalDistanceBetweenPipes), 0, 0));
        }
    }
}
