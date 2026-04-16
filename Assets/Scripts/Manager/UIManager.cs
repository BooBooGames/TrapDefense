using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Image waveProgressBarFill;
    public TextMeshProUGUI waveProgressBarLabel;
    public Image gearCounterFill;
    public TextMeshProUGUI gearCounterLabel;

    [SerializeField][Min(0.1f)] private float gearGenerationDuration = 5f;

    public TextMeshProUGUI coinCounterLabel, gemsCounterLabel;

    public Button weaponUpgradeButton;
    public TextMeshProUGUI weaponUpgradeLevelLabel;
    public WeaponUpgradeController weaponUpgradeTarget;

    private int coinCount;
    private int gemsCount;
    private int gearCount;
    private float gearGenerationTimer;

    private void Awake()
    {
        Instance = this;
        coinCount = ParseLabelValue(coinCounterLabel);
        gemsCount = ParseLabelValue(gemsCounterLabel);
        gearCount = ParseInitialGearCount();
        UpdateGearUi(0f);
        UpdateCurrencyUi();
        BindWeaponUpgradeUi();
        RefreshWeaponUpgradeUi();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (weaponUpgradeTarget != null)
        {
            weaponUpgradeTarget.UpgradeStateChanged -= HandleWeaponUpgradeStateChanged;
        }
    }

    private void Update()
    {
        gearGenerationTimer += Time.deltaTime;

        while (gearGenerationTimer >= gearGenerationDuration)
        {
            gearGenerationTimer -= gearGenerationDuration;
            gearCount++;
        }

        float progress = gearGenerationDuration > 0f ? gearGenerationTimer / gearGenerationDuration : 0f;
        UpdateGearUi(progress);
    }

    public void UpdateWaveProgress(int currentWave, int totalWaves, float overallProgress)
    {
        if (waveProgressBarFill != null)
        {
            waveProgressBarFill.fillAmount = Mathf.Clamp01(overallProgress);
        }

        if (waveProgressBarLabel != null)
        {
            int displayedWave = totalWaves > 0 ? Mathf.Clamp(Mathf.Max(currentWave, 1), 1, totalWaves) : 0;
            waveProgressBarLabel.text = totalWaves > 0 ? $"Wave {displayedWave}/{totalWaves}" : "Wave 0/0";
        }
    }

    public void AddCoins(int amount)
    {
        coinCount = Mathf.Max(0, coinCount + amount);
        UpdateCurrencyUi();
    }

    public void AddGems(int amount)
    {
        gemsCount = Mathf.Max(0, gemsCount + amount);
        UpdateCurrencyUi();
    }

    public bool TrySpendGears(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (gearCount < amount)
        {
            return false;
        }

        gearCount -= amount;
        UpdateGearUi(gearGenerationDuration > 0f ? gearGenerationTimer / gearGenerationDuration : 0f);
        return true;
    }

    private void UpdateGearUi(float progress)
    {
        if (gearCounterFill != null)
        {
            gearCounterFill.fillAmount = Mathf.Clamp01(progress);
        }

        if (gearCounterLabel != null)
        {
            gearCounterLabel.text = gearCount.ToString();
        }

        RefreshWeaponUpgradeUi();
    }

    private int ParseInitialGearCount()
    {
        if (gearCounterLabel == null)
        {
            return 0;
        }

        return int.TryParse(gearCounterLabel.text, out int parsedCount) ? Mathf.Max(0, parsedCount) : 0;
    }

    private void UpdateCurrencyUi()
    {
        if (coinCounterLabel != null)
        {
            coinCounterLabel.text = coinCount.ToString();
        }

        if (gemsCounterLabel != null)
        {
            gemsCounterLabel.text = gemsCount.ToString();
        }
    }

    private int ParseLabelValue(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return 0;
        }

        return int.TryParse(label.text, out int parsedValue) ? Mathf.Max(0, parsedValue) : 0;
    }

    private void BindWeaponUpgradeUi()
    {
        if (weaponUpgradeButton != null)
        {
            weaponUpgradeButton.onClick.RemoveListener(HandleWeaponUpgradeClicked);
            weaponUpgradeButton.onClick.AddListener(HandleWeaponUpgradeClicked);
        }

        if (weaponUpgradeTarget != null)
        {
            weaponUpgradeTarget.UpgradeStateChanged -= HandleWeaponUpgradeStateChanged;
            weaponUpgradeTarget.UpgradeStateChanged += HandleWeaponUpgradeStateChanged;
        }
    }

    private void HandleWeaponUpgradeClicked()
    {
        if (weaponUpgradeTarget == null)
        {
            return;
        }

        weaponUpgradeTarget.TryUpgrade(this);
        RefreshWeaponUpgradeUi();
    }

    private void HandleWeaponUpgradeStateChanged(WeaponUpgradeController _)
    {
        RefreshWeaponUpgradeUi();
    }

    private void RefreshWeaponUpgradeUi()
    {
        if (weaponUpgradeLevelLabel != null)
        {
            if (weaponUpgradeTarget == null)
            {
                weaponUpgradeLevelLabel.text = "Weapon Lv. 0/0";
            }
            else
            {
                weaponUpgradeLevelLabel.text = $"Weapon Lv. {weaponUpgradeTarget.CurrentLevel}/{weaponUpgradeTarget.MaxLevel}";
            }
        }

        if (weaponUpgradeButton != null)
        {
            weaponUpgradeButton.interactable =
                weaponUpgradeTarget != null &&
                weaponUpgradeTarget.CanUpgrade() &&
                gearCount >= weaponUpgradeTarget.CurrentUpgradeCost;
        }
    }
}
