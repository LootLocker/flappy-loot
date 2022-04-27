using UnityEngine;
using LootLocker.Requests;
using TMPro;
using System.Collections;

public class Leaderboard : MonoBehaviour
{
    public int scoreToUpload;

    public TextMeshProUGUI playerNamesText;
    public TextMeshProUGUI playerScoresText;
    private int leaderboardID = 1562;
    public bool canUploadScore;

    public void SubmitScore()
    {
        if(canUploadScore == false)
        {
            return;
        }
        // Get the players saved ID, and add the incremental characters
        string playerID = PlayerPrefs.GetString("PlayerID") + GetAndIncrementScoreCharacters();
        string metadata = PlayerPrefs.GetString("PlayerName");
        
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardID.ToString(), metadata, (response) =>
        {
            if (response.statusCode == 200)
            {
                Debug.Log("Successful");
                // Only let the player upload score once until we reset it
                canUploadScore = false;
                StartCoroutine(FetchHighscoresCentered());
            }
            else
            {
                Debug.Log("failed: " + response.Error);
            }
        });
        
    }

    // Increment and save a string that goes from a to z, then za to zz, zza to zzz etc.
    string GetAndIncrementScoreCharacters()
    {
        // Get the current score string
        string incrementalScoreString = PlayerPrefs.GetString(nameof(incrementalScoreString), "a");

        // Get the current character
        char incrementalCharacter = PlayerPrefs.GetString(nameof(incrementalCharacter), "a")[0];

        // If the previous character we added was 'z', add one more character to the string.
        // Otherwise, replace last character of the string with the current incrementalCharacter
        if (incrementalScoreString[incrementalScoreString.Length-1] == 'z')
        {
            // Add one more character
            incrementalScoreString += incrementalCharacter;
        }
        else
        {
            // Replace character
            incrementalScoreString = incrementalScoreString.Substring(0, incrementalScoreString.Length - 1) + incrementalCharacter.ToString();
        }

        // If the letter int is lower than 'z' add to it otherwise start from 'a' again
        if((int)incrementalCharacter < 122)
        {
            incrementalCharacter++;
        }
        else
        {
            incrementalCharacter = 'a';
        }

        // Save the current incremental values to PlayerPrefs
        PlayerPrefs.SetString(nameof(incrementalCharacter), incrementalCharacter.ToString());
        PlayerPrefs.SetString(nameof(incrementalScoreString), incrementalScoreString.ToString());

        // Return the updated string
        return incrementalScoreString;
    }
    private void Update()
    {
        
    }

    public IEnumerator FetchTopLeaderboardScores()
    {
        // Let the player know that the scores are loading
        string playerNames = "Loading...";
        string playerScores = "";

        playerScoresText.text = playerScores;
        playerNamesText.text = playerNames;

        // How many scores?
        int count = 10;
        int after = 0;

        bool done = false;
        LootLockerSDKManager.GetScoreListMain(leaderboardID, count, after, (response) =>
        {
            if (response.statusCode == 200)
            {
                Debug.Log("Successful");

                // Set the title of the names tab
                playerNames = "Names\n";
                // Set the title of the scores tab
                playerScores = "Score\n";
                
                LootLockerLeaderboardMember[] members = response.items;
                for (int i = 0; i < members.Length; i++)
                {
                    // Show the ranking, players name and score, and create a new line for the next entry
                    playerNames += members[i].rank + ". " + members[i].metadata+"\n";
                    playerScores += members[i].score + "\n";
                }
                done = true;
            }
            else
            {
                Debug.Log("failed: " + response.Error);
                // Give the user information that the leaderboard couldn't be retrieved
                playerNames = "Error, could not retrieve leaderboard";
                done = true;
            }
        });
        // Wait until the process has finished
        yield return new WaitWhile(() => done == false);
        // Update the TextMeshPro components
        playerNamesText.text = playerNames;
        playerScoresText.text = playerScores;
    }

    IEnumerator FetchHighscoresCentered()
    {
        bool done = false;
        // Let the player know that the scores are loading
        string playerNames = "Loading...";
        string playerScores = "";

        playerScoresText.text = playerScores;
        playerNamesText.text = playerNames;

        // Get the player ID from Player prefs with the incremental score string attached
        string latestPlayerID = PlayerPrefs.GetString("PlayerID") + PlayerPrefs.GetString("incrementalScoreString");
        string[] memberIDs = new string[1] { latestPlayerID };

        // Get the score that matches this ID
        LootLockerSDKManager.GetByListOfMembers(memberIDs, leaderboardID, (response) =>
        {
            if (response.statusCode == 200)
            {
                Debug.Log("Successful");

                // We're only asking for one player, so we just need to check the first entry
                int rank = response.members[0].rank;
                int count = 10;

                // 5 before and 5 after
                int after = rank < 6 ? 0 : rank - 5;

                // Get the entries based on the rank that we just found
                LootLockerSDKManager.GetScoreListMain(leaderboardID, count, after, (response) =>
                {
                    if (response.statusCode == 200)
                    {
                        // Set the title of the names tab
                        playerNames = "Names\n";
                        // Set the title of the scores tab
                        playerScores = "Score\n";

                        Debug.Log("Successful");

                        LootLockerLeaderboardMember[] members = response.items;
                        for (int i = 0; i < members.Length; i++)
                        {
                            // Highlight the new score with yellow, add the rest as normal
                            if (members[i].rank == rank)
                            {
                                playerNames += "<color=#f4e063ff>"+members[i].rank + ". " + members[i].metadata + "</color>" + "\n";
                                playerScores += "<color=#f4e063ff>" + members[i].score + "</color>" + "\n";
                            }
                            else
                            {
                                playerNames += members[i].rank + ". " + members[i].metadata + "\n";
                                playerScores += members[i].score + "\n";
                            }
                        }
                        done = true;
                    }
                    else
                    {
                        Debug.Log("failed: " + response.Error);
                        done = true;
                    }
                });
            }
            else
            {
                Debug.Log("failed: " + response.Error);
                done = true;
            }
        });

        // Wait until request is done
        yield return new WaitWhile(() => done == false);

        // Update the TextMeshPro components
        playerNamesText.text = playerNames;
        playerScoresText.text = playerScores;
    }
}
