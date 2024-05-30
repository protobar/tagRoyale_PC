using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class LobbyManager : MonoBehaviourPunCallbacks
{

    public TMP_InputField roomInputField;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject createRoomPanel;
    public GameObject browseRoom;
    public TMP_Text roomName;
    public int maxPlayers = 2;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    [Header("Room Stuff")]
    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject;

    [Header("Player Stuff")]
    List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParent;

    public TMP_Text[] nicknameTexts;

    public GameObject playBtn;
    void Start()
    {
        PhotonNetwork.JoinLobby();
        // Ensure the nickname is set correctly at the start
        UpdateNicknameTexts(PhotonNetwork.NickName);
    }

    /*private void OnEnable()
    {
        // Register for any potential nickname changes
        PhotonNetwork.NickName = PhotonNetwork.NickName; // Initial set
        UpdateNicknameTexts(PhotonNetwork.NickName);
    }*/

    public void OnClickCreate()
    {
        if(roomInputField.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions() { MaxPlayers = maxPlayers, BroadcastPropsChangeToAll = true });
            createRoomPanel.SetActive(false);
        }
    }
    public void UpdateNicknameTexts(string nickname)
    {
        // Update all TMP_Text elements with the provided nickname
        foreach (TMP_Text nicknameText in nicknameTexts)
        {
            if (nicknameText != null)
            {
                nicknameText.text = nickname;
            }
        }
    }
    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if(Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach(RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach(RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void UpdatePlayerList()
    {
        foreach(PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);   
        }
        playerItemsList.Clear();

        if(PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach(KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);

            if(player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }
            playerItemsList.Add(newPlayerItem);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            playBtn.SetActive(true);
        }
        else
        {
            playBtn.SetActive(false);
        }
    }

    public void OnClickLoadMap1()
    {
        PhotonNetwork.LoadLevel(2);
    }

    public void OnClickLoadMap2()
    {
        PhotonNetwork.LoadLevel(3);

    }
}
