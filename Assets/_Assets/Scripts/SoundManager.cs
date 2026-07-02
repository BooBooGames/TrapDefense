using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum AudioType
{
    UI_Button_Click,
    Zombie_Idle,
    Zombie_Alert,
    Zombie_Attack,
    Zombie_Hurt,
    Zombie_Death,
    Get_Reward,
    Reward_Animation,
    Level_Up,
}

[Serializable]
public class AudioData
{
    public AudioType _AudioType;
    [Range(0f, 1f)] public float Volume;
    public bool AllowMultiple;
    public bool ShouldLoop;
    public AudioClip[] _AudioClipsArray;
    [Range(0f, 1f)] public float Cooldown;
    public Vector2 _PitchRange = new Vector2(1f, 1f);
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager Instance;

    [SerializeField] private AudioSourceHandler audioSourcePrefab;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioData> _audioClipsList;

    public static event Action<AudioType> OnSfxFinishedPlaying;

    private List<AudioSourceHandler> availableAudioSourcesList;
    private List<AudioSourceHandler> occupiedAudioSourcesList;
    private Dictionary<AudioSourceHandler, AudioType> audioSourceHandlerToAudioTypeMap;

    private int index = 0;

    private void Awake()
    {
        Instance = this;

        audioSourceHandlerToAudioTypeMap = new();
        availableAudioSourcesList = new List<AudioSourceHandler>();
        occupiedAudioSourcesList = new List<AudioSourceHandler>();
    }

    private void Start()
    {

    }

    private AudioData GetAudioClipDataArrayFromAudioType(AudioType pAudioType)
    {
        foreach(var audioData in _audioClipsList)
        {
            if(audioData._AudioType == pAudioType)
            {
                return audioData;
            }
        }

        return null;
    }

    private void PlayAudio(AudioData audioData)
    {
        if (audioData._AudioClipsArray.Length == 0 || !GameSettingsSystem.SoundEnabled) return;

        AudioClip mainAudioClip = GetRandomItemFrom(audioData._AudioClipsArray);

        if(mainAudioClip == null) return;

        if(!audioData.AllowMultiple && IsAnyOccupiedSourcePlayingThisAudioData(audioData, out AudioSourceHandler audioSourceHandler))
        {
            audioSourceHandler.PlayWithClip(audioData, mainAudioClip);
        }
        else
        {
            if(availableAudioSourcesList.Count == 0)
            {
                AudioSourceHandler newSource = Instantiate(audioSourcePrefab, transform);

                newSource.gameObject.name = $"AHS_{index++}";
                newSource.Setup(this);
                newSource.PlayWithClip(audioData, mainAudioClip);
                occupiedAudioSourcesList.Add(newSource);
                audioSourceHandlerToAudioTypeMap.Add(newSource, audioData._AudioType);
            }
            else
            {
                AudioSourceHandler currentSource = availableAudioSourcesList[^1];

                currentSource.PlayWithClip(audioData, mainAudioClip);

                availableAudioSourcesList.Remove(currentSource);
                occupiedAudioSourcesList.Add(currentSource);
                audioSourceHandlerToAudioTypeMap.Add(currentSource, audioData._AudioType);
            }
        }
    }

    private bool IsAnyOccupiedSourcePlayingThisAudioData(AudioData audioData, out AudioSourceHandler audioSourceHandler)
    {
        foreach(AudioSourceHandler handler in occupiedAudioSourcesList)
        {
            if(handler._audioDataBeingPlayed == audioData)
            {
                audioSourceHandler = handler;
                return true;
            }
        }

        audioSourceHandler = null;
        return false;
    }

    public static void PlayAudio(AudioType pAudioType)
    {
        if (Instance == null) return;

        Instance.PlayAudio(Instance.GetAudioClipDataArrayFromAudioType(pAudioType));
    }

    public static void PlayButtonClickSound()
    {
        PlayAudio(AudioType.UI_Button_Click);
        HapticsManager.MediumImpactHaptic();
    }

    public static T GetRandomItemFrom<T>(T[] array)
    {
        if (array == null || array.Length == 0) return default;

        return array[Random.Range(0, array.Length)];
    }

    public void ReleaseAudioSourceHandler(AudioSourceHandler audioSourceHandler)
    {
        availableAudioSourcesList.Add(audioSourceHandler);
        occupiedAudioSourcesList.Remove(audioSourceHandler);

        OnSfxFinishedPlaying?.Invoke(audioSourceHandlerToAudioTypeMap[audioSourceHandler]);

        audioSourceHandlerToAudioTypeMap.Remove(audioSourceHandler);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var enumValues = (AudioType[])Enum.GetValues(typeof(AudioType));

        _audioClipsList ??= new List<AudioData>();

        Dictionary<AudioType, AudioData> existingMap = new();

        foreach (var data in _audioClipsList)
        {
            if (!existingMap.ContainsKey(data._AudioType))
            {
                existingMap.Add(data._AudioType, data);
            }
        }

        List<AudioData> newList = new List<AudioData>(enumValues.Length);

        foreach (var enumValue in enumValues)
        {
            if (existingMap.TryGetValue(enumValue, out var existingData))
            {
                newList.Add(existingData);
            }
            else
            {
                newList.Add(new AudioData
                {
                    _AudioType = enumValue,
                    Volume = 1f,
                });
            }
        }

        _audioClipsList = newList;
    }
#endif
}
