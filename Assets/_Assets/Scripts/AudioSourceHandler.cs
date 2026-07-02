using System.Collections;
using UnityEngine;

public class AudioSourceHandler : MonoBehaviour
{
    private SoundManager soundManager;

    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public AudioClip audioClipBeingPlayed = null;
    [HideInInspector] public AudioData _audioDataBeingPlayed;

    private bool _isCooldownActive = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator HandleAudioClipDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        audioSource.clip = null;
        soundManager.ReleaseAudioSourceHandler(this);
    }

    private IEnumerator HandleCooldown(float pCooldown)
    {
        if (pCooldown == 0f) yield break;

        _isCooldownActive = true;
        yield return new WaitForSeconds(pCooldown);
        _isCooldownActive = false;
    }

    public void PlayWithClip(AudioData audioData, AudioClip mainAudioClip)
    {
        if (_isCooldownActive) return;

        StopAllCoroutines();
        audioSource.Stop();

        StartCoroutine(HandleCooldown(audioData.Cooldown));
        _audioDataBeingPlayed = audioData;
        audioSource.clip = mainAudioClip;
        audioSource.volume = audioData.Volume;
        audioSource.loop = audioData.ShouldLoop;
        audioSource.pitch = Random.Range(audioData._PitchRange.x, audioData._PitchRange.y);
        isOccupied = true;

        audioClipBeingPlayed = mainAudioClip;
        audioSource.Play();

        StartCoroutine(HandleAudioClipDuration(mainAudioClip.length));
    }

    public void Setup(SoundManager soundManager)
    {
        audioSource = GetComponent<AudioSource>();
        this.soundManager = soundManager;
    }
}
