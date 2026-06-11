using System;
using System.Collections.Generic;
using UnityEngine;

public static class PowerCardUpgradeSystem
{
    private const int DefaultLevel = 1;
    public const int CopiesRequiredForUpgrade = 4;

    private const string TrapAcceleratorId = "1";
    private const string TrapAcceleratorName = "Trap Accelerator";
    private const string PocketGearsId = "2";
    private const string PocketGearsName = "Pocket Gears";
    private const string MinorHealId = "3";
    private const string MinorHealName = "Minor Heal";
    private const string AngelBlessingId = "4";
    private const string AngelBlessingName = "Angel Blessing";
    private const string ThinArmorId = "5";
    private const string ThinArmorName = "Thin Armor";
    private const string WaveBonusId = "6";
    private const string WaveBonusName = "Wave Bonus";
    private const string WeakeningStrikeId = "7";
    private const string WeakeningStrikeName = "Weakening Strike";
    private const string InvulnerabilityPulseId = "8";
    private const string InvulnerabilityPulseName = "Invulnerability Pulse";
    private const string DeathMarkId = "9";
    private const string DeathMarkName = "Death Mark";
    private const string DoomTrapsId = "10";
    private const string DoomTrapsName = "Doom Traps";
    private const string SecondWindId = "11";
    private const string SecondWindName = "Second Wind";
    private const string SharpTrapsId = "12";
    private const string SharpTrapsName = "Sharp Traps";
    private const string ScrapCollectorId = "13";
    private const string ScrapCollectorName = "Scrap Collector";
    private const string ResourceMasteryId = "14";
    private const string ResourceMasteryName = "Resource Mastery";
    private const string ToughBaseId = "15";
    private const string ToughBaseName = "Tough Base";

