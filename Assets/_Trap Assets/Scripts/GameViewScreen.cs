using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameViewScreen : MonoBehaviour
{
    public static GameViewScreen Instance { get; private set; }

    [SerializeField] private ZombieCrowdSpawner zombieCrowdSpawner;
    [SerializeField] private Image waveProgressBarFill;
    [SerializeField] private TextMeshProUGUI waveProgressBarLabel;
    [SerializeField] private Image gearCounterFill;
    [SerializeField] private TextMeshProUGUI gearCounterLabel;
    [SerializeField][Min(0.1f)] private float gearGenerationDuration = 5f;
    [SerializeField] private Button weaponUpgradeButton;
    [SerializeField] private TextMeshProUGUI weaponUpgradeLevelLabel;
    [SerializeField] private WeaponUpgradeController weaponUpgradeTarget;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthBarLabel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField][Min(0)] private int playerHealthUpgradeBonus;
    [SerializeField] private bool pauseOnGameOver = true;

    private int gearCount;
    private float gearGenerationTimer;
    private int maxPlayerHealth;
    private int currentPlayerHealth;
    private bool gameOverTriggered;

    public int GearCount => gearCount;

    private void Awake()
    {
        Instance = this;
        AutoResolveReferences();

        if (zombieCrowdSpawner != null)
        {
            zombieCrowdSpawner.SetGameViewScreen(this);
        }

        gearCount = ParseInitialGearCount();
        UpdateGearUi(GetGearProgress());
        RefreshWaveProgress();
        RefreshWeaponUpgradeUi();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BindWeaponUpgradeUi();

        if (weaponUpgradeTarget != null)
        {
            weaponUpgradeTarget.UpgradeStateChanged -= HandleWeaponUpgradeStateChanged;
            weaponUpgradeTarget.UpgradeStateChanged += HandleWeaponUpgradeStateChanged;
        }

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
        if (weaponUpgradeTarget != null)
        {
            weaponUpgradeTarget.UpgradeStateChanged -= HandleWeaponUpgradeStateChanged;
        }

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

    public void InitializePlayerHealth(int baseHealth)
    {
        maxPlayerHealth = Mathf.Max(1, baseHealth + playerHealthUpgradeBonus);
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

    private void BindWeaponUpgradeUi()
    {
        if (weaponUpgradeButton == null)
        {
            return;
        }

        weaponUpgradeButton.onClick.RemoveListener(HandleWeaponUpgradeClicked);
        weaponUpgradeButton.onClick.AddListener(HandleWeaponUpgradeClicked);
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

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
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

    private void AutoResolveReferences()
    {
        if (zombieCrowdSpawner == null)
        {
            zombieCrowdSpawner = FindFirstObjectByType<ZombieCrowdSpawner>();
        }

        if (waveProgressBarLabel == null)
        {
            waveProgressBarLabel = FindLabelByName("Wave Count Text (TMP)");
        }

        if (waveProgressBarFill == null)
        {
            waveProgressBarFill = FindSiblingFillImage(waveProgressBarLabel);
        }

        if (gearCounterLabel == null)
        {
            gearCounterLabel = FindLabelByName("Gere count Text (TMP)") ?? FindLabelByName("Gear Count Text (TMP)");
        }

        if (gearCounterFill == null)
        {
            gearCounterFill = FindSiblingFillImage(gearCounterLabel);
        }

        if (weaponUpgradeButton == null)
        {
            weaponUpgradeButton = FindButtonByName("Weapon Upgrade");
        }

        if (weaponUpgradeLevelLabel == null && weaponUpgradeButton != null)
        {
            TextMeshProUGUI[] labels = weaponUpgradeButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null && labels[i].transform.parent == weaponUpgradeButton.transform)
                {
                    weaponUpgradeLevelLabel = labels[i];
                    break;
                }
            }

            if (weaponUpgradeLevelLabel == null && labels.Length > 0)
            {
                weaponUpgradeLevelLabel = labels[0];
            }
        }

        if (weaponUpgradeTarget == null)
        {
            weaponUpgradeTarget = FindFirstObjectByType<WeaponUpgradeController>();
        }

        if (healthBarLabel == null)
        {
            healthBarLabel = FindLabelByName("Health Count Text (TMP)");
        }

        if (healthBarFill == null)
        {
            healthBarFill = FindSiblingFillImage(healthBarLabel);
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = FindGameObjectByName("Game Over Panel");
        }
    }

    private TextMeshProUGUI FindLabelByName(string objectName)
    {
        TextMeshProUGUI[] labels = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && labels[i].name == objectName)
            {
                return labels[i];
            }
        }

        return null;
    }

    private Button FindButtonByName(string partialName)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].name.Contains(partialName))
            {
                return buttons[i];
            }
        }

        return null;
    }

    private Image FindSiblingFillImage(Component referenceComponent)
    {
        if (referenceComponent == null || referenceComponent.transform.parent == null)
        {
            return null;
        }

        Image[] images = referenceComponent.transform.parent.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name.Contains("Fill"))
            {
                return images[i];
            }
        }

        return null;
    }

    private GameObject FindGameObjectByName(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].name == objectName)
            {
                return transforms[i].gameObject;
            }
        }

        return null;
    }
}
