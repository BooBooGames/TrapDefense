using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private UpgradeScreenConfig upgradeConfig;


    [SerializeField] private GameObject gameViewPanel;
    [SerializeField] private GameObject homeScreenPanel;
    [SerializeField] private GameObject cardViewPanel;
    [SerializeField] private GameObject upgradeScreenPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private CurrencyView currencyView;
    [SerializeField] private GameObject bottomHudPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject chestPreviewPanel;
    [SerializeField] private GameObject winPreviewPanel;
    [SerializeField] private GameObject failPreviewPanel;
    [SerializeField] private GameObject evolutionScreenPanel;
    [SerializeField] private GameObject cardUpgradePanel;
    [SerializeField] private GameObject ageChangePanel;
    [SerializeField] private GameObject FTUEPanel;
    [SerializeField] private PerksCardInfoPanel perksCardInfoPanel;
    // [SerializeField] private Button playButton;

    private BottomHudView bottomHudController;
    private GameViewScreen gameViewScreen;
    private UpgradeScreenView upgradeScreenView;
    private SettingPanelView settingPanelView;
    private WinPreviewPanel winPreviewPanelView;
    private FailPreviewPanel failPreviewPanelView;
    private EvolutionScreenView evolutionScreenView;
    public FTUEController ftueController;
    private GameObject currentScreenPanel;
    private bool settingsOpenedFromGameView;
    private float timeScaleBeforeSettings = 1f;
    public UpgradeScreenConfig ConfigAsset => upgradeConfig;

    private void Awake()
    {
        Instance = this;
        // BindFlowButtons();

        bottomHudController = bottomHudPanel.GetComponent<BottomHudView>();
        gameViewScreen = gameViewPanel.GetComponent<GameViewScreen>();
        upgradeScreenView = upgradeScreenPanel.GetComponent<UpgradeScreenView>();
        settingPanelView = settingPanel.GetComponentInChildren<SettingPanelView>(true);
        winPreviewPanelView = winPreviewPanel.GetComponent<WinPreviewPanel>();
        failPreviewPanelView = failPreviewPanel.GetComponent<FailPreviewPanel>();
        evolutionScreenView = evolutionScreenPanel.GetComponent<EvolutionScreenView>();
        currencyView.BindEvolutionButton(ShowEvolutionPanel);
        PlayerUpgradeSystem.Initialize(upgradeConfig);

        if (FTUEController.IsCompleted)
        {
            ShowHomeScreen(true);
        }
        else
        {
            ShowFTUE();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowHomeScreen(bool fromFTUE = false)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        ShowScreen(homeScreenPanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.HomeButtonIndex);
        if (!fromFTUE) SoundManager.Instance.PlayButtonClickSound();
    }

    public void ShowUpgradeScreen()
    {
        ShowScreen(upgradeScreenPanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.UpgradeButtonIndex);
        SoundManager.Instance.PlayButtonClickSound();
    }

    public void ShowCardScreen()
    {
        ShowScreen(cardUpgradePanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.CardButtonIndex);
        SoundManager.Instance.PlayButtonClickSound();
    }

    public void ShowShopScreen()
    {
        ShowScreen(shopPanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.ShopButtonIndex);
        SoundManager.Instance.PlayButtonClickSound();
    }

    public void ShowSettingsScreen()
    {
        settingsOpenedFromGameView = currentScreenPanel == gameViewPanel;
        settingPanelView.ConfigureOpenContext(settingsOpenedFromGameView);

        if (settingsOpenedFromGameView)
        {
            timeScaleBeforeSettings = Time.timeScale;
            Time.timeScale = 0f;
            WeaponRotator.SetGameplayMotionEnabled(false);
            WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        }

        SetPanelActive(settingPanel, true);
        UpdateGameViewBGImageVisibility();
    }

    public void CloseSettingsScreen()
    {
        SetPanelActive(settingPanel, false);
        ResumeGameIfSettingsPaused();
        UpdateGameViewBGImageVisibility();
    }

    public void EndGameAndShowHomeFromSettings()
    {
        SetPanelActive(settingPanel, false);
        settingsOpenedFromGameView = false;
        ShowFailPreview(gameViewScreen.InGameCoins);
    }

    public void StartGame()
    {
        ShowScreen(gameViewPanel, false);
        WeaponRotator.SetGameplayMotionEnabled(true);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(true);
        gameViewScreen.StartGameplay();
    }

    public void RefreshGameSceneWeaponUnlockState()
    {
        gameViewScreen.RefreshSceneWeaponUnlockState();
    }

    public void ShowWinPreview(int coins, int elixirReward = 0)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        SetPanelActive(failPreviewPanel, false);
        winPreviewPanelView.Show(
            coins,
            elixirReward,
            () => CollectGameCoinsAndStartNextLevel(coins, 1, elixirReward, 1, winPreviewPanelView.Hide),
            multiplier => CollectGameCoinsAndStartNextLevel(coins, multiplier, elixirReward, 2, winPreviewPanelView.Hide));
    }

    public void ShowFailPreview(int coins, int elixirReward = 0)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        SetPanelActive(winPreviewPanel, false);
        failPreviewPanelView.Show(
            coins,
            elixirReward,
            () => CollectGameCoinsAndReturnHome(coins, 1, elixirReward, 1, failPreviewPanelView.Hide),
            () => CollectGameCoinsAndReturnHome(coins, 2, elixirReward, 2, failPreviewPanelView.Hide));
    }

    /*  private void BindFlowButtons()
     {
         if (playButton == null)
         {
             return;
         }

         playButton.onClick.RemoveListener(StartGame);
         playButton.onClick.AddListener(StartGame);
     } */

    private void ShowScreen(GameObject activePanel, bool showBottomHud)
    {
        currentScreenPanel = activePanel;

        SetPanelActive(currencyView.gameObject, true);
        SetPanelActive(homeScreenPanel, activePanel == homeScreenPanel);
        SetPanelActive(upgradeScreenPanel, activePanel == upgradeScreenPanel);
        SetPanelActive(cardViewPanel, activePanel == cardViewPanel);
        SetPanelActive(shopPanel, activePanel == shopPanel);
        SetPanelActive(gameViewPanel, activePanel == gameViewPanel);
        SetPanelActive(bottomHudPanel, showBottomHud);
        SetPanelActive(settingPanel, activePanel == settingPanel);
        SetPanelActive(winPreviewPanel, false);
        SetPanelActive(failPreviewPanel, false);
        SetPanelActive(chestPreviewPanel, false);
        SetPanelActive(cardUpgradePanel, activePanel == cardUpgradePanel);
        SetPanelActive(ageChangePanel, false);
        SetPanelActive(FTUEPanel, false);
        UpdateGameViewBGImageVisibility();
    }

    private void ShowFTUE()
    {
        currentScreenPanel = FTUEPanel;
        Time.timeScale = 1f;
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);

        SetPanelActive(currencyView.gameObject, false);
        SetPanelActive(homeScreenPanel, false);
        SetPanelActive(upgradeScreenPanel, false);
        SetPanelActive(cardViewPanel, false);
        SetPanelActive(shopPanel, false);
        SetPanelActive(gameViewPanel, false);
        SetPanelActive(bottomHudPanel, false);
        SetPanelActive(settingPanel, false);
        SetPanelActive(winPreviewPanel, false);
        SetPanelActive(failPreviewPanel, false);
        SetPanelActive(chestPreviewPanel, false);
        SetPanelActive(cardUpgradePanel, false);
        SetPanelActive(ageChangePanel, false);
        SetPanelActive(FTUEPanel, true);

        ftueController.Begin(CompleteFTUEAndStartGame);
    }

    private void CompleteFTUEAndStartGame()
    {
        SetPanelActive(FTUEPanel, false);
        ShowHomeScreen(true);
        // StartGame();
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        panel.SetActive(isActive);
    }

    private void UpdateGameViewBGImageVisibility()
    {
        bool isSettingsOpen = settingPanel.activeSelf;
        bool shouldShowBGImage = !isSettingsOpen && (currentScreenPanel == gameViewPanel || currentScreenPanel == homeScreenPanel);
        currencyView.SetGameViewBGImageVisible(shouldShowBGImage);
        currencyView.SetUpgradeScreenBGImageVisible(currentScreenPanel == upgradeScreenPanel);
        currencyView.SetEvolutionButtonVisible(!isSettingsOpen && currentScreenPanel == homeScreenPanel);
        currencyView.SetElixirCounterVisible(!isSettingsOpen && (currentScreenPanel == cardUpgradePanel || currentScreenPanel == cardViewPanel));
        currencyView.SetWaveCounterVisible(currentScreenPanel == gameViewPanel);
    }

    private void ShowEvolutionPanel()
    {
        UpgradeScreenConfig config = PlayerUpgradeSystem.Config;
        evolutionScreenView.ShowEvolutionBackground(config);
        SoundManager.Instance.PlayButtonClickSound();
    }

    private void ResumeGameIfSettingsPaused()
    {
        if (!settingsOpenedFromGameView)
        {
            return;
        }

        settingsOpenedFromGameView = false;
        Time.timeScale = timeScaleBeforeSettings <= 0f ? 1f : timeScaleBeforeSettings;
        WeaponRotator.SetGameplayMotionEnabled(currentScreenPanel == gameViewPanel);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(currentScreenPanel == gameViewPanel);
    }

    private void CollectGameCoinsAndReturnHome(int coins, int multiplier, int elixirReward, int elixirMultiplier, UnityEngine.Events.UnityAction hidePanel)
    {
        SoundManager.Instance.PlayButtonClickSound();
        hidePanel.Invoke();

        CollectGameCoins(coins, multiplier);
        CollectElixir(elixirReward, elixirMultiplier);

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void CollectGameCoinsAndStartNextLevel(int coins, int multiplier, int elixirReward, int elixirMultiplier, UnityEngine.Events.UnityAction hidePanel)
    {
        SoundManager.Instance.PlayButtonClickSound();

        hidePanel.Invoke();

        CollectGameCoins(coins, multiplier);
        CollectElixir(elixirReward, elixirMultiplier);
        ResetGameplaySessionData();
        LevelManager.Instance.LoadNextLevel();
        PlayerUpgradeSystem.ResetProgressionStateToDefaults(LevelManager.Instance.activeLevelInstance.upgradeScreenConfig);
        gameViewScreen.ResetSessionDataForNextLevel();
        Time.timeScale = 1f;
        ShowHomeScreen(true);
    }

    private void ResetGameplaySessionData()
    {
        PlayerXpSystem.Instance.ResetSessionData();
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        WeaponRotator.SetGameplayMotionEnabled(false);
    }

    private static void CollectGameCoins(int coins, int multiplier)
    {
        long collectedCoinTotal = (long)Mathf.Max(0, coins) * Mathf.Max(1, multiplier);
        int collectedCoins = (int)System.Math.Min(collectedCoinTotal, int.MaxValue);
        if (collectedCoins > 0)
        {
            PlayerCurrencySystem.AddCoins(collectedCoins);
        }
    }

    private static void CollectElixir(int elixirReward, int multiplier)
    {
        int collectedElixir = Mathf.Max(0, elixirReward) * Mathf.Max(1, multiplier);
        if (collectedElixir > 0)
        {
            PlayerCurrencySystem.AddElixir(collectedElixir);
        }
    }
}
