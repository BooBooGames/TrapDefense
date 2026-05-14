using System;
using UnityEngine;

public class WeaponUpgradeController : MonoBehaviour
{
    [SerializeField][Min(1f)] private float damagePower = 1f;
    [SerializeField] private WeaponRotator[] speedTargets;
    [SerializeField] private GameObject[] levelVisuals = new GameObject[9];
    [SerializeField] private WeaponUpgradeLevel[] upgradeLevels = new WeaponUpgradeLevel[9];
    [SerializeField][Min(1)] private int currentLevel = 1;
    [SerializeField] private ParticleSystem weaponUpgradeEffect;

    private float speedMultiplier = 1f;

    public event Action<WeaponUpgradeController> UpgradeStateChanged;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => upgradeLevels != null ? upgradeLevels.Length : 0;
    public float DamagePower => damagePower;


    private void Awake()
    {
        currentLevel = Mathf.Clamp(currentLevel, 1, Mathf.Max(1, MaxLevel));
        ApplyCurrentLevelState();
    }

    public bool CanUpgrade()
    {
        return upgradeLevels != null && currentLevel < upgradeLevels.Length;
    }

    public bool TryUpgrade(GameViewScreen gameViewScreen, int gearCost)
    {
        if (!CanUpgrade() || gameViewScreen == null)
        {
            return false;
        }

        int upgradeCost = Mathf.Max(0, gearCost);
        if (!gameViewScreen.TrySpendGears(upgradeCost))
        {
            return false;
        }

        currentLevel++;
        ApplyCurrentLevelState();
        PlayUpgradeEffect();
        return true;
    }

    public void ApplySpeedMultiplier(float multiplier)
    {
        speedMultiplier *= Mathf.Max(0f, multiplier);
        ApplyCurrentLevelState();
    }

    public static void ApplySpeedMultiplierToCurrentTraps(float multiplier)
    {
        WeaponUpgradeController[] upgradeControllers = FindObjectsByType<WeaponUpgradeController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < upgradeControllers.Length; i++)
        {
            upgradeControllers[i]?.ApplySpeedMultiplier(multiplier);
        }
    }

    private void PlayUpgradeEffect()
    {
        if (weaponUpgradeEffect == null)
        {
            return;
        }

        weaponUpgradeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        weaponUpgradeEffect.Play();
    }

    private void ApplyCurrentLevelState()
    {
        if (upgradeLevels == null || upgradeLevels.Length == 0)
        {
            return;
        }

        WeaponUpgradeLevel level = upgradeLevels[Mathf.Clamp(currentLevel - 1, 0, upgradeLevels.Length - 1)];

        damagePower = Mathf.Max(1f, level.weaponPower);

        if (speedTargets != null)
        {
            float effectiveWeaponSpeed = level.weaponSpeed * speedMultiplier;
            for (int i = 0; i < speedTargets.Length; i++)
            {
                if (speedTargets[i] != null)
                {
                    speedTargets[i].SetRotationSpeed(effectiveWeaponSpeed);
                    speedTargets[i].SetMovementSpeed(effectiveWeaponSpeed);
                }
            }
        }

        if (levelVisuals != null && levelVisuals.Length > 0)
        {
            int activeVisualIndex = Mathf.Clamp(currentLevel - 1, 0, levelVisuals.Length - 1);
            GameObject activeVisual = levelVisuals[activeVisualIndex];

            for (int i = 0; i < levelVisuals.Length; i++)
            {
                if (levelVisuals[i] != null && levelVisuals[i] != activeVisual)
                {
                    levelVisuals[i].SetActive(false);
                }
            }

            if (activeVisual != null)
            {
                activeVisual.SetActive(true);
            }
        }

        if (speedTargets != null)
        {
            for (int i = 0; i < speedTargets.Length; i++)
            {
                if (speedTargets[i] != null)
                {
                    speedTargets[i].RestartMotion(true);
                }
            }
        }

        UpgradeStateChanged?.Invoke(this);
    }
}

[Serializable]
public class WeaponUpgradeLevel
{
    [Min(1f)] public float weaponPower = 1f;
    [Min(0f)] public float weaponSpeed = 0f;
}
