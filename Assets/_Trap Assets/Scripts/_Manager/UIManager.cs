using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
    // [SerializeField] private Button playButton;

    private BottomHudView bottomHudController;
    private GameViewScreen gameViewScreen;
    private SettingPanelView settingPanelView;
    private WinPreviewPanel winPreviewPanelView;
    private FailPreviewPanel failPreviewPanelView;
    private GameObject currentScreenPanel;
    private bool settingsOpenedFromGameView;
    private float timeScaleBeforeSettings = 1f;

    private void Awake()
    {
        Instance = this;
        // BindFlowButtons();

        if (bottomHudPanel != null)
        {
            bottomHudController = bottomHudPanel.GetComponent<BottomHudView>();
            if (bottomHudController == null)
            {
                bottomHudController = bottomHudPanel.AddComponent<BottomHudView>();
            }
        }

        if (gameViewPanel != null)
        {
            gameViewScreen = gameViewPanel.GetComponent<GameViewScreen>();
            if (gameViewScreen == null)
            {
                gameViewScreen = gameViewPanel.AddComponent<GameViewScreen>();
            }
        }

        if (currencyView == null)
        {
            currencyView = FindFirstObjectByType<CurrencyView>();
        }

        if (settingPanel != null)
        {
            settingPanelView = settingPanel.GetComponentInChildren<SettingPanelView>(true);
        }

        if (winPreviewPanel != null)
        {
            winPreviewPanelView = winPreviewPanel.GetComponent<WinPreviewPanel>();
        }

        if (failPreviewPanel != null)
        {
            failPreviewPanelView = failPreviewPanel.GetComponent<FailPreviewPanel>();
        }

        ShowHomeScreen();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowHomeScreen()
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        ShowScreen(homeScreenPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.HomeButtonIndex);
        SoundManager.Instance.PlayButtonClickSound();
    }

    public void ShowUpgradeScreen()
    {
        ShowScreen(upgradeScreenPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.UpgradeButtonIndex);
        SoundManager.Instance.PlayButtonClickSound();
    }

    public void ShowCardScreen()
    {
        /*  ShowScreen(cardViewPanel, true);
         bottomHudController?.SetSelectedButton(BottomHudView.CardButtonIndex); */
    }

    public void ShowShopScreen()
    {
        ShowScreen(shopPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.ShopButtonIndex);
    }

    public void ShowSettingsScreen()
    {
        settingsOpenedFromGameView = currentScreenPanel == gameViewPanel;
        settingPanelView?.ConfigureOpenContext(settingsOpenedFromGameView);

        if (settingsOpenedFromGameView)
        {
            timeScaleBeforeSettings = Time.timeScale;
            Time.timeScale = 0f;
            WeaponRotator.SetGameplayMotionEnabled(false);
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
        ShowFailPreview(gameViewScreen != null ? gameViewScreen.InGameCoins : 0);
    }

    public void StartGame()
    {
        ShowScreen(gameViewPanel, false);
        WeaponRotator.SetGameplayMotionEnabled(true);
        gameViewScreen?.StartGameplay();
    }

    public void RefreshGameSceneWeaponUnlockState()
    {
        gameViewScreen?.RefreshSceneWeaponUnlockState();
    }

    public void ShowWinPreview(int coins)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        SetPanelActive(failPreviewPanel, false);
        winPreviewPanelView?.Show(
            coins,
            () => CollectGameCoinsAndReturnHome(coins, 1, winPreviewPanelView.Hide),
            () => CollectGameCoinsAndReturnHome(coins, 2, winPreviewPanelView.Hide));
    }

    public void ShowFailPreview(int coins)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        SetPanelActive(winPreviewPanel, false);
        failPreviewPanelView?.Show(
            coins,
            () => CollectGameCoinsAndReturnHome(coins, 1, failPreviewPanelView.Hide),
            () => CollectGameCoinsAndReturnHome(coins, 2, failPreviewPanelView.Hide));
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

        SetPanelActive(homeScreenPanel, activePanel == homeScreenPanel);
        SetPanelActive(upgradeScreenPanel, activePanel == upgradeScreenPanel);
        SetPanelActive(cardViewPanel, activePanel == cardViewPanel);
        SetPanelActive(shopPanel, activePanel == shopPanel);
        SetPanelActive(gameViewPanel, activePanel == gameViewPanel);
        SetPanelActive(bottomHudPanel, showBottomHud);
        SetPanelActive(settingPanel, activePanel == settingPanel);
        SetPanelActive(winPreviewPanel, false);
        SetPanelActive(failPreviewPanel, false);
        UpdateGameViewBGImageVisibility();
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    private void UpdateGameViewBGImageVisibility()
    {
        // bool isSettingsOpen = settingPanel != null && settingPanel.activeSelf;
        bool shouldShowBGImage = /* !isSettingsOpen && */ currentScreenPanel == gameViewPanel || currentScreenPanel == homeScreenPanel;
        currencyView?.SetGameViewBGImageVisible(shouldShowBGImage);
        currencyView?.SetUpgradeScreenBGImageVisible(currentScreenPanel == upgradeScreenPanel);
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
    }

    private void CollectGameCoinsAndReturnHome(int coins, int multiplier, UnityEngine.Events.UnityAction hidePanel)
    {
        hidePanel?.Invoke();

        long collectedCoinTotal = (long)Mathf.Max(0, coins) * Mathf.Max(1, multiplier);
        int collectedCoins = (int)System.Math.Min(collectedCoinTotal, int.MaxValue);
        if (collectedCoins > 0)
        {
            PlayerCurrencySystem.AddCoins(collectedCoins);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
