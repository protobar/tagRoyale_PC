using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField userName;
    public TMP_Text buttonText;
    

    public void OnClickConnect()
    {
        if(userName.text.Length >= 1)
        {
            PhotonNetwork.NickName = userName.text;
            buttonText.text = "Connecting...";
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene(1);
    }
}
