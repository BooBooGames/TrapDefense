using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject gameViewPanel;
    [SerializeField] private GameObject homeScreenPanel;
    [SerializeField] private GameObject upgradeScreenPanel;
    [SerializeField] private GameObject cardViewPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject bottomHudPanel;
    [SerializeField] private GameObject settingPanel;
    // [SerializeField] private Button playButton;
    [SerializeField] private CurrencyView currencyView;

    private BottomHudView bottomHudController;
    private GameViewScreen gameViewScreen;
    private GameObject currentScreenPanel;

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
        ShowScreen(homeScreenPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.HomeButtonIndex);
    }

    public void ShowUpgradeScreen()
    {
        ShowScreen(upgradeScreenPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.UpgradeButtonIndex);
    }

    public void ShowCardScreen()
    {
        ShowScreen(cardViewPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.CardButtonIndex);
    }

    public void ShowShopScreen()
    {
        ShowScreen(shopPanel, true);
        bottomHudController?.SetSelectedButton(BottomHudView.ShopButtonIndex);
    }

    public void ShowSettingsScreen()
    {
        SetPanelActive(settingPanel, true);
        UpdateGameViewBGImageVisibility();
    }

    public void CloseSettingsScreen()
    {
        SetPanelActive(settingPanel, false);
        UpdateGameViewBGImageVisibility();
    }

    public void StartGame()
    {
        ShowScreen(gameViewPanel, false);
        gameViewScreen?.StartGameplay();
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
}
