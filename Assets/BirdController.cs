using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    private Rigidbody2D rb;
    private float jumpAmount = 4;
    private float rotationMultiplier = 10f;
    private float rotationLerpSpeed = 20f;
    public Transform wing;
    public AnimationCurve flapCurve;
    private float flapDuration = 0.2f;
    // Our flapping sound
    public AudioClip flapSound;
    // Sound to play when dying
    public AudioClip deathSound;
    // Audiosource that will play our clips
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Control the bird automatically on the menu and during the countdown
        if (GameController.instance.currentGameState == GameController.GameState.Menu || 
            GameController.instance.currentGameState == GameController.GameState.Countdown ||
            GameController.instance.currentGameState == GameController.GameState.Loading)
        {
            if (transform.position.y <= 0f)
            {
                Flap();
            }
        }
        // Let the player control the bird when playing
        if (GameController.instance.currentGameState == GameController.GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Flap();
            }
        }
    }

    void FixedUpdate()
    {
        if(GameController.instance.currentGameState == GameController.GameState.Menu ||
            GameController.instance.currentGameState == GameController.GameState.Countdown ||
            GameController.instance.currentGameState == GameController.GameState.Playing || 
            GameController.instance.currentGameState == GameController.GameState.Loading)
        {
            RotateWithVelocity();
        }
        if (GameController.instance.currentGameState == GameController.GameState.Playing)
        {
            // Clamp the birds position in the top part of the screen
            if (rb.position.y > 3f)
            {
                rb.MovePosition(new Vector2(rb.position.x, 3f));
                rb.velocity = Vector2.zero;
            }
            if (rb.position.y <= -3f)
            {
                GameController.instance.SwitchGameState(GameController.GameState.Dying);
            }
        }
    }

    void RotateWithVelocity()
    {
        rb.SetRotation(Mathf.Lerp(rb.rotation, rb.velocity.y * rotationMultiplier, Time.deltaTime * rotationLerpSpeed));
    }

    void Flap()
    {
        // Stop the audio if it is already playing
        if(audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(flapSound);
        StartCoroutine(FlapWingRoutine());
        rb.velocity = new Vector2(0, jumpAmount);
    }

    public void ResetBird()
    {
        // Reset the different velocities and rotation
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.SetRotation(0f);
        // Reset the position
        rb.transform.position = (new Vector2(0, 0));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameController.instance.currentGameState == GameController.GameState.Playing)
        {
            GameController.instance.SwitchGameState(GameController.GameState.Dying);
            StartCoroutine(DieRoutine());
        }
    }

    IEnumerator FlapWingRoutine()
    {
        // Our timer
        float timer = 0f;
        
        // Variables to control the rotations
        float flappingAngle = 45f;
        Vector3 wingRotation = wing.localEulerAngles;
        float startAngle = 0;

        // We want to lerp the angle of our birds wing, but the built in lerpAngle does not take in overshoot
        // and in this particular case we want the angle to be able to overshoot to get a bit of punch in the animation
        // This local function interpolate correctly when wrapped around 360 degrees but allows for overshoot
        static float LerpAngleUnclamped(float a, float b, float t)
        {
            float delta =  Mathf.Repeat((b - a), 360);
            if (delta > 180)
                delta -= 360;
            return a + delta * t;
        }

        // Run for the duration
        while (timer <= flapDuration)
        {
            // Lerp with the animation curves output as a value
            wingRotation.z = LerpAngleUnclamped(startAngle, flappingAngle, flapCurve.Evaluate(timer / flapDuration));
            wing.localEulerAngles = wingRotation;
            timer += Time.deltaTime;
            yield return null;
        }
        // In case we overshoot, set the angle back to the starting value
        wingRotation.z = startAngle;
        wing.localEulerAngles = wingRotation;
    }

    IEnumerator DieRoutine()
    {
        // Freeze time
        Time.timeScale = 0f;
        // Wait for 0.1 seconds
        yield return new WaitForSecondsRealtime(0.1f);

        // Play death sound effect with the original pitch
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(deathSound);

        // Unfreeze time
        Time.timeScale = 1f;

        // Launch the bird upwards and to the left
        rb.AddForce(new Vector3(-1, 1, 0) * 5f, ForceMode2D.Impulse);

        // Give the bird some rotation force as well
        rb.AddTorque(2f, ForceMode2D.Impulse);
    }
}
