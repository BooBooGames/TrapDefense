using System;
using UnityEngine;

public static class PlayerCurrencySystem
{
    private static bool isInitialized;
    private static int coins;
    private static int gems;
    private static int elixir;

    public static event Action<int, int> CurrencyChanged;
    public static event Action<int> ElixirChanged;

    public static int Coins => coins;
    public static int Gems => gems;
    public static int Elixir => elixir;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isInitialized = false;
        coins = 0;
        gems = 0;
        elixir = 0;
        CurrencyChanged = null;
        ElixirChanged = null;
    }

    public static void Initialize(int defaultCoins, int defaultGems)
    {
        Initialize(defaultCoins, defaultGems, 0);
    }

    public static void Initialize(int defaultCoins, int defaultGems, int defaultElixir)
    {
        SaveGameData saveData = GameSaveSystem.Load();
        int resolvedCoins = saveData.hasSavedCoins
            ? Mathf.Max(0, saveData.coins)
            : Mathf.Max(isInitialized ? coins : 0, Mathf.Max(0, defaultCoins));
        int resolvedGems = saveData.hasSavedGems
            ? Mathf.Max(0, saveData.gems)
            : Mathf.Max(isInitialized ? gems : 0, Mathf.Max(0, defaultGems));
        int resolvedElixir = saveData.hasSavedElixir
            ? Mathf.Max(0, saveData.elixir)
            : Mathf.Max(isInitialized ? elixir : 0, Mathf.Max(0, defaultElixir));

        bool valuesChanged = !isInitialized || resolvedCoins != coins || resolvedGems != gems;
        bool elixirChanged = !isInitialized || resolvedElixir != elixir;

        coins = resolvedCoins;
        gems = resolvedGems;
        elixir = resolvedElixir;
        isInitialized = true;

        if (valuesChanged)
        {
            NotifyCurrencyChanged();
        }

        if (elixirChanged)
        {
            NotifyElixirChanged();
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
        SaveGems();
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
        SaveGems();
        NotifyCurrencyChanged();
        return true;
    }

    public static void AddElixir(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        EnsureInitialized();
        elixir = Mathf.Max(0, elixir + amount);
        SaveElixir();
        NotifyElixirChanged();
    }

    public static void ReloadElixirFromSave()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        int resolvedElixir = Mathf.Max(0, saveData.elixir);
        bool elixirChanged = !isInitialized || resolvedElixir != elixir;

        elixir = resolvedElixir;
        isInitialized = true;

        if (elixirChanged)
        {
            NotifyElixirChanged();
        }
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

    private static void SaveGems()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        saveData.gems = gems;
        saveData.hasSavedGems = true;
        GameSaveSystem.Save(saveData);
    }

    private static void SaveElixir()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        saveData.elixir = elixir;
        saveData.hasSavedElixir = true;
        GameSaveSystem.Save(saveData);
    }

    private static void NotifyCurrencyChanged()
    {
        CurrencyChanged?.Invoke(coins, gems);
    }

    private static void NotifyElixirChanged()
    {
        ElixirChanged?.Invoke(elixir);
    }
}
