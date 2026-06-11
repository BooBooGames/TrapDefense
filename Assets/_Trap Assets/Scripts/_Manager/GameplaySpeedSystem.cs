using System;
using UnityEngine;

public static class GameplaySpeedSystem
{
    public const float NormalSpeedMultiplier = 1f;
    public const float BoostedSpeedMultiplier = 2f;
    public const int FreeBoostDurationSeconds = 15 * 60;

    private const float RemainingSaveIntervalSeconds = 1f;

    private static bool freeBoostActive;
    private static int freeBoostRemainingSeconds;
    private static long freeBoostExpirationUtc;
    private static bool unlimitedBoostActive;
    private static float nextRemainingSaveTime;

    public static bool FreeBoostActive => freeBoostActive;
    public static int FreeBoostRemainingSeconds => freeBoostRemainingSeconds;
    public static long FreeBoostExpirationUtc => freeBoostExpirationUtc;
    public static bool UnlimitedBoostActive => unlimitedBoostActive;
    public static bool IsBoostActive => unlimitedBoostActive || freeBoostActive;
    public static float CurrentGameplayTimeScale => IsBoostActive ? BoostedSpeedMultiplier : NormalSpeedMultiplier;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        freeBoostActive = false;
        freeBoostRemainingSeconds = 0;
        freeBoostExpirationUtc = 0;
        unlimitedBoostActive = false;
        nextRemainingSaveTime = 0f;
    }

    public static void Initialize()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        ApplySaveData(saveData);
        bool saveChanged = ValidateFreeBoost(saveData);

        if (saveChanged)
        {
            GameSaveSystem.Save(saveData);
        }
    }

    public static void Tick()
    {
        SaveGameData saveData = null;
        bool saveChanged = false;

        if (freeBoostActive)
        {
            saveData = GameSaveSystem.Load();
            ApplySaveData(saveData);
            saveChanged = ValidateFreeBoost(saveData);

            if (freeBoostActive && Time.unscaledTime >= nextRemainingSaveTime)
            {
                saveData.freeSpeedBoostRemainingSeconds = freeBoostRemainingSeconds;
                saveChanged = true;
                nextRemainingSaveTime = Time.unscaledTime + RemainingSaveIntervalSeconds;
            }
        }

        if (saveChanged)
        {
            GameSaveSystem.Save(saveData);
        }

    }

    public static void ActivateFreeBoost()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        long expirationUtc = GetUtcNowSeconds() + FreeBoostDurationSeconds;

        saveData.freeSpeedBoostActive = true;
        saveData.freeSpeedBoostRemainingSeconds = FreeBoostDurationSeconds;
        saveData.freeSpeedBoostExpirationUtc = expirationUtc;

        ApplySaveData(saveData);
        GameSaveSystem.Save(saveData);
        ApplyCurrentSpeedToTimeScale(true);
    }

    public static void ActivateUnlimitedBoost()
    {
        SaveGameData saveData = GameSaveSystem.Load();

        saveData.unlimitedSpeedBoostActive = true;
        saveData.freeSpeedBoostActive = false;
        saveData.freeSpeedBoostRemainingSeconds = 0;
        saveData.freeSpeedBoostExpirationUtc = 0;

        ApplySaveData(saveData);
        GameSaveSystem.Save(saveData);
        ApplyCurrentSpeedToTimeScale(true);
    }

    public static void ApplyCurrentSpeedToTimeScale(bool forceWhenPaused = false)
    {
        if (!forceWhenPaused && Time.timeScale <= 0f)
        {
            return;
        }

        Time.timeScale = CurrentGameplayTimeScale;
    }

    public static void Flush()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        ApplySaveData(saveData);

        bool saveChanged = ValidateFreeBoost(saveData);
        if (freeBoostActive)
        {
            saveData.freeSpeedBoostRemainingSeconds = freeBoostRemainingSeconds;
            saveChanged = true;
        }

        if (saveChanged)
        {
            GameSaveSystem.Save(saveData);
        }
    }

    private static void ApplySaveData(SaveGameData saveData)
    {
        freeBoostActive = saveData.freeSpeedBoostActive;
        freeBoostRemainingSeconds = Mathf.Max(0, saveData.freeSpeedBoostRemainingSeconds);
        freeBoostExpirationUtc = Math.Max(0, saveData.freeSpeedBoostExpirationUtc);
        unlimitedBoostActive = saveData.unlimitedSpeedBoostActive;
    }

    private static bool ValidateFreeBoost(SaveGameData saveData)
    {
        bool changed = false;
        long now = GetUtcNowSeconds();

        if (freeBoostActive && freeBoostExpirationUtc <= 0 && freeBoostRemainingSeconds > 0)
        {
            freeBoostExpirationUtc = now + freeBoostRemainingSeconds;
            saveData.freeSpeedBoostExpirationUtc = freeBoostExpirationUtc;
            changed = true;
        }

        if (freeBoostActive && freeBoostExpirationUtc > now)
        {
            freeBoostRemainingSeconds = (int)Math.Max(0, freeBoostExpirationUtc - now);

            if (saveData.freeSpeedBoostRemainingSeconds != freeBoostRemainingSeconds)
            {
                saveData.freeSpeedBoostRemainingSeconds = freeBoostRemainingSeconds;
                changed = true;
            }

            return changed;
        }

        if (freeBoostActive || freeBoostRemainingSeconds != 0 || freeBoostExpirationUtc != 0)
        {
            freeBoostActive = false;
            freeBoostRemainingSeconds = 0;
            freeBoostExpirationUtc = 0;

            saveData.freeSpeedBoostActive = false;
            saveData.freeSpeedBoostRemainingSeconds = 0;
            saveData.freeSpeedBoostExpirationUtc = 0;
            changed = true;
        }

        return changed;
    }

    private static long GetUtcNowSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
