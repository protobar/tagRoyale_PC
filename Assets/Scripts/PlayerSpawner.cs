using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;
    public GameObject jumpButton;
    public GameObject boostButton;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
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

        // Ensure each player has their own jump and boost buttons
        if (jumpButton != null && boostButton != null)
        {
            // Get the PlayerController script of the spawned player
            PlayerController playerController = newPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Assign jump button reference and setup its onClick event
                Button jumpButtonComponent = jumpButton.GetComponent<Button>();
                if (jumpButtonComponent != null)
                {
                    jumpButtonComponent.onClick.AddListener(() => { playerController.OnJumpButtonPressed(); });
                }

                // Assign boost button reference and setup its onClick event
                Button boostButtonComponent = boostButton.GetComponent<Button>();
                if (boostButtonComponent != null)
                {
                    boostButtonComponent.onClick.AddListener(() => { playerController.OnBoostButtonPressed(); });
                }
            }
        }

        // Handle tagging for the player
        if (PhotonNetwork.IsMasterClient)
        {
            // Start as tagged
            newPlayer.GetComponent<PlayerController>().photonView.RPC("OnTagged", RpcTarget.AllBuffered);
        }
        else
        {
            newPlayer.GetComponent<PlayerController>().photonView.RPC("OnUnTagged", RpcTarget.AllBuffered);
        }
    }
}
