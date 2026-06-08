using System;
using UnityEngine;

public static class PlayerUpgradeSystem
{
    private static bool isInitialized;
    private static UpgradeScreenConfig config;
    private static bool[] unlockedWeaponStates = Array.Empty<bool>();
    private static int gearFlowLevel;
    private static int baseHealthLevel;

    public static event Action UpgradeStateChanged;

    public static UpgradeScreenConfig Config => CurrentConfig;
    public static int GearFlowLevel => gearFlowLevel;
    public static int BaseHealthLevel => baseHealthLevel;
    public static float CurrentGearFlowValue => CurrentConfig.GearFlow.EvaluateValue(gearFlowLevel);
    public static int CurrentBaseHealthValue => CurrentConfig.BaseHealth.EvaluateValue(baseHealthLevel);
    public static int CurrentBaseHealthBonus => Mathf.Max(0, CurrentBaseHealthValue - CurrentConfig.BaseHealth.defaultValue);

    private static UpgradeScreenConfig CurrentConfig => config != null ? config : UpgradeScreenConfig.Resolve(null);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isInitialized = false;
        config = null;
        unlockedWeaponStates = Array.Empty<bool>();
        gearFlowLevel = 0;
        baseHealthLevel = 0;
        UpgradeStateChanged = null;
    }

    public static void Initialize(UpgradeScreenConfig sourceConfig)
    {
        config = UpgradeScreenConfig.Resolve(sourceConfig);

        SaveGameData saveData = GameSaveSystem.Load();
        bool saveChanged = EnsureSaveDefaults(saveData);
        ApplySaveData(saveData);
        isInitialized = true;

        if (saveChanged)
        {
            GameSaveSystem.Save(saveData);
        }
    }

    public static bool IsWeaponUnlocked(int weaponIndex)
    {
        EnsureInitialized();
        return weaponIndex >= 0 &&
            weaponIndex < unlockedWeaponStates.Length &&
            unlockedWeaponStates[weaponIndex];
    }

    public static UpgradeResourceCost GetWeaponUnlockCost(int weaponIndex)
    {
        EnsureInitialized();

        WeaponUnlockDefinition weapon = CurrentConfig.GetWeapon(weaponIndex);
        return weapon != null ? weapon.unlockCost : default;
    }

    public static bool CanUpgradeGearFlow()
    {
        EnsureInitialized();
        return gearFlowLevel < CurrentConfig.GearFlow.maxLevel;
    }

    public static UpgradeResourceCost GetGearFlowUpgradeCost()
    {
        EnsureInitialized();
        UpgradeResourceCost cost = CanUpgradeGearFlow()
            ? CurrentConfig.GearFlow.EvaluateUpgradeCost(gearFlowLevel + 1)
            : default;

        return ApplyGearUpgradeCostModifiers(cost);
    }

    public static bool CanUpgradeBaseHealth()
    {
        EnsureInitialized();
        return baseHealthLevel < CurrentConfig.BaseHealth.maxLevel;
    }

    public static UpgradeResourceCost GetBaseHealthUpgradeCost()
    {
        EnsureInitialized();
        UpgradeResourceCost cost = CanUpgradeBaseHealth()
            ? CurrentConfig.BaseHealth.EvaluateUpgradeCost(baseHealthLevel + 1)
            : default;

        return ApplyGearUpgradeCostModifiers(cost);
    }

    public static bool CanAfford(UpgradeResourceCost cost)
    {
        PlayerCurrencySystem.Initialize(0, 0);

        bool enoughCoins = PlayerCurrencySystem.Coins >= cost.coins;
        bool enoughGears = cost.gears <= 0 ||
            (GameViewScreen.Instance != null && GameViewScreen.Instance.GearCount >= cost.gears);

        return enoughCoins && enoughGears;
    }

    public static bool TryUnlockWeapon(int weaponIndex)
    {
        EnsureInitialized();

        if (weaponIndex < 0 || weaponIndex >= CurrentConfig.WeaponCount || IsWeaponUnlocked(weaponIndex))
        {
            return false;
        }

        UpgradeResourceCost cost = GetWeaponUnlockCost(weaponIndex);
        if (!TrySpendCost(cost))
        {
            return false;
        }

        unlockedWeaponStates[weaponIndex] = true;
        SaveWeaponUnlockState();
        UpgradeStateChanged?.Invoke();
        return true;
    }

    public static bool TryUpgradeGearFlow()
    {
        EnsureInitialized();

        if (!CanUpgradeGearFlow())
        {
            return false;
        }

        UpgradeResourceCost cost = GetGearFlowUpgradeCost();
        if (!TrySpendCost(cost))
        {
            return false;
        }

        gearFlowLevel++;
        SaveStatLevels();
        UpgradeStateChanged?.Invoke();
        return true;
    }

    public static bool TryUpgradeBaseHealth()
    {
        EnsureInitialized();

        if (!CanUpgradeBaseHealth())
        {
            return false;
        }

        UpgradeResourceCost cost = GetBaseHealthUpgradeCost();
        if (!TrySpendCost(cost))
        {
            return false;
        }

        baseHealthLevel++;
        SaveStatLevels();
        UpgradeStateChanged?.Invoke();
        return true;
    }

    public static void ResetProgressionStateToDefaults(UpgradeScreenConfig sourceConfig)
    {
        config = UpgradeScreenConfig.Resolve(sourceConfig);
        unlockedWeaponStates = CreateDefaultWeaponUnlockStates();
        gearFlowLevel = 0;
        baseHealthLevel = 0;

        SaveGameData saveData = GameSaveSystem.Load();
        saveData.unlockedWeaponStates = (bool[])unlockedWeaponStates.Clone();
        saveData.gearFlowLevel = gearFlowLevel;
        saveData.baseHealthLevel = baseHealthLevel;
        GameSaveSystem.Save(saveData);

        isInitialized = true;
        UpgradeStateChanged?.Invoke();
    }

    public static void ResetWeaponUnlockStateToDefaults(UpgradeScreenConfig sourceConfig)
    {
        ResetProgressionStateToDefaults(sourceConfig);
    }

    private static void EnsureInitialized()
    {
        if (!isInitialized)
        {
            Initialize(null);
        }
    }

    private static bool EnsureSaveDefaults(SaveGameData saveData)
    {
        bool changed = false;

        if (saveData.unlockedWeaponStates == null || saveData.unlockedWeaponStates.Length != CurrentConfig.WeaponCount)
        {
            bool[] previousStates = saveData.unlockedWeaponStates ?? Array.Empty<bool>();
            saveData.unlockedWeaponStates = new bool[CurrentConfig.WeaponCount];

            for (int i = 0; i < saveData.unlockedWeaponStates.Length; i++)
            {
                bool isUnlocked = i < previousStates.Length && previousStates[i];
                WeaponUnlockDefinition weapon = CurrentConfig.GetWeapon(i);
                if (weapon != null && weapon.unlockedByDefault)
                {
                    isUnlocked = true;
                }

                saveData.unlockedWeaponStates[i] = isUnlocked;
            }

            changed = true;
        }
        else
        {
            for (int i = 0; i < saveData.unlockedWeaponStates.Length; i++)
            {
                WeaponUnlockDefinition weapon = CurrentConfig.GetWeapon(i);
                if (weapon != null && weapon.unlockedByDefault && !saveData.unlockedWeaponStates[i])
                {
                    saveData.unlockedWeaponStates[i] = true;
                    changed = true;
                }
            }
        }

        int clampedGearFlowLevel = Mathf.Clamp(saveData.gearFlowLevel, 0, CurrentConfig.GearFlow.maxLevel);
        if (clampedGearFlowLevel != saveData.gearFlowLevel)
        {
            saveData.gearFlowLevel = clampedGearFlowLevel;
            changed = true;
        }

        int clampedBaseHealthLevel = Mathf.Clamp(saveData.baseHealthLevel, 0, CurrentConfig.BaseHealth.maxLevel);
        if (clampedBaseHealthLevel != saveData.baseHealthLevel)
        {
            saveData.baseHealthLevel = clampedBaseHealthLevel;
            changed = true;
        }

        return changed;
    }

    private static bool[] CreateDefaultWeaponUnlockStates()
    {
        bool[] defaultStates = new bool[CurrentConfig.WeaponCount];
        for (int i = 0; i < defaultStates.Length; i++)
        {
            WeaponUnlockDefinition weapon = CurrentConfig.GetWeapon(i);
            defaultStates[i] = weapon != null && weapon.unlockedByDefault;
        }

        return defaultStates;
    }

    private static void ApplySaveData(SaveGameData saveData)
    {
        unlockedWeaponStates = (bool[])saveData.unlockedWeaponStates.Clone();
        gearFlowLevel = saveData.gearFlowLevel;
        baseHealthLevel = saveData.baseHealthLevel;
    }

    private static void SaveWeaponUnlockState()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        saveData.unlockedWeaponStates = (bool[])unlockedWeaponStates.Clone();
        GameSaveSystem.Save(saveData);
    }

    private static void SaveStatLevels()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        saveData.gearFlowLevel = gearFlowLevel;
        saveData.baseHealthLevel = baseHealthLevel;
        GameSaveSystem.Save(saveData);
    }

    private static bool TrySpendCost(UpgradeResourceCost cost)
    {
        if (!CanAfford(cost))
        {
            return false;
        }

        if (cost.coins > 0 && !PlayerCurrencySystem.TrySpendCoins(cost.coins))
        {
            return false;
        }

        if (cost.gears > 0)
        {
            GameViewScreen gameViewScreen = GameViewScreen.Instance;
            if (gameViewScreen == null || !gameViewScreen.TrySpendGears(cost.gears))
            {
                if (cost.coins > 0)
                {
                    PlayerCurrencySystem.AddCoins(cost.coins);
                }

                return false;
            }
        }

        return true;
    }

    private static UpgradeResourceCost ApplyGearUpgradeCostModifiers(UpgradeResourceCost cost)
    {
        return PlayerXpSystem.Instance != null
            ? PlayerXpSystem.Instance.ApplyGearUpgradeCostModifiers(cost)
            : cost;
    }
}
