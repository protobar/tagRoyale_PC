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
    public GameObject loadingPanel; // Reference to the loading panel
    public GameObject playButtonPanel; // Reference to the play button panel

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            // If already connected, show the loading panel and hide the play button panel
            playButtonPanel.SetActive(false);
            loadingPanel.SetActive(true);
            // Automatically transition to the next scene
            StartCoroutine(TransitionToScene1());
        }
    }

    public void OnClickConnect()
    {
        if (userName.text.Length >= 1)
        {
            PhotonNetwork.NickName = userName.text;
            buttonText.text = "Connecting...";
            loadingPanel.SetActive(true); // Show loading panel
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene(1);
    }

    private IEnumerator TransitionToScene1()
    {
        yield return new WaitForSeconds(1f); // Small delay for the loading panel to be visible
        SceneManager.LoadScene(1);
    }
}
