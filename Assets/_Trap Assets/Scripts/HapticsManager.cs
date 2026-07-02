using UnityEngine;
using Solo.MOST_IN_ONE;

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GameSettingsSystem.Initialize();
    }

    public void HapticEnable(bool enable)
    {
        MOST_HapticFeedback.HapticsEnabled = enable;
    }

    public static void LightImpactHaptic()
    {
        if (GameSettingsSystem.HapticEnabled)
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.LightImpact);
        }
    }

    public static void MediumImpactHaptic()
    {
        if (GameSettingsSystem.HapticEnabled)
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
        }
    }

    public static void HeavyImpactHaptic()
    {
        if (GameSettingsSystem.HapticEnabled)
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        }
    }
}
