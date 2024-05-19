using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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

        if(PhotonNetwork.IsMasterClient)
        {
            //Start as tagged
            newPlayer.GetComponent<PlayerController>().photonView.RPC("OnTagged", RpcTarget.AllBuffered);
        }
        else
        {
            newPlayer.GetComponent<PlayerController>().photonView.RPC("OnUnTagged", RpcTarget.AllBuffered);
        }
    }
}
