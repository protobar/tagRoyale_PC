using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ToggleBloomAndColorAdjustments : MonoBehaviour
{
    public Volume volume;
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private bool isBloomAndColorAdjustmentsActive = true;

    void Start()
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
    }

    public void ToggleEffects()
    {
        isBloomAndColorAdjustmentsActive = !isBloomAndColorAdjustmentsActive;

        bloom.active = isBloomAndColorAdjustmentsActive;
        colorAdjustments.active = isBloomAndColorAdjustmentsActive;
    }
}
