using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ZombieSoundRule
{
    public AudioType AudioType;

    public float PerZombieCooldown = 2f;

    public float GlobalCooldown = 0.2f;

    public int MaxVoices = 4;
}

public class ZombieAudioManager : MonoBehaviour
{
    private static ZombieAudioManager Instance;

    [SerializeField] private ZombieSoundRule[] rules;

    private Dictionary<AudioType, ZombieSoundRule> ruleLookup;

    private readonly Dictionary<(Transform, AudioType), float> zombieCooldowns = new();

    private readonly Dictionary<AudioType, float> globalCooldowns = new();

    private readonly Dictionary<AudioType, int> activeVoices = new();

    private void Awake()
    {
        Instance = this;

        ruleLookup = rules.ToDictionary(x => x.AudioType);

        foreach (var rule in rules)
        {
            activeVoices[rule.AudioType] = 0;
        }
    }

    private void Start()
    {
        SoundManager.OnSfxFinishedPlaying += HandleOnSfxFinishedPlaying;
    }

    private void HandleOnSfxFinishedPlaying(AudioType pAudioType)
    {
        if(activeVoices.ContainsKey(pAudioType))
        {
            activeVoices[pAudioType] = Mathf.Max(0, activeVoices[pAudioType] - 1);
        }
    }

    public static void RequestSound(Transform pZombieTransform, AudioType pZombieSoundType)
    {
        if (!Instance.ruleLookup.TryGetValue(pZombieSoundType, out var rule))
            return;

        if (!PassPerZombieCooldown(pZombieTransform, rule))
            return;

        if (!PassVoiceLimit(rule))
            return;

        SoundManager.PlayAudio(pZombieSoundType);
    }

    private static bool PassPerZombieCooldown(Transform pZombieTransform, ZombieSoundRule pZombieSoundRule)
    {
        var key = (pZombieTransform, pZombieSoundRule.AudioType);

        if (Instance.zombieCooldowns.TryGetValue(key, out float nextTime))
        {
            if (Time.time < nextTime) return false;
        }

        Instance.zombieCooldowns[key] = Time.time + pZombieSoundRule.PerZombieCooldown;

        return true;
    }

    private static bool PassGlobalCooldown(ZombieSoundRule pZombieSoundRule)
    {
        if (Instance.globalCooldowns.TryGetValue(pZombieSoundRule.AudioType, out float nextTime))
        {
            if (Time.time < nextTime) return false;
        }

        Instance.globalCooldowns[pZombieSoundRule.AudioType] = Time.time + pZombieSoundRule.GlobalCooldown;

        return true;
    }

    private static bool PassVoiceLimit(ZombieSoundRule pZombieSoundRule)
    {
        if (Instance.activeVoices[pZombieSoundRule.AudioType] >= pZombieSoundRule.MaxVoices) return false;

        Instance.activeVoices[pZombieSoundRule.AudioType]++;

        return true;
    }

    private void OnDestroy()
    {
        SoundManager.OnSfxFinishedPlaying -= HandleOnSfxFinishedPlaying;
    }
}