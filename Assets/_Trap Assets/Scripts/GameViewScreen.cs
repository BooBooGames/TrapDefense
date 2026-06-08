using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameViewScreen : MonoBehaviour
{
    public static GameViewScreen Instance { get; private set; }

    ZombieCrowdSpawner zombieCrowdSpawner;
    UpgradeScreenConfig upgradeConfig;
    CurrencyView currencyView;

    public TextMeshProUGUI inGameCoinLabel;
    [SerializeField] private Image waveProgressBarFill;
    [SerializeField] private GameObject chestBG1, chestBG2, chest1TriggerImage, chest2TriggerImage;
    [SerializeField] private Image gearCounterFill;
    [SerializeField] private TextMeshProUGUI gearCounterLabel;
    [SerializeField][Min(0.1f)] private float gearGenerationDuration = 5f;

    [SerializeField] private Button defaultWeaponUpgradeButton;
    [SerializeField] private Image defaultWeaponIcon;
    [SerializeField] private TextMeshProUGUI defaultWeaponUpgradeLevelLabel, defaultRequiredGearsCostLabel;

    [SerializeField] private Button _1WeaponUpgradeButton;
    [SerializeField] private Image _1WeaponIcon;

    [SerializeField] private TextMeshProUGUI _1WeaponUpgradeLevelLabel, _1RequiredGearsCostLabel;

    [SerializeField] private Button _2WeaponUpgradeButton;
    [SerializeField] private Image _2WeaponIcon;

    [SerializeField] private TextMeshProUGUI _2WeaponUpgradeLevelLabel, _2RequiredGearsCostLabel;



    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject lifeIcon;
    [SerializeField] private TextMeshProUGUI healthBarLabel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField][Min(0)] private int playerHealthUpgradeBonus;
    [SerializeField] private bool pauseOnGameOver = true;

    private const float LifeIconPunchScale = 0.18f;
    private const float LifeIconPunchDuration = 0.28f;
    private const float WeaponButtonPressedScale = 0.92f;
    private const float WeaponButtonPressInDuration = 0.08f;
    private const float WeaponButtonPressOutDuration = 0.14f;
    private const int GameOverElixirReward = 2;
    private const int LevelCompleteElixirReward = 4;
    private const int GameOverElixirRewardMinWave = 7;
    private const int GameOverElixirRewardMaxWave = 9;

    private int gearCount;
    private float gearGenerationTimer;
    private int maxPlayerHealth;
    private int currentPlayerHealth;
    private int inGameCoins;
    private bool gameOverTriggered;
    private Vector3 lifeIconBaseScale;
    private bool lifeIconScaleCached;
    private Tween lifeIconPunchTween;
    private readonly Vector3[] weaponUpgradeButtonBaseScales = new Vector3[3];
    private readonly bool[] weaponUpgradeButtonScaleCached = new bool[3];
    private readonly Tween[] weaponUpgradeButtonTweens = new Tween[3];

    public int GearCount => gearCount;
    public int InGameCoins => inGameCoins;
    public Vector3 GearCounterLabelPosition => gearCounterLabel.transform.position;
    public Vector3 HealthBarLabelPosition => healthBarLabel.transform.position;

    private void Awake()
    {
        Instance = this;
        currencyView = FindFirstObjectByType<CurrencyView>(FindObjectsInactive.Include);
        RefreshLevelReferences();
        ApplyPersistentUpgradeState();

        zombieCrowdSpawner.SetGameViewScreen(this);

        UpdateGearUi(GetGearProgress());
        RefreshInGameCoinLabel();
        RefreshWaveProgress();
        RefreshWeaponUpgradeUi();
        RefreshChestAvailabilityVisuals();

        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        ApplyPersistentUpgradeState();
        BindWeaponUpgradeUi();
        PlayerUpgradeSystem.UpgradeStateChanged -= HandlePlayerUpgradeStateChanged;
        PlayerUpgradeSystem.UpgradeStateChanged += HandlePlayerUpgradeStateChanged;

        BindWeaponUpgradeStateEvents(true);

        zombieCrowdSpawner.ProgressChanged -= HandleWaveProgressChanged;
        zombieCrowdSpawner.ProgressChanged += HandleWaveProgressChanged;

        RefreshWaveProgress();
        RefreshWeaponUpgradeUi();
    }

    private void OnDisable()
    {
        PlayerUpgradeSystem.UpgradeStateChanged -= HandlePlayerUpgradeStateChanged;
        BindWeaponUpgradeStateEvents(false);
        StopUiFeedbackAnimations();

        zombieCrowdSpawner.ProgressChanged -= HandleWaveProgressChanged;
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
        RefreshLevelReferences();
        ApplyPersistentUpgradeState();
        EndPointTrigger.ResetAllGates();
        ResetInGameCoins();
        RefreshWeaponUpgradeUi();
        RefreshChestAvailabilityVisuals();

        if (!gameOverTriggered)
        {
            Time.timeScale = 1f;
        }

        gameOverPanel.SetActive(false);

        zombieCrowdSpawner.StartWaves();
    }

    public void ResetSessionDataForNextLevel()
    {
        RefreshLevelReferences();
        StopUiFeedbackAnimations();
        gameOverTriggered = false;
        gearCount = 0;
        gearGenerationTimer = 0f;
        ResetInGameCoins();
        ResetWeaponUpgradeTargets();
        InitializePlayerHealth();
        RefreshWaveProgress();
        RefreshWeaponUpgradeUi();
        UpdateGearUi(GetGearProgress());
        RefreshChestAvailabilityVisuals();
    }

    public void EndGameplay()
    {
        zombieCrowdSpawner.StopWaves();
        gameOverTriggered = false;
        Time.timeScale = 1f;
        ResetInGameCoins();

        gameOverPanel.SetActive(false);

        RefreshWaveProgress();
        RefreshChestAvailabilityVisuals();
    }

    public void InitializePlayerHealth()
    {
        ApplyPersistentUpgradeState();
        maxPlayerHealth = Mathf.Max(1, PlayerUpgradeSystem.CurrentBaseHealthValue);
        currentPlayerHealth = maxPlayerHealth;
        gameOverTriggered = false;

        gameOverPanel.SetActive(false);

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

        gameOverPanel.SetActive(false);

        UIManager.Instance.ShowWinPreview(inGameCoins, LevelCompleteElixirReward);

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

    public void AddHealth(int amount)
    {
        if (amount <= 0 || gameOverTriggered)
        {
            return;
        }

        int maxHealth = Mathf.Max(1, maxPlayerHealth > 0 ? maxPlayerHealth : PlayerUpgradeSystem.CurrentBaseHealthValue);
        maxPlayerHealth = maxHealth;

        int remainingHealth = Mathf.Max(0, maxHealth - currentPlayerHealth);
        int healthToAdd = Mathf.Min(amount, remainingHealth);
        currentPlayerHealth = Mathf.Min(maxHealth, currentPlayerHealth + healthToAdd);
        UpdatePlayerHealthUi();
    }

    public void DamagePlayer(int amount)
    {
        if (gameOverTriggered || amount <= 0)
        {
            return;
        }

        currentPlayerHealth = Mathf.Max(0, currentPlayerHealth - amount);
        if (!TryApplySecondWind())
        {
            UpdatePlayerHealthUi();
        }

        PlayLifeIconDamageFeedback();

        if (currentPlayerHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private bool TryApplySecondWind()
    {
        if (currentPlayerHealth != 1)
        {
            return false;
        }

        PlayerXpSystem playerXpSystem = PlayerXpSystem.Instance;
        if (playerXpSystem == null || !playerXpSystem.TryConsumeSecondWind(out int healAmount))
        {
            return false;
        }

        AddHealth(healAmount);

        if (UIParticleEffectsManager.Instance != null)
        {
            UIParticleEffectsManager.Instance.PlayHealthEffect(HealthBarLabelPosition);
        }

        return true;
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

    public void AddGears(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gearCount = Mathf.Max(0, gearCount + amount);
        UpdateGearUi(GetGearProgress());
    }

    public void RefreshSceneWeaponUnlockState()
    {
        ApplyPersistentUpgradeState();
        RefreshWeaponUpgradeUi();
    }

    public void RefreshResourceMasteryModifiers()
    {
        ApplyPersistentUpgradeState();
        RefreshWeaponUpgradeUi();
        UpdateGearUi(GetGearProgress());
    }

    public bool wave4triggered, wave8triggered;
    public void RefreshChestAvailabilityVisuals()
    {
        int emptyChestSlots = HomeViewScreen.GetEmptyChestSlotCount();
        bool isChest1Available = HomeViewScreen.IsChestRewardAvailableForWave(4) && emptyChestSlots > 0;

        if (isChest1Available)
        {
            emptyChestSlots--;
        }

        bool isChest2Available = HomeViewScreen.IsChestRewardAvailableForWave(8) && emptyChestSlots > 0;

        chestBG1.SetActive(isChest1Available);
        if (isChest1Available)
        {
            chest1TriggerImage.SetActive(false);
        }
        chest1TriggerImage.transform.parent.gameObject.SetActive(isChest1Available);
        chestBG2.SetActive(isChest2Available);
        if (isChest2Available)
        {
            chest2TriggerImage.SetActive(false);
        }
        chest2TriggerImage.transform.parent.gameObject.SetActive(isChest2Available);
    }

    private void BindWeaponUpgradeUi()
    {
        for (int i = 0; i < 3; i++)
        {
            Button button = GetWeaponUpgradeButton(i);
            int weaponIndex = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleWeaponUpgradeClicked(weaponIndex));
        }
    }

    private void HandleWeaponUpgradeClicked(int weaponIndex)
    {
        PlayWeaponUpgradeButtonPressFeedback(weaponIndex);

        WeaponUpgradeController target = GetWeaponUpgradeTarget(weaponIndex);
        if (!PlayerUpgradeSystem.IsWeaponUnlocked(weaponIndex))
        {
            return;
        }

        WeaponUnlockDefinition weaponDefinition = PlayerUpgradeSystem.Config.GetWeapon(weaponIndex);
        target.TryUpgrade(this, GetRequiredGearCost(target, weaponDefinition));
        RefreshWeaponUpgradeUi();
        SoundManager.Instance.PlayButtonClickSound();
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
        waveProgressBarFill.fillAmount = Mathf.Clamp01(overallProgress);

        int displayedWave = totalWaves > 0 ? Mathf.Clamp(Mathf.Max(currentWave, 1), 1, totalWaves) : 0;
        currencyView.SetWaveCounterText(totalWaves > 0 ? $"Wave {displayedWave}/{totalWaves}" : "Wave 0/0");
    }

    private void RefreshWaveProgress()
    {
        HandleWaveProgressChanged(
            zombieCrowdSpawner.CurrentWaveNumber,
            zombieCrowdSpawner.TotalWaves,
            zombieCrowdSpawner.OverallProgress);
    }

    private void ApplyPersistentUpgradeState()
    {
        PlayerUpgradeSystem.Initialize(ResolveUpgradeConfig());
        gearGenerationDuration = Mathf.Max(0.1f, PlayerUpgradeSystem.CurrentGearFlowValue);
        if (PlayerXpSystem.Instance != null)
        {
            gearGenerationDuration *= PlayerXpSystem.Instance.GetGearGenerationDurationMultiplier();
        }

        playerHealthUpgradeBonus = Mathf.Max(0, PlayerUpgradeSystem.CurrentBaseHealthBonus);
        gearGenerationTimer = Mathf.Clamp(gearGenerationTimer, 0f, gearGenerationDuration);
    }

    private UpgradeScreenConfig ResolveUpgradeConfig()
    {
        return upgradeConfig;
    }

    private void RefreshLevelReferences()
    {
        if (zombieCrowdSpawner != null)
        {
            zombieCrowdSpawner.ProgressChanged -= HandleWaveProgressChanged;
        }

        zombieCrowdSpawner = LevelManager.Instance.activeLevelInstance.crowdSpawner;
        upgradeConfig = LevelManager.Instance.activeLevelInstance.upgradeScreenConfig;
        zombieCrowdSpawner.SetGameViewScreen(this);

        zombieCrowdSpawner.ProgressChanged -= HandleWaveProgressChanged;
        zombieCrowdSpawner.ProgressChanged += HandleWaveProgressChanged;
    }

    private void UpdateGearUi(float progress)
    {
        gearCounterFill.fillAmount = Mathf.Clamp01(progress);
        gearCounterLabel.text = gearCount < 40 ? gearCount.ToString() : "MAX";

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

            int requiredGearCost = GetRequiredGearCost(target, weaponDefinition);
            button.interactable =
                isUnlocked &&
                target.CanUpgrade() &&
                gearCount >= requiredGearCost;
        }
    }

    private void BindWeaponUpgradeStateEvents(bool shouldBind)
    {
        for (int i = 0; i < 3; i++)
        {
            WeaponUpgradeController target = GetWeaponUpgradeTarget(i);
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
            0 => LevelManager.Instance.activeLevelInstance.defaultWeaponUpgradeTarget,
            1 => LevelManager.Instance.activeLevelInstance._1WeaponUpgradeTarget,
            2 => LevelManager.Instance.activeLevelInstance._2WeaponUpgradeTarget,
            _ => null,
        };
    }

    private static void SetWeaponSlotActive(Button button, WeaponUpgradeController target, bool isActive)
    {
        button.gameObject.SetActive(isActive);
        target.gameObject.SetActive(isActive);
    }

    private static void RefreshWeaponSlotLabels(
        WeaponUpgradeController target,
        WeaponUnlockDefinition weaponDefinition,
        TextMeshProUGUI levelLabel,
        TextMeshProUGUI costLabel)
    {
        levelLabel.text = $"Lv. {target.CurrentLevel}/{target.MaxLevel}";
        costLabel.text = target.CanUpgrade()
            ? GetRequiredGearCost(target, weaponDefinition).ToString()
            : "Max";
    }

    private static void RefreshWeaponIcon(Image weaponIcon, WeaponUnlockDefinition weaponDefinition)
    {
        Sprite weaponSprite = weaponDefinition.weaponSprite;
        weaponIcon.sprite = weaponSprite;
        weaponIcon.enabled = weaponSprite != null;
    }

    private static int GetRequiredGearCost(WeaponUpgradeController target, WeaponUnlockDefinition weaponDefinition)
    {
        int baseCost = weaponDefinition.requiredGearCostForUpgrade;
        return PlayerXpSystem.Instance != null
            ? PlayerXpSystem.Instance.ApplyGearUpgradeCostModifiers(baseCost)
            : baseCost;
    }

    private void UpdatePlayerHealthUi()
    {
        float progress = maxPlayerHealth > 0 ? currentPlayerHealth / (float)maxPlayerHealth : 0f;
        healthBarFill.fillAmount = Mathf.Clamp01(progress);
        healthBarLabel.text = currentPlayerHealth.ToString();
    }

    private void PlayLifeIconDamageFeedback()
    {
        Transform iconTransform = lifeIcon.transform;
        if (!lifeIconScaleCached)
        {
            lifeIconBaseScale = iconTransform.localScale;
            lifeIconScaleCached = true;
        }

        lifeIconPunchTween?.Kill();
        iconTransform.localScale = lifeIconBaseScale;
        lifeIconPunchTween = iconTransform
            .DOPunchScale(Vector3.one * LifeIconPunchScale, LifeIconPunchDuration, 8, 0.7f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void PlayWeaponUpgradeButtonPressFeedback(int weaponIndex)
    {
        Button button = GetWeaponUpgradeButton(weaponIndex);
        Transform buttonTransform = button.transform;
        Vector3 baseScale = GetWeaponUpgradeButtonBaseScale(weaponIndex, buttonTransform);

        weaponUpgradeButtonTweens[weaponIndex]?.Kill();
        buttonTransform.localScale = baseScale;

        weaponUpgradeButtonTweens[weaponIndex] = DOTween.Sequence()
            .Append(buttonTransform.DOScale(baseScale * WeaponButtonPressedScale, WeaponButtonPressInDuration).SetEase(Ease.OutQuad))
            .Append(buttonTransform.DOScale(baseScale, WeaponButtonPressOutDuration).SetEase(Ease.OutBack))
            .SetUpdate(true);
    }

    private Vector3 GetWeaponUpgradeButtonBaseScale(int weaponIndex, Transform buttonTransform)
    {
        if (!weaponUpgradeButtonScaleCached[weaponIndex])
        {
            weaponUpgradeButtonBaseScales[weaponIndex] = buttonTransform.localScale;
            weaponUpgradeButtonScaleCached[weaponIndex] = true;
        }

        return weaponUpgradeButtonBaseScales[weaponIndex];
    }

    private void StopUiFeedbackAnimations()
    {
        lifeIconPunchTween?.Kill();
        lifeIconPunchTween = null;

        if (lifeIconScaleCached)
        {
            lifeIcon.transform.localScale = lifeIconBaseScale;
        }

        for (int i = 0; i < weaponUpgradeButtonTweens.Length; i++)
        {
            weaponUpgradeButtonTweens[i]?.Kill();
            weaponUpgradeButtonTweens[i] = null;

            Button button = GetWeaponUpgradeButton(i);
            if (weaponUpgradeButtonScaleCached[i])
            {
                button.transform.localScale = weaponUpgradeButtonBaseScales[i];
            }
        }
    }

    private void TriggerGameOver()
    {
        gameOverTriggered = true;
        int elixirReward = GetGameOverElixirReward();

        UIManager.Instance.ShowFailPreview(inGameCoins, elixirReward);

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
        inGameCoinLabel.text = CoinFormatter.FormatCoins(inGameCoins);
    }

    private int GetGameOverElixirReward()
    {
        int reachedWave = zombieCrowdSpawner != null ? zombieCrowdSpawner.CurrentWaveNumber : 0;
        if (reachedWave < GameOverElixirRewardMinWave || reachedWave > GameOverElixirRewardMaxWave)
        {
            return 0;
        }

        return GameOverElixirReward;
    }

    private float GetGearProgress()
    {
        return gearGenerationDuration > 0f ? gearGenerationTimer / gearGenerationDuration : 0f;
    }

    public void ShowChestTriggerImage(int waveNumber)
    {
        if (waveNumber == 4 && wave4triggered)
        {
            chest1TriggerImage.SetActive(true);
        }
        else if (waveNumber == 8 && wave8triggered)
        {
            chest2TriggerImage.SetActive(true);
        }
    }

    private void ResetWeaponUpgradeTargets()
    {
        for (int i = 0; i < 3; i++)
        {
            GetWeaponUpgradeTarget(i).ResetSessionState();
        }
    }
}
