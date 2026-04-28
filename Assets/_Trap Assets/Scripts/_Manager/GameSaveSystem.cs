using System;
using UnityEngine;

[Serializable]
public class SaveGameData
{
    public int coins;
    public bool hasSavedCoins;

    // Future expansion examples:
    // public int selectedWeaponLevel;
    // public int highestUnlockedWave;
    // public string[] unlockedUpgradeIds;
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
