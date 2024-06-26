using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GraphicsSettingsToggle : MonoBehaviourPunCallbacks
{
    public Toggle lowToggle;
    public Toggle mediumToggle;
    public Toggle highToggle;
    public TMP_Text controlsTypeText; // TMP_Text reference for displaying current controls type

    public Volume volume;
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private bool isBloomAndColorAdjustmentsActive = true;

    public PlayerController playerController; // Reference to the PlayerController script

    private void Start()
    {
        // Ensure the volume is assigned and get the components
        if (volume == null)
        {
            Debug.LogError("Volume not assigned!");
            return;
        }

        // Check if the Volume has the Bloom and ColorAdjustments components
        if (!volume.profile.TryGet(out bloom))
        {
            Debug.LogError("Bloom not found in Volume Profile!");
            return;
        }

        if (!volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("Color Adjustments not found in Volume Profile!");
            return;
        }

        // Ensure the toggles are assigned
        if (lowToggle == null || mediumToggle == null || highToggle == null)
        {
            Debug.LogError("One or more toggles are not assigned!");
            return;
        }

        // Add listeners to each toggle
        lowToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetLowGraphics(); });
        mediumToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetMediumGraphics(); });
        highToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetHighGraphics(); });

        // Find all PlayerController instances in the scene
        PlayerController[] allPlayerControllers = FindObjectsOfType<PlayerController>();

        // Iterate through all PlayerControllers to find the local player's controller
        foreach (PlayerController controller in allPlayerControllers)
        {
            if (controller.photonView.IsMine)
            {
                playerController = controller;
                break;
            }
        }

        if (playerController == null)
        {
            Debug.LogError("Local PlayerController script not found in the scene!");
            return;
        }

        // Update the controls type text initially
        UpdateControlsTypeText();
    }


    public void ToggleEffects()
    {
        isBloomAndColorAdjustmentsActive = !isBloomAndColorAdjustmentsActive;

        bloom.active = isBloomAndColorAdjustmentsActive;
        colorAdjustments.active = isBloomAndColorAdjustmentsActive;
    }

    private void SetLowGraphics()
    {
        // Set graphics quality to low
        QualitySettings.SetQualityLevel(0, true);
        // Ensure other toggles are turned off
        mediumToggle.isOn = false;
        highToggle.isOn = false;
        Debug.Log("Low Graphics Set");
    }

    private void SetMediumGraphics()
    {
        // Set graphics quality to medium
        QualitySettings.SetQualityLevel(2, true);
        // Ensure other toggles are turned off
        lowToggle.isOn = false;
        highToggle.isOn = false;
        Debug.Log("Medium Graphics Set");
    }

    private void SetHighGraphics()
    {
        // Set graphics quality to high
        QualitySettings.SetQualityLevel(5, true);
        // Ensure other toggles are turned off
        lowToggle.isOn = false;
        mediumToggle.isOn = false;
        Debug.Log("High Graphics Set");
    }

    public void ToggleMouseAndKeyboard()
    {
        if (!photonView.IsMine)
        {
            return; // Only allow the local player to toggle controls
        }

        playerController.useMouseAndKeyboard = !playerController.useMouseAndKeyboard;

        // Call an RPC or update state across the network if necessary
        photonView.RPC("SyncControlsState", RpcTarget.All, playerController.useMouseAndKeyboard);

        // Update the controls type text locally
        UpdateControlsTypeText();
    }

    [PunRPC]
    private void SyncControlsState(bool useMouseAndKeyboard)
    {
        playerController.useMouseAndKeyboard = useMouseAndKeyboard;

        // Update the controls type text locally
        UpdateControlsTypeText();
    }
    

    private void UpdateControlsTypeText()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (playerController.useMouseAndKeyboard)
        {
            controlsTypeText.text = "Mouse and Keyboard";
        }
        else
        {
            controlsTypeText.text = "Only Keyboard";
        }
    }
}
