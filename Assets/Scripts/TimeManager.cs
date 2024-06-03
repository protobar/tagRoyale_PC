using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public float gameTime = 60f; // Total game time in seconds
    private float startTime; // Time when the game started
    private float remainingTime; // Remaining game time

    public GameObject gameOverPanel;
    public TMP_Text timeText;
    public TMP_Text pingText;
    public TMP_Text loserText;
    public TMP_Text roundText;
    public TMP_Text countdownText;

    private bool gameIsOver = false;
    private string loserName = "";
    private int roundNumber = 1;
    private int maxRounds = 3;

    void Start()
    {
        remainingTime = gameTime;
        startTime = (float)PhotonNetwork.Time;
        photonView.RPC("SetStartTime", RpcTarget.AllBuffered, startTime);
        UpdateRoundText();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            int ping = PhotonNetwork.GetPing();
            pingText.text = ping.ToString() + "ms";
        }

        if (gameIsOver)
        {
            return;
        }

        remainingTime = gameTime - ((float)PhotonNetwork.Time - startTime);

        if (remainingTime <= 0f)
        {
            Debug.Log("Time is up " + remainingTime);
            photonView.RPC("GameOver", RpcTarget.AllBuffered);
            return;
        }

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
        Debug.Log("Gameover function called");
        if (gameIsOver)
        {
            return;
        }

        gameIsOver = true;
        StartCoroutine(HandleGameOver());
    }

    IEnumerator HandleGameOver()
    {
        Debug.Log("Coroutine Started");

        // Disable PlayerController script on all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerController>().enabled = false;
        }

        yield return new WaitForSeconds(2f);

        // Find the tagged player and determine the loser
        foreach (GameObject player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc.IsTagged())
            {
                loserName = player.GetComponent<PhotonView>().Owner.NickName;
                break;
            }
        }

        photonView.RPC("UpdateLoserText", RpcTarget.AllBuffered, loserName);

        // Display countdown for next level if there are remaining rounds
        if (roundNumber < maxRounds)
        {
            yield return StartCoroutine(CountdownToNextRound());
        }
        else
        {
            photonView.RPC("DisplayFinalGameOverText", RpcTarget.AllBuffered, loserName);
        }
    }

    IEnumerator CountdownToNextRound()
    {
        gameOverPanel.SetActive(true);
        for (int i = 5; i > 0; i--)
        {
            countdownText.text = "Next round starts in: " + i;
            yield return new WaitForSeconds(1f);
        }
        roundNumber++;
        UpdateRoundText();
        ResetRound();
        gameIsOver = false;
    }

    [PunRPC]
    void UpdateLoserText(string loser)
    {
        gameOverPanel.SetActive(true);
        loserName = loser;
        loserText.text = "Loser: " + loser;
    }

    [PunRPC]
    void DisplayFinalGameOverText(string loser)
    {
        gameOverPanel.SetActive(true);
        loserName = loser;
        loserText.text = "Loser: " + loser;
        roundText.text = "Game Over";
        countdownText.text = "";
    }

    [PunRPC]
    void SetStartTime(float masterStartTime)
    {
        startTime = masterStartTime;
    }

    void UpdateRoundText()
    {
        roundText.text = "Round: " + roundNumber;
    }

    void ResetRound()
    {
        gameOverPanel.SetActive(false);
        remainingTime = gameTime;
        startTime = (float)PhotonNetwork.Time;
        photonView.RPC("SetStartTime", RpcTarget.AllBuffered, startTime);

        // Enable PlayerController script on all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerController>().enabled = true;
        }
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
}
