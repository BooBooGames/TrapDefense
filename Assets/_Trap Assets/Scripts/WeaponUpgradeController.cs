using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeController : MonoBehaviour
{
    private static readonly List<WeaponUpgradeController> ActiveControllers = new List<WeaponUpgradeController>();
    private static bool gameplayAnimationsEnabled;

    [SerializeField][Min(1f)] private float damagePower = 1f;
    [SerializeField] private Animator[] speedTargets;
    [SerializeField] private GameObject[] levelVisuals = new GameObject[9];
    [SerializeField] private WeaponUpgradeLevel[] upgradeLevels = new WeaponUpgradeLevel[9];
    [SerializeField][Min(1)] private int currentLevel = 1;
    [SerializeField] private ParticleSystem weaponUpgradeEffect;

    private float speedMultiplier = 1f;

    public event Action<WeaponUpgradeController> UpgradeStateChanged;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => upgradeLevels.Length;
    public float DamagePower => damagePower;


    private void Awake()
    {
        currentLevel = Mathf.Clamp(currentLevel, 1, Mathf.Max(1, MaxLevel));
        ApplyCurrentLevelState();
        SetAnimatorsEnabled(false);
    }

    private void OnEnable()
    {
        if (!ActiveControllers.Contains(this))
        {
            ActiveControllers.Add(this);
        }

        SetAnimatorsEnabled(gameplayAnimationsEnabled);
    }

    private void OnDisable()
    {
        ActiveControllers.Remove(this);
        SetAnimatorsEnabled(false);
    }

    public bool CanUpgrade()
    {
        return currentLevel < upgradeLevels.Length;
    }

    public bool TryUpgrade(GameViewScreen gameViewScreen, int gearCost)
    {
        if (!CanUpgrade())
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

    public void ResetSessionState()
    {
        currentLevel = 1;
        speedMultiplier = 1f;
        ApplyCurrentLevelState();
        SetAnimatorsEnabled(gameplayAnimationsEnabled);
    }

    public static void ApplySpeedMultiplierToCurrentTraps(float multiplier)
    {
        WeaponUpgradeController[] upgradeControllers = FindObjectsByType<WeaponUpgradeController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < upgradeControllers.Length; i++)
        {
            upgradeControllers[i].ApplySpeedMultiplier(multiplier);
        }
    }

    public static void SetGameplayAnimationsEnabled(bool isEnabled)
    {
        gameplayAnimationsEnabled = isEnabled;

        for (int i = 0; i < ActiveControllers.Count; i++)
        {
            ActiveControllers[i].SetAnimatorsEnabled(isEnabled);
        }
    }

    private void PlayUpgradeEffect()
    {
        weaponUpgradeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        weaponUpgradeEffect.Play();
    }

    private void ApplyCurrentLevelState()
    {
        WeaponUpgradeLevel level = upgradeLevels[Mathf.Clamp(currentLevel - 1, 0, upgradeLevels.Length - 1)];

        damagePower = Mathf.Max(1f, level.weaponPower);

        float effectiveWeaponSpeed = level.weaponSpeed * speedMultiplier;
        for (int i = 0; i < speedTargets.Length; i++)
        {
            speedTargets[i].speed = effectiveWeaponSpeed;
        }

        int activeVisualIndex = Mathf.Clamp(currentLevel - 1, 0, levelVisuals.Length - 1);
        GameObject activeVisual = levelVisuals[activeVisualIndex];

        for (int i = 0; i < levelVisuals.Length; i++)
        {
            if (levelVisuals[i] != activeVisual)
            {
                levelVisuals[i].SetActive(false);
            }
        }

        activeVisual.SetActive(true);

        UpgradeStateChanged?.Invoke(this);
    }

    private void SetAnimatorsEnabled(bool isEnabled)
    {
        for (int i = 0; i < speedTargets.Length; i++)
        {
            speedTargets[i].enabled = isEnabled;
        }
    }
}

[Serializable]
public class WeaponUpgradeLevel
{
    public float weaponPower = 1f;
    public float weaponSpeed = 0f;
}