    public static event Action<PowerCardDefinition, int> CardLevelChanged;
    public static event Action<PowerCardDefinition, int> CardCopiesChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        CardLevelChanged = null;
        CardCopiesChanged = null;
    }

    public static int GetCardLevel(PowerCardDefinition cardData)
    {
        return cardData == null ? DefaultLevel : GetCardLevel(cardData.cardId, cardData.cardName);
    }

    public static bool TryUpgradeCard(PowerCardDefinition cardData, out int newLevel)
    {
        newLevel = GetCardLevel(cardData);
        if (cardData == null)
        {
            return false;
        }

        string saveKey = GetSaveKey(cardData.cardId, cardData.cardName);
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return false;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        List<PowerCardLevelSaveData> savedLevels = new List<PowerCardLevelSaveData>(saveData.powerCardLevels ?? Array.Empty<PowerCardLevelSaveData>());
        List<PowerCardCopySaveData> savedCopies = new List<PowerCardCopySaveData>(saveData.powerCardCopies ?? Array.Empty<PowerCardCopySaveData>());
        PowerCardLevelSaveData levelData = null;
        PowerCardCopySaveData copyData = null;

        for (int i = 0; i < savedLevels.Count; i++)
        {
            if (savedLevels[i] != null && string.Equals(savedLevels[i].cardId, saveKey, StringComparison.OrdinalIgnoreCase))
            {
                levelData = savedLevels[i];
                break;
            }
        }

        for (int i = 0; i < savedCopies.Count; i++)
        {
            if (savedCopies[i] != null && string.Equals(savedCopies[i].cardId, saveKey, StringComparison.OrdinalIgnoreCase))
            {
                copyData = savedCopies[i];
                break;
            }
        }

        if (copyData == null || copyData.count < CopiesRequiredForUpgrade)
        {
            return false;
        }

        if (levelData == null)
        {
            levelData = new PowerCardLevelSaveData { cardId = saveKey, level = DefaultLevel };
            savedLevels.Add(levelData);
        }

        copyData.count = Mathf.Max(0, copyData.count - CopiesRequiredForUpgrade);
        levelData.level = Mathf.Max(DefaultLevel, levelData.level) + 1;
        newLevel = levelData.level;
        saveData.powerCardLevels = savedLevels.ToArray();
        saveData.powerCardCopies = savedCopies.ToArray();
        GameSaveSystem.Save(saveData);

        CardLevelChanged?.Invoke(cardData, levelData.level);
        CardCopiesChanged?.Invoke(cardData, copyData.count);
        return true;
    }

    public static int AddCardCopies(PowerCardDefinition cardData, int amount)
    {
        if (cardData == null || amount <= 0)
        {
            return GetCardCopyCount(cardData);
        }

        string saveKey = GetSaveKey(cardData.cardId, cardData.cardName);
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return 0;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        List<PowerCardCopySaveData> savedCopies = new List<PowerCardCopySaveData>(saveData.powerCardCopies ?? Array.Empty<PowerCardCopySaveData>());
        PowerCardCopySaveData copyData = null;

        for (int i = 0; i < savedCopies.Count; i++)
        {
            if (savedCopies[i] != null && string.Equals(savedCopies[i].cardId, saveKey, StringComparison.OrdinalIgnoreCase))
            {
                copyData = savedCopies[i];
                break;
            }
        }

        if (copyData == null)
        {
            copyData = new PowerCardCopySaveData { cardId = saveKey };
            savedCopies.Add(copyData);
        }

        copyData.count = Mathf.Max(0, copyData.count + amount);
        saveData.powerCardCopies = savedCopies.ToArray();
        GameSaveSystem.Save(saveData);

        CardCopiesChanged?.Invoke(cardData, copyData.count);
        return copyData.count;
    }

    public static int GetCardCopyCount(PowerCardDefinition cardData)
    {
        return cardData == null ? 0 : GetCardCopyCount(cardData.cardId, cardData.cardName);
    }

    public static bool CanUpgradeCard(PowerCardDefinition cardData)
    {
        return GetCardCopyCount(cardData) >= CopiesRequiredForUpgrade;
    }

    public static float GetCardCopyFillAmount(PowerCardDefinition cardData)
    {
        return Mathf.Clamp01(GetCardCopyCount(cardData) / (float)CopiesRequiredForUpgrade);
    }

    public static string GetCardCopyProgressText(PowerCardDefinition cardData)
    {
        int displayCount = Mathf.Min(GetCardCopyCount(cardData), CopiesRequiredForUpgrade);
        return $"{displayCount}/{CopiesRequiredForUpgrade}";
    }

    public static string[] GetCurrentDescriptions(PowerCardDefinition cardData)
    {
        if (cardData == null)
        {
            return Array.Empty<string>();
        }

        return GetDescriptionsForLevel(cardData, GetCardLevel(cardData), false);
    }

    public static string[] GetNextLevelDescriptions(PowerCardDefinition cardData)
    {
        if (cardData == null)
        {
            return Array.Empty<string>();
        }

        return GetDescriptionsForLevel(cardData, GetCardLevel(cardData) + 1, true);
    }

    public static float GetTrapAcceleratorSpeedMultiplier(PowerCardDefinition cardData)
    {
        return 1f + GetPercentValue(8f, 1f, GetCardLevel(cardData)) / 100f;
    }

    public static int GetPocketGearsReward(PowerCardDefinition cardData)
    {
        return GetIntValue(3, 2, GetCardLevel(cardData));
    }

    public static int GetMinorHealAmount(PowerCardDefinition cardData)
    {
        return GetIntValue(5, 2, GetCardLevel(cardData));
    }

    public static int GetAngelBlessingHealAmount()
    {
        return GetIntValue(5, 2, GetKnownCardLevel(AngelBlessingId, AngelBlessingName));
    }

    public static float GetThinArmorEnemyHealthMultiplier()
    {
        float reductionPercent = GetPercentValue(10f, 2f, GetKnownCardLevel(ThinArmorId, ThinArmorName));
        return Mathf.Max(0f, 1f - reductionPercent / 100f);
    }

    public static int GetWaveBonusGears()
    {
        return GetIntValue(5, 1, GetKnownCardLevel(WaveBonusId, WaveBonusName));
    }

    public static float GetWeakeningStrikeSlowMultiplier()
    {
        return 0.9f;
    }

    public static float GetWeakeningStrikeSlowDuration()
    {
        return GetFloatValue(2f, 1f, GetKnownCardLevel(WeakeningStrikeId, WeakeningStrikeName));
    }

    public static float GetInvulnerabilityPulseDuration()
    {
        return GetFloatValue(5f, 2f, GetKnownCardLevel(InvulnerabilityPulseId, InvulnerabilityPulseName));
    }

    public static float GetInvulnerabilityPulseCooldown()
    {
        return 30f;
    }

    public static float GetDeathMarkInstantKillChance()
    {
        return GetPercentValue(5f, 1f, GetKnownCardLevel(DeathMarkId, DeathMarkName)) / 100f;
    }

    public static float GetDoomTrapsSlowMultiplierPerHit()
    {
        float slowPercent = GetPercentValue(5f, 1f, GetKnownCardLevel(DoomTrapsId, DoomTrapsName));
        return Mathf.Max(0f, 1f - slowPercent / 100f);
    }

    public static float GetDoomTrapsTrapDamageMultiplier()
    {
        return 1.2f;
    }

    public static int GetSecondWindHealAmount()
    {
        return GetIntValue(10, 2, GetKnownCardLevel(SecondWindId, SecondWindName));
    }

    public static float GetSharpTrapsDamageMultiplier()
    {
        return 1f + GetPercentValue(8f, 1f, GetKnownCardLevel(SharpTrapsId, SharpTrapsName)) / 100f;
    }

    public static float GetScrapCollectorGearChance()
    {
        return GetPercentValue(5f, 1f, GetKnownCardLevel(ScrapCollectorId, ScrapCollectorName)) / 100f;
    }

    public static float GetResourceMasteryGearGenerationDurationMultiplier()
    {
        return 1f / 1.25f;
    }

    public static int ApplyResourceMasteryUpgradeCostReduction(int baseCost)
    {
        if (baseCost <= 0)
        {
            return Mathf.Max(0, baseCost);
        }

        float reductionPercent = GetPercentValue(10f, 1f, GetKnownCardLevel(ResourceMasteryId, ResourceMasteryName));
        return Mathf.Max(1, Mathf.FloorToInt(baseCost * Mathf.Max(0f, 1f - reductionPercent / 100f)));
    }

    public static int GetToughBaseHealthBonus(PowerCardDefinition cardData)
    {
        return GetIntValue(5, 2, GetCardLevel(cardData));
    }

    private static string[] GetDescriptionsForLevel(PowerCardDefinition cardData, int level, bool isNextLevel)
    {
        PowerCardUpgradeKind kind = GetUpgradeKind(cardData.cardId, cardData.cardName);
        if (kind == PowerCardUpgradeKind.None)
        {
            return cardData.GetDescriptions();
        }

        string prefix = isNextLevel ? $"Level {level}: " : string.Empty;

        switch (kind)
        {
            case PowerCardUpgradeKind.TrapAccelerator:
                return new[] { $"{prefix}+{FormatPercent(GetPercentValue(8f, 1f, level))}% trap attack speed" };
            case PowerCardUpgradeKind.PocketGears:
                return new[] { $"{prefix}Instantly gain +{GetIntValue(3, 2, level)} gears" };
            case PowerCardUpgradeKind.MinorHeal:
                return new[] { $"{prefix}Instantly heal {GetIntValue(5, 2, level)} HP" };
            case PowerCardUpgradeKind.AngelBlessing:
                return new[] { $"{prefix}+{GetIntValue(5, 2, level)} HP at end of each wave" };
            case PowerCardUpgradeKind.ThinArmor:
                return new[] { $"{prefix}-{FormatPercent(GetPercentValue(10f, 2f, level))}% enemy HP" };
            case PowerCardUpgradeKind.WaveBonus:
                return new[] { $"{prefix}+{GetIntValue(5, 1, level)} gears on wave complete" };
            case PowerCardUpgradeKind.WeakeningStrike:
                return new[] { $"{prefix}Trap hits slow enemy -10% for {FormatNumber(GetFloatValue(2f, 1f, level))}s" };
            case PowerCardUpgradeKind.InvulnerabilityPulse:
                return new[] { $"{prefix}Every 30s enemies deal no damage for {FormatNumber(GetFloatValue(5f, 2f, level))}s" };
            case PowerCardUpgradeKind.DeathMark:
                return new[] { $"{prefix}{FormatPercent(GetPercentValue(5f, 1f, level))}% chance to instantly kill enemies" };
            case PowerCardUpgradeKind.DoomTraps:
                return new[]
                {
                    $"{prefix}Each hit slows enemy speed by -{FormatPercent(GetPercentValue(5f, 1f, level))}%",
                    "+20% trap damage"
                };
            case PowerCardUpgradeKind.SecondWind:
                return new[] { $"{prefix}At 1 HP base recovers {GetIntValue(10, 2, level)} HP once" };
            case PowerCardUpgradeKind.SharpTraps:
                return new[] { $"{prefix}+{FormatPercent(GetPercentValue(8f, 1f, level))}% trap damage" };
            case PowerCardUpgradeKind.ScrapCollector:
                return new[] { $"{prefix}{FormatPercent(GetPercentValue(5f, 1f, level))}% chance to drop +1 gear on enemy kill" };
            case PowerCardUpgradeKind.ResourceMastery:
                return new[]
                {
                    $"{prefix}+25% gear production",
                    $"-{FormatPercent(GetPercentValue(10f, 1f, level))}% upgrade costs"
                };
            case PowerCardUpgradeKind.ToughBase:
                return new[] { $"{prefix}+{GetIntValue(5, 2, level)} base HP" };
            default:
                return cardData.GetDescriptions();
        }
    }

    private static int GetCardLevel(string cardId, string cardName)
    {
        string saveKey = GetSaveKey(cardId, cardName);
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return DefaultLevel;
        }

        return FindSavedLevel(saveKey);
    }

    private static int GetCardCopyCount(string cardId, string cardName)
    {
        string saveKey = GetSaveKey(cardId, cardName);
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return 0;
        }

        return FindSavedCopyCount(saveKey);
    }

    private static int GetKnownCardLevel(string cardId, string cardName)
    {
        int idLevel = string.IsNullOrWhiteSpace(cardId) ? DefaultLevel : FindSavedLevel(cardId.Trim());
        int nameLevel = string.IsNullOrWhiteSpace(cardName) ? DefaultLevel : FindSavedLevel(NormalizeKey(cardName));
        return Mathf.Max(idLevel, nameLevel);
    }

    private static int FindSavedLevel(string saveKey)
    {
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return DefaultLevel;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        PowerCardLevelSaveData[] savedLevels = saveData.powerCardLevels ?? Array.Empty<PowerCardLevelSaveData>();
        for (int i = 0; i < savedLevels.Length; i++)
        {
            PowerCardLevelSaveData levelData = savedLevels[i];
            if (levelData != null && string.Equals(levelData.cardId, saveKey, StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(DefaultLevel, levelData.level);
            }
        }

        return DefaultLevel;
    }

    private static int FindSavedCopyCount(string saveKey)
    {
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return 0;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        PowerCardCopySaveData[] savedCopies = saveData.powerCardCopies ?? Array.Empty<PowerCardCopySaveData>();
        for (int i = 0; i < savedCopies.Length; i++)
        {
            PowerCardCopySaveData copyData = savedCopies[i];
            if (copyData != null && string.Equals(copyData.cardId, saveKey, StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(0, copyData.count);
            }
        }

        return 0;
    }

    private static string GetSaveKey(string cardId, string cardName)
    {
        if (!string.IsNullOrWhiteSpace(cardId))
        {
            return cardId.Trim();
        }

        return NormalizeKey(cardName);
    }

    private static PowerCardUpgradeKind GetUpgradeKind(string cardId, string cardName)
    {
        string normalizedId = NormalizeKey(cardId);
        string normalizedName = NormalizeKey(cardName);

        if (normalizedId == TrapAcceleratorId || normalizedName == NormalizeKey(TrapAcceleratorName)) return PowerCardUpgradeKind.TrapAccelerator;
        if (normalizedId == PocketGearsId || normalizedName == NormalizeKey(PocketGearsName)) return PowerCardUpgradeKind.PocketGears;
        if (normalizedId == MinorHealId || normalizedName == NormalizeKey(MinorHealName)) return PowerCardUpgradeKind.MinorHeal;
        if (normalizedId == AngelBlessingId || normalizedName == NormalizeKey(AngelBlessingName)) return PowerCardUpgradeKind.AngelBlessing;
        if (normalizedId == ThinArmorId || normalizedName == NormalizeKey(ThinArmorName)) return PowerCardUpgradeKind.ThinArmor;
        if (normalizedId == WaveBonusId || normalizedName == NormalizeKey(WaveBonusName)) return PowerCardUpgradeKind.WaveBonus;
        if (normalizedId == WeakeningStrikeId || normalizedName == NormalizeKey(WeakeningStrikeName)) return PowerCardUpgradeKind.WeakeningStrike;
        if (normalizedId == InvulnerabilityPulseId || normalizedName == NormalizeKey(InvulnerabilityPulseName)) return PowerCardUpgradeKind.InvulnerabilityPulse;
        if (normalizedId == DeathMarkId || normalizedName == NormalizeKey(DeathMarkName)) return PowerCardUpgradeKind.DeathMark;
        if (normalizedId == DoomTrapsId || normalizedName == NormalizeKey(DoomTrapsName)) return PowerCardUpgradeKind.DoomTraps;
        if (normalizedId == SecondWindId || normalizedName == NormalizeKey(SecondWindName)) return PowerCardUpgradeKind.SecondWind;
        if (normalizedId == SharpTrapsId || normalizedName == NormalizeKey(SharpTrapsName)) return PowerCardUpgradeKind.SharpTraps;
        if (normalizedId == ScrapCollectorId || normalizedName == NormalizeKey(ScrapCollectorName)) return PowerCardUpgradeKind.ScrapCollector;
        if (normalizedId == ResourceMasteryId || normalizedName == NormalizeKey(ResourceMasteryName)) return PowerCardUpgradeKind.ResourceMastery;
        if (normalizedId == ToughBaseId || normalizedName == NormalizeKey(ToughBaseName)) return PowerCardUpgradeKind.ToughBase;

        return PowerCardUpgradeKind.None;
    }

    private static int GetIntValue(int baseValue, int increasePerLevel, int level)
    {
        return baseValue + Mathf.Max(0, level - DefaultLevel) * increasePerLevel;
    }

    private static float GetFloatValue(float baseValue, float increasePerLevel, int level)
    {
        return baseValue + Mathf.Max(0, level - DefaultLevel) * increasePerLevel;
    }

    private static float GetPercentValue(float basePercent, float increasePerLevel, int level)
    {
        return basePercent + Mathf.Max(0, level - DefaultLevel) * increasePerLevel;
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty);
    }

    private static string FormatPercent(float value)
    {
        return FormatNumber(value);
    }

    private static string FormatNumber(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.#");
    }
}

public enum PowerCardUpgradeKind
{
    None,
    TrapAccelerator,
    PocketGears,
    MinorHeal,
    AngelBlessing,
    ThinArmor,
    WaveBonus,
    WeakeningStrike,
    InvulnerabilityPulse,
    DeathMark,
    DoomTraps,
    SecondWind,
    SharpTraps,
    ScrapCollector,
    ResourceMastery,
    ToughBase
}
