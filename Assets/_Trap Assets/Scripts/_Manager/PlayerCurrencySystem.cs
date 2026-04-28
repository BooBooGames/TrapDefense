using System;
using UnityEngine;

public static class PlayerCurrencySystem
{
    private static bool isInitialized;
    private static int coins;
    private static int gems;

    public static event Action<int, int> CurrencyChanged;

    public static int Coins => coins;
    public static int Gems => gems;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isInitialized = false;
        coins = 0;
        gems = 0;
        CurrencyChanged = null;
    }

    public static void Initialize(int defaultCoins, int defaultGems)
    {
        if (isInitialized)
        {
            return;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        coins = saveData.hasSavedCoins ? Mathf.Max(0, saveData.coins) : Mathf.Max(0, defaultCoins);
        gems = Mathf.Max(0, defaultGems);
        isInitialized = true;
    }

    public static void AddCoins(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        EnsureInitialized();
        coins = Mathf.Max(0, coins + amount);
        SaveCoins();
        NotifyCurrencyChanged();
    }

    public static void AddGems(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        EnsureInitialized();
        gems = Mathf.Max(0, gems + amount);
        NotifyCurrencyChanged();
    }

    private static void EnsureInitialized()
    {
        if (!isInitialized)
        {
            Initialize(0, 0);
        }
    }

    private static void SaveCoins()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        saveData.coins = coins;
        saveData.hasSavedCoins = true;
        GameSaveSystem.Save(saveData);
    }

    private static void NotifyCurrencyChanged()
    {
        CurrencyChanged?.Invoke(coins, gems);
    }
}
