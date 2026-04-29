using System;
using UnityEngine;

public class WeaponUpgradeController : MonoBehaviour
{
    [SerializeField] private WeaponDamageSource[] damageSources;
    [SerializeField] private WeaponRotator[] speedTargets;
    [SerializeField] private GameObject[] levelVisuals = new GameObject[9];
    [SerializeField] private WeaponUpgradeLevel[] upgradeLevels = new WeaponUpgradeLevel[9];
    [SerializeField][Min(1)] private int currentLevel = 1;

    public event Action<WeaponUpgradeController> UpgradeStateChanged;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => upgradeLevels != null ? upgradeLevels.Length : 0;

    public int CurrentUpgradeCost
    {
        get
        {
            if (!CanUpgrade())
            {
                return 0;
            }

            return upgradeLevels[currentLevel].gearCost;
        }
    }

    private void Awake()
    {
        currentLevel = Mathf.Clamp(currentLevel, 1, Mathf.Max(1, MaxLevel));
        ApplyCurrentLevelState();
    }

    public bool CanUpgrade()
    {
        return upgradeLevels != null && currentLevel < upgradeLevels.Length;
    }

    public bool TryUpgrade(GameViewScreen gameViewScreen)
    {
        if (!CanUpgrade() || gameViewScreen == null)
        {
            return false;
        }

        int upgradeCost = upgradeLevels[currentLevel].gearCost;
        if (!gameViewScreen.TrySpendGears(upgradeCost))
        {
            return false;
        }

        currentLevel++;
        ApplyCurrentLevelState();
        return true;
    }

    private void ApplyCurrentLevelState()
    {
        if (upgradeLevels == null || upgradeLevels.Length == 0)
        {
            return;
        }

        WeaponUpgradeLevel level = upgradeLevels[Mathf.Clamp(currentLevel - 1, 0, upgradeLevels.Length - 1)];

        if (damageSources != null)
        {
            for (int i = 0; i < damageSources.Length; i++)
            {
                if (damageSources[i] != null)
                {
                    damageSources[i].SetDamagePower(level.weaponPower);
                }
            }
        }

        if (speedTargets != null)
        {
            for (int i = 0; i < speedTargets.Length; i++)
            {
                if (speedTargets[i] != null)
                {
                    speedTargets[i].SetRotationSpeed(level.weaponSpeed);
                }
            }
        }

        if (levelVisuals != null && levelVisuals.Length > 0)
        {
            for (int i = 0; i < levelVisuals.Length; i++)
            {
                levelVisuals[i].SetActive(false);
                if (levelVisuals[i] != null)
                {
                    Debug.Log($"Setting level visual {i} active: {i == currentLevel - 1}");
                }
                levelVisuals[currentLevel - 1].SetActive(true);
            }
        }

        UpgradeStateChanged?.Invoke(this);
    }
}

[Serializable]
public class WeaponUpgradeLevel
{
    [Min(0)] public int gearCost;
    [Min(1f)] public float weaponPower = 1f;
    [Min(0f)] public float weaponSpeed = 0f;
}
