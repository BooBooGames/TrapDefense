using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager Instance;

    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        Instance = this;
    }

    public static void StartAudio()
    {
        if (!GameSettingsSystem.MusicEnabled) return;

        Instance._audioSource.Play();
    }

    public static void StopAudio()
    {
        Instance._audioSource.Stop();
    }
}
