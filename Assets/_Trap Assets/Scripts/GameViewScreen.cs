using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameViewScreen : MonoBehaviour
{
    public static GameViewScreen Instance { get; private set; }

    [SerializeField] private ZombieCrowdSpawner zombieCrowdSpawner;
    public TextMeshProUGUI inGameCoinLabel;
    [SerializeField] private Image waveProgressBarFill;
    [SerializeField] private TextMeshProUGUI waveProgressBarLabel;
    [SerializeField] private GameObject chest1TriggerImage, chest2TriggerImage;
    [SerializeField] private Image gearCounterFill;
    [SerializeField] private TextMeshProUGUI gearCounterLabel;
    [SerializeField][Min(0.1f)] private float gearGenerationDuration = 5f;

    [SerializeField] private Button defaultWeaponUpgradeButton;
    [SerializeField] private Image defaultWeaponIcon;
    [SerializeField] private TextMeshProUGUI defaultWeaponUpgradeLevelLabel, defaultRequiredGearsCostLabel;
    [SerializeField] private WeaponUpgradeController defaultWeaponUpgradeTarget;

    [SerializeField] private Button _1WeaponUpgradeButton;
    [SerializeField] private Image _1WeaponIcon;

    [SerializeField] private TextMeshProUGUI _1WeaponUpgradeLevelLabel, _1RequiredGearsCostLabel;
    [SerializeField] private WeaponUpgradeController _1WeaponUpgradeTarget;

    [SerializeField] private Button _2WeaponUpgradeButton;
    [SerializeField] private Image _2WeaponIcon;

    [SerializeField] private TextMeshProUGUI _2WeaponUpgradeLevelLabel, _2RequiredGearsCostLabel;
    [SerializeField] private WeaponUpgradeController _2WeaponUpgradeTarget;


    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject lifeIcon;
    [SerializeField] private TextMeshProUGUI healthBarLabel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private UpgradeScreenConfig upgradeConfig;
    [SerializeField][Min(0)] private int playerHealthUpgradeBonus;
    [SerializeField] private bool pauseOnGameOver = true;

    private int gearCount;
    private float gearGenerationTimer;
    private int maxPlayerHealth;
    private int currentPlayerHealth;
    private int inGameCoins;
    private bool gameOverTriggered;

    public int GearCount => gearCount;
    public int InGameCoins => inGameCoins;

    private void Awake()
    {
        Instance = this;
        ApplyPersistentUpgradeState();

        if (zombieCrowdSpawner != null)
        {
            zombieCrowdSpawner.SetGameViewScreen(this);
        }

        gearCount = ParseInitialGearCount();
        UpdateGearUi(GetGearProgress());
        RefreshInGameCoinLabel();
        RefreshWaveProgress();
        RefreshWeaponUpgradeUi();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ApplyPersistentUpgradeState();
        BindWeaponUpgradeUi();
        PlayerUpgradeSystem.UpgradeStateChanged -= HandlePlayerUpgradeStateChanged;
        PlayerUpgradeSystem.UpgradeStateChanged += HandlePlayerUpgradeStateChanged;

        BindWeaponUpgradeStateEvents(true);

        if (zombieCrowdSpawner != null)
        {
            zombieCrowdSpawner.ProgressChanged -= HandleWaveProgressChanged;
            zombieCrowdSpawner.ProgressChanged += HandleWaveProgressChanged;
        }

        RefreshWaveProgress();
        RefreshWeaponUpgradeUi();
    }

    private void OnDisable()
    {
        PlayerUpgradeSystem.UpgradeStateChanged -= HandlePlayerUpgradeStateChanged;
        BindWeaponUpgradeStateEvents(false);

        if (zombieCrowdSpawner != null)
        {
            zombieCrowdSpawner.ProgressChanged -= HandleWaveProgressChanged;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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

        UpdateGearUi(GetGearProgress());
    }

    public void StartGameplay()
    {
        ApplyPersistentUpgradeState();
        EndPointTrigger.ResetAllGates();
        ResetInGameCoins();
        RefreshWeaponUpgradeUi();

        if (!gameOverTriggered)
        {
            Time.timeScale = 1f;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        zombieCrowdSpawner?.StartWaves();
    }

    public void EndGameplay()
    {
        zombieCrowdSpawner?.StopWaves();
        gameOverTriggered = false;
        Time.timeScale = 1f;
        ResetInGameCoins();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        RefreshWaveProgress();
    }

    public void InitializePlayerHealth()
    {
        ApplyPersistentUpgradeState();
        maxPlayerHealth = Mathf.Max(1, PlayerUpgradeSystem.CurrentBaseHealthValue);
        currentPlayerHealth = maxPlayerHealth;
        gameOverTriggered = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pauseOnGameOver)
        {
            Time.timeScale = 1f;
        }

        UpdatePlayerHealthUi();
    }

    public void AddInGameCoins(int amount)
    {
        if (amount <= 0 || gameOverTriggered)
        {
            return;
        }

        inGameCoins = Mathf.Max(0, inGameCoins + amount);
        RefreshInGameCoinLabel();
    }

    public void HandleAllWavesCompleted()
    {
        if (gameOverTriggered)
        {
            return;
        }

        gameOverTriggered = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UIManager.Instance?.ShowWinPreview(inGameCoins);

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    public void AddPlayerHealthUpgrade(int amount)
    {
        playerHealthUpgradeBonus = Mathf.Max(0, playerHealthUpgradeBonus + amount);
        maxPlayerHealth = Mathf.Max(1, maxPlayerHealth + amount);
        currentPlayerHealth = Mathf.Min(maxPlayerHealth, currentPlayerHealth + amount);
        UpdatePlayerHealthUi();
    }

    public void DamagePlayer(int amount)
    {
        if (gameOverTriggered || amount <= 0)
        {
            return;
        }

        currentPlayerHealth = Mathf.Max(0, currentPlayerHealth - amount);
        UpdatePlayerHealthUi();

        if (currentPlayerHealth <= 0)
        {
            TriggerGameOver();
        }
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
        UpdateGearUi(GetGearProgress());
        return true;
    }

    public void RefreshSceneWeaponUnlockState()
    {
        ApplyPersistentUpgradeState();
        RefreshWeaponUpgradeUi();
    }

    private void BindWeaponUpgradeUi()
    {
        for (int i = 0; i < 3; i++)
        {
            Button button = GetWeaponUpgradeButton(i);
            if (button == null)
            {
                continue;
            }

            int weaponIndex = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleWeaponUpgradeClicked(weaponIndex));
        }
    }

    private void HandleWeaponUpgradeClicked(int weaponIndex)
    {
        WeaponUpgradeController target = GetWeaponUpgradeTarget(weaponIndex);
        if (target == null || !PlayerUpgradeSystem.IsWeaponUnlocked(weaponIndex))
        {
            return;
        }

        WeaponUnlockDefinition weaponDefinition = PlayerUpgradeSystem.Config.GetWeapon(weaponIndex);
        target.TryUpgrade(this, GetRequiredGearCost(target, weaponDefinition));
        RefreshWeaponUpgradeUi();
    }

    private void HandleWeaponUpgradeStateChanged(WeaponUpgradeController _)
    {
        RefreshWeaponUpgradeUi();
    }

    private void HandlePlayerUpgradeStateChanged()
    {
        ApplyPersistentUpgradeState();
        RefreshWeaponUpgradeUi();
    }

    private void HandleWaveProgressChanged(int currentWave, int totalWaves, float overallProgress)
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

    private void RefreshWaveProgress()
    {
        if (zombieCrowdSpawner == null)
        {
            HandleWaveProgressChanged(0, 0, 0f);
            return;
        }

        HandleWaveProgressChanged(
            zombieCrowdSpawner.CurrentWaveNumber,
            zombieCrowdSpawner.TotalWaves,
            zombieCrowdSpawner.OverallProgress);
    }

    private void ApplyPersistentUpgradeState()
    {
        PlayerUpgradeSystem.Initialize(ResolveUpgradeConfig());
        gearGenerationDuration = Mathf.Max(0.1f, PlayerUpgradeSystem.CurrentGearFlowValue);
        playerHealthUpgradeBonus = Mathf.Max(0, PlayerUpgradeSystem.CurrentBaseHealthBonus);
        gearGenerationTimer = Mathf.Clamp(gearGenerationTimer, 0f, gearGenerationDuration);
    }

    private UpgradeScreenConfig ResolveUpgradeConfig()
    {
        if (upgradeConfig != null)
        {
            return upgradeConfig;
        }

        UpgradeScreenView[] upgradeViews = Resources.FindObjectsOfTypeAll<UpgradeScreenView>();
        for (int i = 0; i < upgradeViews.Length; i++)
        {
            if (upgradeViews[i] != null && upgradeViews[i].ConfigAsset != null)
            {
                return upgradeViews[i].ConfigAsset;
            }
        }

        return null;
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

    private void RefreshWeaponUpgradeUi()
    {
        for (int i = 0; i < 3; i++)
        {
            WeaponUpgradeController target = GetWeaponUpgradeTarget(i);
            Button button = GetWeaponUpgradeButton(i);
            Image weaponIcon = GetWeaponIcon(i);
            TextMeshProUGUI levelLabel = GetWeaponUpgradeLevelLabel(i);
            TextMeshProUGUI costLabel = GetWeaponUpgradeCostLabel(i);
            WeaponUnlockDefinition weaponDefinition = PlayerUpgradeSystem.Config.GetWeapon(i);
            bool isUnlocked = PlayerUpgradeSystem.IsWeaponUnlocked(i);

            SetWeaponSlotActive(button, target, isUnlocked);
            RefreshWeaponIcon(weaponIcon, weaponDefinition);
            RefreshWeaponSlotLabels(target, weaponDefinition, levelLabel, costLabel);

            if (button != null)
            {
                int requiredGearCost = GetRequiredGearCost(target, weaponDefinition);
                button.interactable =
                    isUnlocked &&
                    target != null &&
                    target.CanUpgrade() &&
                    gearCount >= requiredGearCost;
            }
        }
    }

    private void BindWeaponUpgradeStateEvents(bool shouldBind)
    {
        for (int i = 0; i < 3; i++)
        {
            WeaponUpgradeController target = GetWeaponUpgradeTarget(i);
            if (target == null)
            {
                continue;
            }

            target.UpgradeStateChanged -= HandleWeaponUpgradeStateChanged;
            if (shouldBind)
            {
                target.UpgradeStateChanged += HandleWeaponUpgradeStateChanged;
            }
        }
    }

    private Button GetWeaponUpgradeButton(int weaponIndex)
    {
        return weaponIndex switch
        {
            0 => defaultWeaponUpgradeButton,
            1 => _1WeaponUpgradeButton,
            2 => _2WeaponUpgradeButton,
            _ => null,
        };
    }

    private TextMeshProUGUI GetWeaponUpgradeLevelLabel(int weaponIndex)
    {
        return weaponIndex switch
        {
            0 => defaultWeaponUpgradeLevelLabel,
            1 => _1WeaponUpgradeLevelLabel,
            2 => _2WeaponUpgradeLevelLabel,
            _ => null,
        };
    }

    private Image GetWeaponIcon(int weaponIndex)
    {
        return weaponIndex switch
        {
            0 => defaultWeaponIcon,
            1 => _1WeaponIcon,
            2 => _2WeaponIcon,
            _ => null,
        };
    }

    private TextMeshProUGUI GetWeaponUpgradeCostLabel(int weaponIndex)
    {
        return weaponIndex switch
        {
            0 => defaultRequiredGearsCostLabel,
            1 => _1RequiredGearsCostLabel,
            2 => _2RequiredGearsCostLabel,
            _ => null,
        };
    }

    private WeaponUpgradeController GetWeaponUpgradeTarget(int weaponIndex)
    {
        return weaponIndex switch
        {
            0 => defaultWeaponUpgradeTarget,
            1 => _1WeaponUpgradeTarget,
            2 => _2WeaponUpgradeTarget,
            _ => null,
        };
    }

    private static void SetWeaponSlotActive(Button button, WeaponUpgradeController target, bool isActive)
    {
        if (button != null)
        {
            button.gameObject.SetActive(isActive);
        }

        if (target != null)
        {
            target.gameObject.SetActive(isActive);
        }
    }

    private static void RefreshWeaponSlotLabels(
        WeaponUpgradeController target,
        WeaponUnlockDefinition weaponDefinition,
        TextMeshProUGUI levelLabel,
        TextMeshProUGUI costLabel)
    {
        if (levelLabel != null)
        {
            levelLabel.text = target == null ? "Lv. 0/0" : $"Lv. {target.CurrentLevel}/{target.MaxLevel}";
        }

        if (costLabel != null)
        {
            costLabel.text = target != null && target.CanUpgrade()
                ? GetRequiredGearCost(target, weaponDefinition).ToString()
                : "Max";
        }
    }

    private static void RefreshWeaponIcon(Image weaponIcon, WeaponUnlockDefinition weaponDefinition)
    {
        if (weaponIcon == null)
        {
            return;
        }

        Sprite weaponSprite = weaponDefinition != null ? weaponDefinition.weaponSprite : null;
        weaponIcon.sprite = weaponSprite;
        weaponIcon.enabled = weaponSprite != null;
    }

    private static int GetRequiredGearCost(WeaponUpgradeController target, WeaponUnlockDefinition weaponDefinition)
    {
        if (weaponDefinition != null && weaponDefinition.requiredGearCostForUpgrade > 0)
        {
            return weaponDefinition.requiredGearCostForUpgrade;
        }

        return target != null ? target.CurrentUpgradeCost : 0;
    }

    private void UpdatePlayerHealthUi()
    {
        if (healthBarFill != null)
        {
            float progress = maxPlayerHealth > 0 ? currentPlayerHealth / (float)maxPlayerHealth : 0f;
            healthBarFill.fillAmount = Mathf.Clamp01(progress);
        }

        if (healthBarLabel != null)
        {
            healthBarLabel.text = currentPlayerHealth.ToString();
        }
    }

    private void TriggerGameOver()
    {
        gameOverTriggered = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFailPreview(inGameCoins);
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    private void ResetInGameCoins()
    {
        inGameCoins = 0;
        RefreshInGameCoinLabel();
    }

    private void RefreshInGameCoinLabel()
    {
        if (inGameCoinLabel != null)
        {
            inGameCoinLabel.text = CoinFormatter.FormatCoins(inGameCoins);
        }
    }

    private float GetGearProgress()
    {
        return gearGenerationDuration > 0f ? gearGenerationTimer / gearGenerationDuration : 0f;
    }

    private int ParseInitialGearCount()
    {
        if (gearCounterLabel == null)
        {
            return 0;
        }

        return int.TryParse(gearCounterLabel.text, out int parsedCount) ? Mathf.Max(0, parsedCount) : 0;
    }

    public void ShowChestTriggerImage(int waveNumber)
    {
        if (waveNumber == 4 && chest1TriggerImage != null)
        {
            chest1TriggerImage.SetActive(true);
        }
        else if (waveNumber == 8 && chest2TriggerImage != null)
        {
            chest2TriggerImage.SetActive(true);
        }
    }
}
