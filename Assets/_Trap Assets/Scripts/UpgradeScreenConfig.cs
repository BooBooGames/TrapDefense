using System;
using UnityEngine;

[CreateAssetMenu(menuName = "TrapDefense/Upgrades/Upgrade Screen Config", fileName = "UpgradeScreenConfig")]
public class UpgradeScreenConfig : ScriptableObject
{
    private const string DefaultResourcePath = "DefaultUpgradeScreenConfig";

    private static UpgradeScreenConfig runtimeFallback;

    [SerializeField]
    private WeaponUnlockDefinition[] weapons =
    {
        new WeaponUnlockDefinition
        {
            weaponName = "Weapon 1",
            unlockedByDefault = true,
        },
        new WeaponUnlockDefinition
        {
            weaponName = "Weapon 2",
            unlockCost = new UpgradeResourceCost { coins = 50, gears = 0 },
        },
        new WeaponUnlockDefinition
        {
            weaponName = "Weapon 3",
            unlockCost = new UpgradeResourceCost { coins = 100, gears = 0 },
        },
    };

    [SerializeField] private GearFlowUpgradeDefinition gearFlow = new GearFlowUpgradeDefinition();
    [SerializeField] private BaseHealthUpgradeDefinition baseHealth = new BaseHealthUpgradeDefinition();

    public WeaponUnlockDefinition[] Weapons => weapons;
    public GearFlowUpgradeDefinition GearFlow => gearFlow;
    public BaseHealthUpgradeDefinition BaseHealth => baseHealth;
    public int WeaponCount => weapons != null ? weapons.Length : 0;

    public WeaponUnlockDefinition GetWeapon(int weaponIndex)
    {
        if (weapons == null || weaponIndex < 0 || weaponIndex >= weapons.Length)
        {
            return null;
        }

        return weapons[weaponIndex];
    }

    public static UpgradeScreenConfig Resolve(UpgradeScreenConfig explicitConfig)
    {
        if (explicitConfig != null)
        {
            return explicitConfig;
        }

        UpgradeScreenConfig resourceConfig = Resources.Load<UpgradeScreenConfig>(DefaultResourcePath);
        if (resourceConfig != null)
        {
            return resourceConfig;
        }

        if (runtimeFallback == null)
        {
            runtimeFallback = CreateInstance<UpgradeScreenConfig>();
            runtimeFallback.hideFlags = HideFlags.DontSave;
            runtimeFallback.EnsureDefaults();
        }

        return runtimeFallback;
    }

    private void OnValidate()
    {
        EnsureDefaults();
    }

    private void EnsureDefaults()
    {
        if (weapons == null || weapons.Length < 3)
        {
            WeaponUnlockDefinition[] resized = new WeaponUnlockDefinition[Mathf.Max(3, weapons != null ? weapons.Length : 0)];
            for (int i = 0; i < resized.Length; i++)
            {
                if (weapons != null && i < weapons.Length && weapons[i] != null)
                {
                    resized[i] = weapons[i];
                }
                else
                {
                    resized[i] = CreateDefaultWeapon(i);
                }
            }

            weapons = resized;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                weapons[i] = CreateDefaultWeapon(i);
            }

            if (string.IsNullOrWhiteSpace(weapons[i].weaponName))
            {
                weapons[i].weaponName = $"Weapon {i + 1}";
            }
        }

        weapons[0].unlockedByDefault = true;

        gearFlow ??= new GearFlowUpgradeDefinition();
        baseHealth ??= new BaseHealthUpgradeDefinition();
    }

    private static WeaponUnlockDefinition CreateDefaultWeapon(int weaponIndex)
    {
        return new WeaponUnlockDefinition
        {
            weaponName = $"Weapon {weaponIndex + 1}",
            unlockedByDefault = weaponIndex == 0,
            unlockCost = new UpgradeResourceCost
            {
                coins = weaponIndex == 1 ? 50 : weaponIndex == 2 ? 100 : 0,
                gears = 0,
            },
        };
    }
}

[Serializable]
public class WeaponUnlockDefinition
{
    public string weaponName = "Weapon";
    public Sprite weaponSprite;
    public UpgradeResourceCost unlockCost;
    public bool unlockedByDefault;
}

[Serializable]
public struct UpgradeResourceCost
{
    [Min(0)] public int coins;
    [Min(0)] public int gears;

    public bool IsFree => coins <= 0 && gears <= 0;

    public string ToDisplayString()
    {
        if (coins > 0 && gears > 0)
        {
            return $"{coins} Coins + {gears} Gears";
        }

        if (coins > 0)
        {
            return $"{coins} Coins";
        }

        if (gears > 0)
        {
            return $"{gears} Gears";
        }

        return "Free";
    }
}

[Serializable]
public class GearFlowUpgradeDefinition
{
    [Min(0.1f)] public float defaultValueSeconds = 2.2f;
    [Range(0.01f, 1f)] public float valueMultiplier = 0.91f;
    [Min(1)] public int costPerLevel = 10;
    [Min(1)] public int maxLevel = 9;

    public float EvaluateValue(int level)
    {
        int clampedLevel = Mathf.Max(0, level);
        return Mathf.Max(0.1f, defaultValueSeconds * Mathf.Pow(valueMultiplier, clampedLevel));
    }

    public UpgradeResourceCost EvaluateUpgradeCost(int nextLevel)
    {
        return new UpgradeResourceCost
        {
            coins = costPerLevel * Mathf.Max(1, nextLevel),
            gears = 0,
        };
    }
}

[Serializable]
public class BaseHealthUpgradeDefinition
{
    [Min(1)] public int defaultValue = 20;
    [Min(0)] public int levelBaseOffset = 6;
    [Min(1)] public int levelValueMultiplier = 4;
    [Min(0)] public int costBaseOffset = 1;
    [Min(1)] public int costMultiplier = 5;
    [Min(1)] public int maxLevel = 9;

    public int EvaluateValue(int level)
    {
        int clampedLevel = Mathf.Max(0, level);
        return clampedLevel <= 0 ? defaultValue : (levelBaseOffset + clampedLevel) * levelValueMultiplier;
    }

    public UpgradeResourceCost EvaluateUpgradeCost(int nextLevel)
    {
        return new UpgradeResourceCost
        {
            coins = (costBaseOffset + Mathf.Max(1, nextLevel)) * costMultiplier,
            gears = 0,
        };
    }
}
