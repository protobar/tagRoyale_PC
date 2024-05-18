using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTSManager : MonoBehaviour
{
    public GameObject playPanel;
    public GameObject connectToServerPanel;
    public GameObject blackScreen;

    private void Start()
    {
        playPanel.SetActive(true);
        connectToServerPanel.SetActive(false);
        Invoke("TurnOffBlackScreen", 1f);

    }

    void TurnOffBlackScreen()
    {
        blackScreen.SetActive(false);
    }

}
