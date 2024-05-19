using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public float gameTime = 60f; // Total game time in seconds
    private float startTime; // Time when the game started
    private float remainingTime; // Remaining game time

    public GameObject gameOverPanel;
    public TMP_Text timeText; // Reference to the TextMeshProUGUI component
    public TMP_Text pingText;
    public TMP_Text loserText; // Reference to the TMP_Text to display the loser

    private bool gameIsOver = false;
    private string loserName = "";

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
        if (PhotonNetwork.IsConnected)
        {
            // Get the current ping value from Photon
            int ping = PhotonNetwork.GetPing();
            // Display the ping value on the TextMeshPro object
            pingText.text = ping.ToString() + "ms";
        }

        if (gameIsOver)
        {
            return;
        }

        // Calculate remaining time based on the difference between current time and start time
        remainingTime = gameTime - ((float)PhotonNetwork.Time - startTime);

        // Check for game end condition
        if (remainingTime <= 0f)
        {
            // Call GameOver RPC
            photonView.RPC("GameOver", RpcTarget.AllBuffered);
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

    [PunRPC]
    void GameOver()
    {
        if (gameIsOver)
        {
            return;
        }

        gameIsOver = true;
        StartCoroutine(HandleGameOver());
    }

    IEnumerator HandleGameOver()
    {
        // Disable PlayerController script on all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerController>().enabled = false;
        }

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Find the tagged player and determine the loser
        foreach (GameObject player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc.IsTagged()) // Assuming you have a method to check if the player is tagged
            {
                loserName = player.GetComponent<PhotonView>().Owner.NickName;
                break;
            }
        }

        // Synchronize the loser name across all clients
        photonView.RPC("UpdateLoserText", RpcTarget.AllBuffered, loserName);
    }

    [PunRPC]
    void UpdateLoserText(string loser)
    {
        gameOverPanel.SetActive(true);
        loserName = loser;
        loserText.text = loserName;
    }

    public void LeaveRoomAndGoToLobby()
    {
        // Leave the current room
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // Load the lobby scene after leaving the room
        SceneManager.LoadScene(0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the start time and loser name to all other players
            stream.SendNext(startTime);
            stream.SendNext(loserName);
        }
        else
        {
            // Receive the start time and loser name from the master client
            startTime = (float)stream.ReceiveNext();
            loserName = (string)stream.ReceiveNext();
        }
    }
}
