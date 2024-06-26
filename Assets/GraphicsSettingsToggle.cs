using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicsSettingsToggle : MonoBehaviour
{
    public Toggle lowToggle;
    public Toggle mediumToggle;
    public Toggle highToggle;

    public Volume volume;
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private bool isBloomAndColorAdjustmentsActive = true;


    private void Start()
    {
        //PostProcessing
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
}
