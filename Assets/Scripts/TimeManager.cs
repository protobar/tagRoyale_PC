using UnityEngine;
using TMPro;
using Photon.Pun;

public class TimeManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public float gameTime = 60f; // Total game time in seconds
    private float startTime; // Time when the game started
    private float remainingTime; // Remaining game time

    public TMP_Text timeText; // Reference to the TextMeshProUGUI component

    void Start()
    {
        // Initialize remaining time on all clients
        remainingTime = gameTime;

        if (PhotonNetwork.IsMasterClient)
        {
            // Start the countdown timer on the master client
            startTime = (float)PhotonNetwork.Time;
        }
    }

    void Update()
    {
        // Calculate remaining time based on the difference between current time and start time
        remainingTime = gameTime - ((float)PhotonNetwork.Time - startTime);

        // Check for game end condition
        if (remainingTime <= 0f)
        {
            // Game over logic (e.g., determine winner, end game)
            // Stop the countdown timer
            return;
        }

        // Update the TMP_Text element with the remaining time
        UpdateTimeText();
    }

    void UpdateTimeText()
    {
        // Format remaining time as minutes and seconds
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        // Update the TMP_Text element
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the start time to all other players
            stream.SendNext(startTime);
        }
        else
        {
            // Receive the start time from the master client
            startTime = (float)stream.ReceiveNext();
        }
    }
}
