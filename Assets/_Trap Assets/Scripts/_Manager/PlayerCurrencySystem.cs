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
        SaveGameData saveData = GameSaveSystem.Load();
        int resolvedCoins = saveData.hasSavedCoins
            ? Mathf.Max(0, saveData.coins)
            : Mathf.Max(isInitialized ? coins : 0, Mathf.Max(0, defaultCoins));
        int resolvedGems = Mathf.Max(isInitialized ? gems : 0, Mathf.Max(0, defaultGems));

        bool valuesChanged = !isInitialized || resolvedCoins != coins || resolvedGems != gems;

        coins = resolvedCoins;
        gems = resolvedGems;
        isInitialized = true;

        if (valuesChanged)
        {
            NotifyCurrencyChanged();
        }
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

    public static bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        EnsureInitialized();
        if (coins < amount)
        {
            return false;
        }

        coins -= amount;
        SaveCoins();
        NotifyCurrencyChanged();
        return true;
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

    public static bool TrySpendGems(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        EnsureInitialized();
        if (gems < amount)
        {
            return false;
        }

        gems -= amount;
        NotifyCurrencyChanged();
        return true;
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
