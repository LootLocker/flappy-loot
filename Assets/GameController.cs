using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public enum SwooshDirection { Top, Down, Right, Left };
    // Our instance of the game controller, for easy access
    public static GameController instance;
    // Our different states
    public enum GameState { Loading, Menu, Countdown, Playing, Dying, Dead };

    // Current state of the game
    public GameState currentGameState = GameState.Menu;

    // The text that will show info to our player
    public TextMeshProUGUI infoText;

    // Variables for the countdown timer
    private float countDownTimer;
    private int countdownInt;

    // Pipe controller
    public PipeController pipeController;
    // Bird controller
    public BirdController birdController;

    // The player score
    private int score;

    // The text that will show the score to our player
    public TextMeshProUGUI scoreText;

    // Reference to our playerManager
    public PlayerManager playerManager;

    // Our leaderboard
    public Leaderboard leaderboard;

    // Button that will submit the score
    public GameObject submitScoreButton;

    // Reference to our leaderboard game object
    public GameObject leaderboardObject;

    // Reference to the player name input field game object
    public GameObject playerNameInputFieldObject;

    // Reference to the button for going back to the menu
    public GameObject backToMenuButtonObject;

    // Reference to the title text object
    public GameObject titleTextObject;

    public int Score
    {
        get { return score; }
        set
        {
            // Update the text when we update the score
            scoreText.text = value.ToString();
            score = value;
            StartCoroutine(WiggleScaleTextRoutine(scoreText.rectTransform));
        }
    }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // We will only have one gamecontroller and one scene, so no need to check if the instance already exists
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetupRoutine());
    }

    // Clear the text after a set amount of time
    IEnumerator ClearInfoTextAfterTimeoutRoutine(float timeToClear)
    {
        yield return new WaitForSeconds(timeToClear);
        infoText.text = "";
    }

    // Set the text for the main menu
    void SetMainMenuText()
    {
        infoText.text = "Press spacebar to play";
    }

    // Update is called once per frame
    void Update()
    {
        // Depending on what gamestate we're in, do different things
        switch (currentGameState)
        {
            case GameState.Menu:
                // If we are in the menu state, pressing spacebar will take us to the game state
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SwitchGameState(GameState.Countdown);
                }
                break;
            case GameState.Countdown:
                // Decrease the countdowntimer
                countDownTimer -= Time.deltaTime;

                // If the timer is a new int, we update the text
                if (Mathf.CeilToInt(countDownTimer) != countdownInt)
                {
                    // Update the int
                    countdownInt = Mathf.CeilToInt(countDownTimer);
                    if (countdownInt == 0)
                    {
                        // If we are on 0, we switch to let the player control the bird
                        infoText.text = "GO!\nPRESS SPACEBAR";

                        // Clear the text after 0.5 seconds
                        StartCoroutine(ClearInfoTextAfterTimeoutRoutine(0.5f));
                        // Switch to gamestate
                        SwitchGameState(GameState.Playing);
                    }
                    else
                    {
                        // Update the text for the countdown numbers
                        infoText.text = countdownInt.ToString();
                    }
                }
                break;
            case GameState.Playing:
                break;
            case GameState.Dead:
                break;
        }
    }

    // What we should do when switching between the different game states
    public void SwitchGameState(GameState newGamestate)
    {
        switch (newGamestate)
        {
            case GameState.Menu:
                // Reset the text when we go back to the menu
                pipeController.ResetPipes();
                birdController.ResetBird();
                // Reset the score and scoreText
                score = 0;
                scoreText.text = "";
                SetMainMenuText();

                // Swoosh off the playername input field
                StartCoroutine(SwooshObjectRoutine(playerNameInputFieldObject.GetComponent<RectTransform>(), true));

                // Show the leaderboard and fetch the updated scores
                StartCoroutine(SwooshObjectRoutine(leaderboardObject.GetComponent<RectTransform>(), false));
                StartCoroutine(leaderboard.FetchTopLeaderboardScores());

                // Turn off score button
                StartCoroutine(SwooshObjectRoutine(submitScoreButton.GetComponent<RectTransform>(), true));

                // Hide the go back to menu button
                StartCoroutine(SwooshObjectRoutine(backToMenuButtonObject.GetComponent<RectTransform>(), true));

                // Show title text
                StartCoroutine(SwooshObjectRoutine(titleTextObject.GetComponent<RectTransform>(), false));
                break;
            case GameState.Countdown:
                // Hide the leaderboard
                StartCoroutine(SwooshObjectRoutine(leaderboardObject.GetComponent<RectTransform>(), true));
                // We set it to 3.5f, so the text "GET READY" will show for 0.5 seconds before we show the countdown numbers
                countDownTimer = 3.5f;
                countdownInt = Mathf.CeilToInt(countDownTimer);
                infoText.text = "GET READY";
                // Hide the title text
                StartCoroutine(SwooshObjectRoutine(titleTextObject.GetComponent<RectTransform>(), true));
                break;
            case GameState.Playing:
                Score = 0;
                break;
            case GameState.Dying:

                StartCoroutine(DeathRoutine());

                // Fetch the leaderboards as soon as the player has died, so that they're updated when going to the dead state
                StartCoroutine(leaderboard.FetchTopLeaderboardScores());
                break;
            case GameState.Dead:
                // Turn on score button
                StartCoroutine(SwooshObjectRoutine(submitScoreButton.GetComponent<RectTransform>(), false));
                // Show the leaderboard
                StartCoroutine(SwooshObjectRoutine(leaderboardObject.GetComponent<RectTransform>(), false));
                // Show the playername input field
                StartCoroutine(SwooshObjectRoutine(playerNameInputFieldObject.GetComponent<RectTransform>(), false));
                // Show the go back to menu button
                StartCoroutine(SwooshObjectRoutine(backToMenuButtonObject.GetComponent<RectTransform>(), false));
                break;
        }
        currentGameState = newGamestate;
    }

    IEnumerator DeathRoutine()
    {
        // Show game over text
        infoText.text = "GAME OVER";

        // Save our score to the leaderboard
        leaderboard.scoreToUpload = score;

        // Let the player upload their score
        leaderboard.canUploadScore = true;

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Switch to Dead state, so that the player can restart the game
        SwitchGameState(GameState.Dead);
        infoText.text = "";
    }

    IEnumerator SetupRoutine()
    {
        // Set the info text to loading
        infoText.text = "Logging in...";
        // Wait while the login is happening
        yield return playerManager.LoginRoutine();

        // If the player couldn't log in, let them know, and then retry
        if (playerManager.loggedIn == false)
        {
            float loginCountdown = 4;
            float timer = loginCountdown;
            while (timer >= -1f)
            {
                timer -= Time.deltaTime;
                // Update the text when we get to a new number
                if (Mathf.CeilToInt(timer) != Mathf.CeilToInt(loginCountdown))
                {
                    infoText.text = "Failed to login retrying in " + Mathf.CeilToInt(timer).ToString();
                    loginCountdown -= 1f;
                }
                yield return null;
            }
            StartCoroutine(SetupRoutine());
            yield break;
        }

        // Get the top players in our leaderboard
        yield return leaderboard.FetchTopLeaderboardScores();

        // Set up initial value for the text
        SetMainMenuText();
        SwitchGameState(GameState.Menu);

        yield return null;
    }



    IEnumerator SwooshObjectRoutine(RectTransform objectToSwoosh, bool hide)
    {
        // Do not hide if the object is already hidden
        if (!objectToSwoosh.gameObject.activeInHierarchy && hide)
        {
            yield break;
        }
        // Do not show if the object is already activated
        if (objectToSwoosh.gameObject.activeInHierarchy && !hide)
        {
            yield break;
        }
        // Get the swoosh direction from the RectTransform
        Vector3 startposition = objectToSwoosh.anchoredPosition;
        Vector3 endPosition = startposition;

        // This position is saved separetely.
        // We want to set this position when the gameObject has been deactivated, so that it will start from the correct position next time
        Vector3 originalStartPosition = startposition;

        // UI values are much larger than regular values, so we multiply with 200 to make sure that they are positioned outside of the screen
        if (objectToSwoosh.anchorMax == new Vector2(1f, 0.5f))
        {
            // Anchored to the right side
            startposition += Vector3.right * 200f;
        }
        else if (objectToSwoosh.anchorMax == new Vector2(0f, 00.5f))
        {
            // Anchored to the left side
            startposition += Vector3.left * 200f;
        }
        else if (objectToSwoosh.anchorMax == new Vector2(0.5f, 1))
        {
            // Anchored to the top
            startposition += Vector3.up * 200f;
        }
        else if (objectToSwoosh.anchorMax == new Vector2(0.5f, 0))
        {
            // Anchored to the bottom
            startposition += Vector3.down * 200f;
        }
        else
        {
            // Anchors are not set up, insert it from the top
            startposition += Vector3.up * 200f;
        }
        // Timer
        float timer = 0f;

        // How long to swoosh
        float duration = 0.5f;

        // The value to be used for our lerp
        float value = 0f;

        // If we're hiding instead of showing, switch around
        // the start and endvalues
        if (hide)
        {
            Vector3 temp = endPosition;
            endPosition = startposition;
            startposition = temp;
        }

        // Set the position and turn on the gameObject
        objectToSwoosh.anchoredPosition = startposition;
        objectToSwoosh.gameObject.SetActive(true);

        // Run for duration
        while (timer <= duration)
        {
            // Create a value with smoothed start and smoothed end
            value = timer / duration;

            // Ease in with cos, ease out with sin to get a smooth curve
            if (hide == false)
            {
                value = Mathf.Sin(value * Mathf.PI * 0.5f);
            }
            else
            {
                value = 1f - Mathf.Cos(value * Mathf.PI * 0.5f);
            }

            objectToSwoosh.anchoredPosition = Vector3.Lerp(startposition, endPosition, value);
            timer += Time.deltaTime;
            yield return null;
        }

        // In case we overshoot, set the position
        objectToSwoosh.anchoredPosition = endPosition;

        // Turn off the game object if we're hiding it
        if (hide)
        {
            objectToSwoosh.gameObject.SetActive(false);
            // Set back to the original position
            objectToSwoosh.anchoredPosition = originalStartPosition;
        }
        yield return null;
    }

    public void GoBackToMenu()
    {
        SwitchGameState(GameState.Menu);
    }

    IEnumerator WiggleScaleTextRoutine(Transform transformToWiggle)
    {
        // Timer
        float timer = 0f;
        // Duration of the wiggle
        float duration = 0.5f;
        // The value we'll use to store our calculation
        float value = 0f;
        // How fast the wiggling should be
        float speed = 20f;
        // How much to increase the scale
        float scaleSize = 0.5f;

        // The original scale
        Vector3 originalScale = Vector3.one;
        while (timer <= duration)
        {
            // Wiggle for the duration using sin
            value = Mathf.Lerp(Mathf.Cos(timer * speed) * scaleSize, 0, timer / duration);

            // Add the wiggle Vector to the localScale
            transformToWiggle.localScale = Vector3.one + (Vector3.one * value);

            timer += Time.deltaTime;
            yield return null;
        }
        // In case we overshoot, reset the scale
        transformToWiggle.localScale = originalScale;
    }

}