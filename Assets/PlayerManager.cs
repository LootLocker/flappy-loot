using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public bool loggedIn;

    public string incrementalPlayerID;

    public int incrementalIDInt;

    // Reference to the input field for changing the players name
    public TMP_InputField playerInputField;

    public IEnumerator LoginRoutine()
    {
        incrementalIDInt = PlayerPrefs.GetInt(nameof(incrementalIDInt));
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully started LootLocker session");
                loggedIn = true;
                done = true;
                // Save the player ID for use in the leaderboard
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                // If the players name hasn't been set, set it to #GuestXXXXXXX (XXXXX = player ID)
                PlayerPrefs.SetString("PlayerName", PlayerPrefs.GetString("PlayerName", "#Guest" + PlayerPrefs.GetString("PlayerID")));
            }
            else
            {
                Debug.Log("Error starting LootLocker session");
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);

        // Update the name 
        UpdatePlayerInputField();
    }

    public void UpdatePlayerName()
    {
        PlayerPrefs.SetString("PlayerName", playerInputField.text);
    }

    public void UpdatePlayerInputField()
    {
        // Set the text to the player name
        playerInputField.text = PlayerPrefs.GetString("PlayerName");

        // Set the placeholder name to the player name as well
        TextMeshProUGUI placeHolderText = playerInputField.placeholder as TextMeshProUGUI;
        placeHolderText.text = PlayerPrefs.GetString("PlayerName");
    }
}
