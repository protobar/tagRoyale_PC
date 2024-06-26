using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] == null)
        {
            PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] = 0;
        }

        // Get the index of the local player in the player list
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Use the player index to select a spawn point
        Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];

        // Instantiate the player prefab at the selected spawn point
        GameObject playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
        var newPlayer = PhotonNetwork.Instantiate(playerToSpawn.name, spawnPoint.position, Quaternion.identity);

        // Handle tagging for the player
        if (PhotonNetwork.IsMasterClient)
        {
            // The master client selects a random player to be tagged
            StartCoroutine(AssignRandomTag());
        }
    }

    private IEnumerator AssignRandomTag()
    {
        // Wait for all players to be instantiated
        yield return new WaitForSeconds(1.0f);

        // Get a list of all players in the room
        Player[] players = PhotonNetwork.PlayerList;

        // Select a random player
        int randomIndex = Random.Range(0, players.Length);
        Player randomPlayer = players[randomIndex];

        // Notify all players who is tagged
        PhotonNetwork.RaiseEvent(0, randomPlayer.ActorNumber, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }

    [PunRPC]
    private void TagPlayer(int actorNumber)
    {
        // Find the player with the given actor number and tag them
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.photonView.Owner.ActorNumber == actorNumber)
            {
                player.photonView.RPC("OnTagged", RpcTarget.AllBuffered);
            }
            else
            {
                player.photonView.RPC("OnUnTagged", RpcTarget.AllBuffered);
            }
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 0)
        {
            int actorNumber = (int)photonEvent.CustomData;
            TagPlayer(actorNumber);
        }
    }
}
