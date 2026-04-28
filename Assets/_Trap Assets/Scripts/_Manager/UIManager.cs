using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private enum BottomHudButtonIndex
    {
        Lock = 0,
        Upgrade = 1,
        Home = 2,
        Card = 3,
        Shop = 4
    }

    public static UIManager Instance { get; private set; }
    public GameObject gameOverPanel, gameViewPanel, homeScreenPanel, upgradeScreenPanel, cardViewPanel, shopPanel, bottomHudPanel, selectedButtonImage;

    public List<Button> bottomHudButtons;

    public Button playButton;

    public Image waveProgressBarFill;
    public TextMeshProUGUI waveProgressBarLabel;
    public Image gearCounterFill;
    public TextMeshProUGUI gearCounterLabel;

    [SerializeField][Min(0.1f)] private float gearGenerationDuration = 5f;

    public TextMeshProUGUI coinCounterLabel, gemsCounterLabel;

    public Button weaponUpgradeButton;
    public TextMeshProUGUI weaponUpgradeLevelLabel;
    public WeaponUpgradeController weaponUpgradeTarget;
    public ZombieCrowdSpawner zombieCrowdSpawner;

    public Image healthBarFill;
    public TextMeshProUGUI healthBarLabel;


    [SerializeField][Min(0)] private int playerHealthUpgradeBonus = 0;
    [SerializeField] private bool pauseOnGameOver = true;
    [SerializeField] private float selectedButtonYPosition = 0f;
    [SerializeField][Min(1f)] private float selectedButtonScale = 1.2f;
    [SerializeField][Min(0.1f)] private float unselectedButtonScale = 1f;

    private int gearCount;
    private float gearGenerationTimer;
    private int maxPlayerHealth;
    private int currentPlayerHealth;
    private bool gameOverTriggered;
    private readonly List<float> defaultBottomButtonYPositions = new List<float>();

    private void Awake()
    {
        Instance = this;
        PlayerCurrencySystem.Initialize(ParseLabelValue(coinCounterLabel), ParseLabelValue(gemsCounterLabel));
        gearCount = ParseInitialGearCount();
        UpdateGearUi(0f);
        UpdateCurrencyUi(PlayerCurrencySystem.Coins, PlayerCurrencySystem.Gems);
        BindNavigationButtons();
        BindWeaponUpgradeUi();
        RefreshWeaponUpgradeUi();
        if (zombieCrowdSpawner == null)
        {
            zombieCrowdSpawner = FindFirstObjectByType<ZombieCrowdSpawner>();
        }
        PlayerCurrencySystem.CurrencyChanged += HandleCurrencyChanged;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        CacheBottomHudButtonDefaults();
        ShowHomeScreen();
        UpdateBottomHudSelection((int)BottomHudButtonIndex.Home);
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

        PlayerCurrencySystem.CurrencyChanged -= HandleCurrencyChanged;
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

        if (currentPlayerHealth > 0)
        {
            return;
        }

        TriggerGameOver();
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

    private void UpdateCurrencyUi(int coins, int gems)
    {
        if (coinCounterLabel != null)
        {
            coinCounterLabel.text = coins.ToString();
        }

        if (gemsCounterLabel != null)
        {
            gemsCounterLabel.text = gems.ToString();
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

    public void ShowHomeScreen()
    {
        ShowScreen(homeScreenPanel, true);
    }

    public void ShowUpgradeScreen()
    {
        ShowScreen(upgradeScreenPanel, true);
    }

    public void ShowCardScreen()
    {
        ShowScreen(cardViewPanel, true);
    }

    public void ShowShopScreen()
    {
        ShowScreen(shopPanel, true);
    }

    public void StartGame()
    {
        if (!gameOverTriggered)
        {
            Time.timeScale = 1f;
        }

        ShowScreen(gameViewPanel, false);
        zombieCrowdSpawner?.StartWaves();
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

    private void BindNavigationButtons()
    {
        BindBottomHudButton(BottomHudButtonIndex.Lock, HandleLockButtonClicked);
        BindBottomHudButton(BottomHudButtonIndex.Upgrade, ShowUpgradeScreen);
        BindBottomHudButton(BottomHudButtonIndex.Home, ShowHomeScreen);
        BindBottomHudButton(BottomHudButtonIndex.Card, ShowCardScreen);
        BindBottomHudButton(BottomHudButtonIndex.Shop, ShowShopScreen);

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(StartGame);
            playButton.onClick.AddListener(StartGame);
        }
    }

    private void BindBottomHudButton(BottomHudButtonIndex index, UnityAction callback)
    {
        if (bottomHudButtons == null)
        {
            return;
        }

        int buttonIndex = (int)index;
        if (buttonIndex < 0 || buttonIndex >= bottomHudButtons.Count)
        {
            return;
        }

        Button button = bottomHudButtons[buttonIndex];
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(() => HandleBottomHudButtonClicked(buttonIndex, callback));
    }

    private void HandleLockButtonClicked()
    {
    }

    private void HandleBottomHudButtonClicked(int buttonIndex, UnityAction callback)
    {
        UpdateBottomHudSelection(buttonIndex);
        callback?.Invoke();
    }

    private void CacheBottomHudButtonDefaults()
    {
        defaultBottomButtonYPositions.Clear();

        if (bottomHudButtons == null)
        {
            return;
        }

        for (int i = 0; i < bottomHudButtons.Count; i++)
        {
            RectTransform buttonRect = bottomHudButtons[i] != null ? bottomHudButtons[i].transform as RectTransform : null;
            defaultBottomButtonYPositions.Add(buttonRect != null ? buttonRect.anchoredPosition.y : 0f);
        }
    }

    private void UpdateBottomHudSelection(int selectedIndex)
    {
        if (bottomHudButtons == null)
        {
            return;
        }

        for (int i = 0; i < bottomHudButtons.Count; i++)
        {
            Button button = bottomHudButtons[i];
            if (button == null)
            {
                continue;
            }

            RectTransform buttonRect = button.transform as RectTransform;
            if (buttonRect == null)
            {
                continue;
            }

            bool isSelected = i == selectedIndex;
            Vector2 anchoredPosition = buttonRect.anchoredPosition;
            float defaultY = i < defaultBottomButtonYPositions.Count ? defaultBottomButtonYPositions[i] : anchoredPosition.y;
            anchoredPosition.y = isSelected ? selectedButtonYPosition : defaultY;
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.localScale = Vector3.one * (isSelected ? selectedButtonScale : unselectedButtonScale);

            if (!isSelected || selectedButtonImage == null)
            {
                continue;
            }

            RectTransform selectionRect = selectedButtonImage.transform as RectTransform;
            if (selectionRect != null)
            {
                selectionRect.position = buttonRect.position;
            }
            else
            {
                selectedButtonImage.transform.position = button.transform.position;
            }
        }
    }

    private void ShowScreen(GameObject activePanel, bool showBottomHud)
    {
        SetPanelActive(homeScreenPanel, activePanel == homeScreenPanel);
        SetPanelActive(upgradeScreenPanel, activePanel == upgradeScreenPanel);
        SetPanelActive(cardViewPanel, activePanel == cardViewPanel);
        SetPanelActive(shopPanel, activePanel == shopPanel);
        SetPanelActive(gameViewPanel, activePanel == gameViewPanel);
        SetPanelActive(bottomHudPanel, showBottomHud);
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
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

    private void HandleCurrencyChanged(int coins, int gems)
    {
        UpdateCurrencyUi(coins, gems);
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
