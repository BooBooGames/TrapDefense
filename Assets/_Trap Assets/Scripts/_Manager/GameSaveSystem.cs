using System;
using UnityEngine;

[Serializable]
public class SaveGameData
{
    public int coins;
    public bool hasSavedCoins;
    public int gems;
    public bool hasSavedGems;
    public int elixir;
    public bool hasSavedElixir;
    public bool[] unlockedWeaponStates;
    public int gearFlowLevel;
    public int baseHealthLevel;
    public PowerCardLevelSaveData[] powerCardLevels;
    public PowerCardCopySaveData[] powerCardCopies;
    public ChestSlotSaveData[] chestSlots;
    public int currentLevel = 1;
    public int chestRewardLevel = 1;
    public bool wave4ChestClaimed;
    public bool wave8ChestClaimed;

    public bool freeSpeedBoostActive;
    public int freeSpeedBoostRemainingSeconds;
    public long freeSpeedBoostExpirationUtc;
    public bool unlimitedSpeedBoostActive;

    public bool musicEnabled = true;
    public bool soundEnabled = true;
    public bool hapticEnabled = true;

    // Future expansion examples:
    // public int selectedWeaponLevel;
    // public int highestUnlockedWave;
    // public string[] unlockedUpgradeIds;
}

[Serializable]
public class PowerCardLevelSaveData
{
    public string cardId;
    public int level = 1;
}

[Serializable]
public class PowerCardCopySaveData
{
    public string cardId;
    public int count;
}

[Serializable]
public class ChestSlotSaveData
{
    public bool hasChest;
    public int chestType;
    public int state;
    public long unlockEndUtc;
    public int coinsReward;
    public int gemsReward;
    public int cardsReward;
}

public static class GameSaveSystem
{
    private const string SaveKey = "trap_defense_save_data";

    public static SaveGameData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return new SaveGameData();
        }

        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new SaveGameData();
        }

        SaveGameData loadedData = JsonUtility.FromJson<SaveGameData>(json);
        return loadedData ?? new SaveGameData();
    }

    public static void Save(SaveGameData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }
}
