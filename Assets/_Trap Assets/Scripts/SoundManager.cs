using UnityEngine;
using Solo.MOST_IN_ONE;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public AudioListener audioListener;
    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySound(AudioClip clip)
    {
        if (GameSettingsSystem.SoundEnabled)
        {
            if (clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClickSound()
    {

        PlaySound(buttonClickSound);
        MediumImpactHaptic();
    }

    public void HapticEnable(bool enable)
    {
        MOST_HapticFeedback.HapticsEnabled = enable;
    }

    public void LightImpactHaptic()
    {
        if (GameSettingsSystem.HapticEnabled)
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.LightImpact);
        }
    }

    public void MediumImpactHaptic()
    {
        if (GameSettingsSystem.HapticEnabled)
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
        }
    }

    public void HeavyImpactHaptic()
    {
        if (GameSettingsSystem.HapticEnabled)
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        }
    }
}
