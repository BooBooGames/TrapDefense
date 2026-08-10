using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public bool IsWeaponMovementAllowed =>
        !TutorialManager.Instance.IsTutorialDone(TutorialType.WeaponDragSystem) ||

        (currentScreenPanel == gameViewPanel &&
        IsPanelActive(gameViewPanel) &&
        !IsPanelActive(settingPanel) &&
        !IsPanelActive(winPreviewPanel) &&
        !IsPanelActive(failPreviewPanel) &&
        !IsPanelActive(chestPreviewPanel) &&
        !IsPanelActive(evolutionScreenPanel) &&
        !IsPanelActive(ageChangePanel) &&
        !IsPanelActive(cardViewPanel) &&
        (xSpeedPanel == null || !IsPanelActive(xSpeedPanel._mainContainer)) &&
        (perksCardInfoPanel == null || !IsPanelActive(perksCardInfoPanel.gameObject)) &&
        (summonScreenView == null || !IsPanelActive(summonScreenView.gameObject)));

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
    [SerializeField] private SummonScreenView summonScreenView;
    [SerializeField] private xSpeedPanel xSpeedPanel;
    [SerializeField] private Image damageImage;
    [SerializeField][Range(0f, 1f)] private float damageFlashMaxAlpha = 0.55f;
    [SerializeField][Min(0.01f)] private float damageFlashFadeInDuration = 0.08f;
    [SerializeField][Min(0.01f)] private float damageFlashFadeOutDuration = 0.45f;
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
    private bool xSpeedPanelOpenedFromGameView;
    private bool didUseReviveInThisRun = false;

    private float timeScaleBeforeSettings = 1f;
    private Coroutine damageFlashRoutine;

    private void Awake()
    {
        Instance = this;
        GameplaySpeedSystem.Initialize();
        // BindFlowButtons();

        bottomHudController = bottomHudPanel.GetComponent<BottomHudView>();
        gameViewScreen = gameViewPanel.GetComponent<GameViewScreen>();
        upgradeScreenView = upgradeScreenPanel.GetComponent<UpgradeScreenView>();
        settingPanelView = settingPanel.GetComponentInChildren<SettingPanelView>(true);
        winPreviewPanelView = winPreviewPanel.GetComponent<WinPreviewPanel>();
        failPreviewPanelView = failPreviewPanel.GetComponent<FailPreviewPanel>();
        evolutionScreenView = evolutionScreenPanel.GetComponent<EvolutionScreenView>();
        currencyView.BindEvolutionButton(ShowEvolutionPanel);
        BindXSpeedPanel();
        PlayerUpgradeSystem.Initialize(GetActiveLevelUpgradeConfig());
        CloseXSpeedPanel(false);
        ClosePerksCardInfoPanel();
        CloseSummonScreen();
        SetDamageImageAlpha(0f);

        ShowHomeScreen(true);

        if (FTUEController.IsCompleted)
        {
        }
        else
        {
            //ShowFTUE();
        }
    }

    private void Start()
    {
        if (Utils.IsTutorialDone(TutorialType.MainUpgrades)) return;

        // Need to trigger this on first time game lose, the scene reloads every time the player goes back to the home screen, so this will fire at all scene reloads
        if (!Utils.IsTutorialDone(TutorialType.FirstPlay)) return;

        TutorialManager.RegisterEvent(TutorialEventType.LostGameForFirstTime);
    }

    private void Update()
    {
        GameplaySpeedSystem.Tick();

        if (currentScreenPanel == gameViewPanel
            && Time.timeScale > 0f
            && (xSpeedPanel == null || !xSpeedPanel._mainContainer.activeSelf))
        {
            GameplaySpeedSystem.ApplyCurrentSpeedToTimeScale(true);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            GameplaySpeedSystem.Flush();
        }
    }

    private void OnApplicationQuit()
    {
        GameplaySpeedSystem.Flush();
    }

    private void OnDestroy()
    {
        GameplaySpeedSystem.Flush();

        if (xSpeedPanel != null)
        {
            xSpeedPanel.Unbind();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowHomeScreen(bool fromFTUE = false)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);

        RvManager.ToggleRVsSpawn(false);
        GameplaySpeedSystem.ToggleFreeSpeedBoostTimer(false);

        ShowScreen(homeScreenPanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.HomeButtonIndex);
        if (!fromFTUE) SoundManager.PlayButtonClickSound();
    }

    public void ShowUpgradeScreen()
    {
        ShowScreen(upgradeScreenPanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.UpgradeButtonIndex);
        SoundManager.PlayButtonClickSound();
    }

    public void ShowCardScreen()
    {

        ShowScreen(cardUpgradePanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.CardButtonIndex);
        SoundManager.PlayButtonClickSound();
    }

    public void ShowShopScreen()
    {
        ShowScreen(shopPanel, true);
        bottomHudController.SetSelectedButton(BottomHudView.ShopButtonIndex);
        SoundManager.PlayButtonClickSound();
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
        ShowFailPreview(gameViewScreen.InGameCoins, isFromSettings: true);
    }

    public void StartGame()
    {
        if (Utils.IsTutorialDone(TutorialType.MainUpgrades))
        {
            if (!Utils.IsTutorialDone(TutorialType.WeaponDragSystem))
            {
                TutorialManager.RegisterEvent(TutorialEventType.SecondPlay);
            }
        }

        ShowScreen(gameViewPanel, false);
        WeaponRotator.SetGameplayMotionEnabled(true);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(true);

        didUseReviveInThisRun = false;

        gameViewScreen.StartGameplay();
    }

    public void RefreshGameSceneWeaponUnlockState()
    {
        gameViewScreen.RefreshSceneWeaponUnlockState();
    }

    public void ShowXSpeedPanel()
    {
        if (xSpeedPanel == null)
        {
            return;
        }

        xSpeedPanelOpenedFromGameView = currentScreenPanel == gameViewPanel;

        if (xSpeedPanelOpenedFromGameView)
        {
            Time.timeScale = 0f;
            WeaponRotator.SetGameplayMotionEnabled(false);
            WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        }

        xSpeedPanel.RefreshState();
        SetPanelActive(xSpeedPanel._mainContainer, true);
        SoundManager.PlayButtonClickSound();
    }

    public void CloseXSpeedPanel(bool playSound = true)
    {
        if (xSpeedPanel == null)
        {
            return;
        }

        SetPanelActive(xSpeedPanel._mainContainer, false);
        ResumeGameIfXSpeedPanelPaused();

        if (playSound)
        {
            SoundManager.PlayButtonClickSound();
        }
    }

    public void ShowWinPreview(int coins, int elixirReward = 0)
    {
        SoundManager.PlayAudio(AudioType.Win_Level);
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);
        SetPanelActive(failPreviewPanel, false);
        winPreviewPanelView.Show(
            coins,
            elixirReward,
            () => CollectGameCoinsAndStartNextLevel(coins, 1, elixirReward, 1, winPreviewPanelView.Hide),
            multiplier => CollectGameCoinsAndStartNextLevel(coins, multiplier, elixirReward, 2, winPreviewPanelView.Hide));
    }

    public void ShowFailPreview(int coins, int elixirReward = 0, bool isFromSettings = false)
    {
        AudioManager.StopAudio();

        SetPanelActive(winPreviewPanel, false);

        if(isFromSettings)
        {
            OnFailLevel(1, coins, elixirReward);
            return;
        }

        failPreviewPanelView.Show(
            coins,
            elixirReward,
            () => OnFailLevel(1, coins, elixirReward),
            () => TryClaim2xOnLevelFail(coins, elixirReward),
            HandleReviveButtonClickOnFail);
    }

    public void ShowPerksCardInfoPanel(PowerCardDefinition cardData, Sprite cardBackgroundSprite)
    {
        if (perksCardInfoPanel == null || cardData == null)
        {
            return;
        }

        perksCardInfoPanel.Show(cardData, cardBackgroundSprite, ClosePerksCardInfoPanel);
    }

    public void ClosePerksCardInfoPanel()
    {
        if (perksCardInfoPanel == null)
        {
            return;
        }

        perksCardInfoPanel.Hide();
    }

    public void ShowSummonScreen(
        PowerCardDefinition cardData,
        Sprite cardBackgroundSprite,
        Func<bool> canSummonX1,
        Func<bool> canSummonX10,
        UnityAction onSummonX1,
        UnityAction onSummonX10,
        UnityAction onContinue,
        UnityAction onRevealComplete)
    {
        if (summonScreenView == null || cardData == null)
        {
            return;
        }

        summonScreenView.Show(
            cardData,
            cardBackgroundSprite,
            canSummonX1,
            canSummonX10,
            onSummonX1,
            onSummonX10,
            onContinue,
            onRevealComplete);
    }

    public void RefreshSummonScreenButtonStates()
    {
        if (summonScreenView == null || !summonScreenView.gameObject.activeSelf)
        {
            return;
        }

        summonScreenView.RefreshButtonStates();
    }

    public void CloseSummonScreen()
    {
        if (summonScreenView == null)
        {
            return;
        }

        summonScreenView.Hide();
    }

    public void PlayDamageScreenFlash()
    {
        if (damageImage == null)
        {
            return;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        if (!damageImage.gameObject.activeSelf)
        {
            damageImage.gameObject.SetActive(true);
        }

        damageFlashRoutine = StartCoroutine(DamageScreenFlashRoutine());
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

        SetPanelActive(evolutionScreenPanel, activePanel == evolutionScreenPanel);

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
        CloseXSpeedPanel(false);
        SetPanelActive(cardUpgradePanel, activePanel == cardUpgradePanel);
        SetPanelActive(ageChangePanel, false);
        SetPanelActive(FTUEPanel, false);
        ClosePerksCardInfoPanel();
        CloseSummonScreen();
        UpdateGameViewBGImageVisibility();
    }

    private void TryClaim2xOnLevelFail(int coins, int elixirReward)
    {
        string eventName = "claim_2x_reward";

        HCSDKManager.INSTANCE.DisplayRV(eventName, () =>
        {
            AnalyticsManager.ShowRVEvent(eventName);
            OnFailLevel(2, coins, elixirReward);
        });
    }

    private void HandleReviveButtonClickOnFail()
    {
        string eventName = "revive";
        HCSDKManager.INSTANCE.DisplayRV(eventName, () =>
        {
            didUseReviveInThisRun = true;
            gameViewScreen.Revive();
            failPreviewPanelView.Hide();
            AnalyticsManager.ShowRVEvent(eventName);

            if (gameViewScreen.ShouldPauseOnGameOver())
            {
                Time.timeScale = 1f;
            }
        });
    }

    private void OnFailLevel(int pRewardMultiplier, int coins, int elixirReward)
    {
        WeaponRotator.SetGameplayMotionEnabled(false);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(false);

        CollectGameCoinsAndReturnHome(coins, pRewardMultiplier, elixirReward, pRewardMultiplier, failPreviewPanelView.Hide);
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
        CloseXSpeedPanel(false);
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
        //if (isActive)
        //{
        //    Debug.Log($"Turn on {panel.name}");
        //}
        //else
        //{
        //    Debug.Log($"Turn off {panel.name}");

        //}
        panel.SetActive(isActive);
    }

    private static bool IsPanelActive(GameObject panel)
    {
        return panel != null && panel.activeSelf;
    }

    private void BindXSpeedPanel()
    {
        if (xSpeedPanel == null)
        {
            return;
        }

        xSpeedPanel.Bind(ActivateFreeXSpeedBoost, ActivateUnlimitedXSpeedBoost, () => CloseXSpeedPanel());
    }

    private void ActivateFreeXSpeedBoost()
    {
        GameplaySpeedSystem.ActivateFreeBoost();
        xSpeedPanel.RefreshState();
        PauseGameplayIfXSpeedPanelIsOpen();
    }

    private void ActivateUnlimitedXSpeedBoost()
    {
        GameplaySpeedSystem.ActivateUnlimitedBoost();
        xSpeedPanel.RefreshState();
        PauseGameplayIfXSpeedPanelIsOpen();
    }

    private void PauseGameplayIfXSpeedPanelIsOpen()
    {
        if (xSpeedPanelOpenedFromGameView && xSpeedPanel != null && xSpeedPanel.gameObject.activeSelf)
        {
            Time.timeScale = 0f;
        }
    }

    private void ResumeGameIfXSpeedPanelPaused()
    {
        if (!xSpeedPanelOpenedFromGameView)
        {
            return;
        }

        xSpeedPanelOpenedFromGameView = false;

        if (currentScreenPanel == gameViewPanel)
        {
            GameplaySpeedSystem.ApplyCurrentSpeedToTimeScale(true);
            WeaponRotator.SetGameplayMotionEnabled(true);
            WeaponUpgradeController.SetGameplayAnimationsEnabled(true);
            return;
        }

        Time.timeScale = 1f;
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
        UpgradeScreenConfig config = UpgradeScreenConfig.Resolve(GetActiveLevelUpgradeConfig());
        PlayerUpgradeSystem.Initialize(config);
        evolutionScreenView.ShowEvolutionBackground(config);
        SoundManager.PlayButtonClickSound();
    }

    private static UpgradeScreenConfig GetActiveLevelUpgradeConfig()
    {
        LevelManager levelManager = LevelManager.Instance;
        if (levelManager == null || levelManager.activeLevelInstance == null)
        {
            return null;
        }

        return levelManager.activeLevelInstance.upgradeScreenConfig;
    }

    private void ResumeGameIfSettingsPaused()
    {
        if (!settingsOpenedFromGameView)
        {
            return;
        }

        settingsOpenedFromGameView = false;
        if (currentScreenPanel == gameViewPanel)
        {
            GameplaySpeedSystem.ApplyCurrentSpeedToTimeScale(true);
        }
        else
        {
            Time.timeScale = timeScaleBeforeSettings <= 0f ? 1f : timeScaleBeforeSettings;
        }

        WeaponRotator.SetGameplayMotionEnabled(currentScreenPanel == gameViewPanel);
        WeaponUpgradeController.SetGameplayAnimationsEnabled(currentScreenPanel == gameViewPanel);
    }

    private IEnumerator DamageScreenFlashRoutine()
    {
        yield return FadeDamageImageAlpha(0f, damageFlashMaxAlpha, damageFlashFadeInDuration);
        yield return FadeDamageImageAlpha(damageFlashMaxAlpha, 0f, damageFlashFadeOutDuration);
        SetDamageImageAlpha(0f);
        damageFlashRoutine = null;
    }

    private IEnumerator FadeDamageImageAlpha(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            float progress = Mathf.Clamp01(elapsed / safeDuration);
            SetDamageImageAlpha(Mathf.Lerp(fromAlpha, toAlpha, progress));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        SetDamageImageAlpha(toAlpha);
    }

    private void SetDamageImageAlpha(float alpha)
    {
        if (damageImage == null)
        {
            return;
        }

        Color color = damageImage.color;
        color.a = Mathf.Clamp01(alpha);
        damageImage.color = color;
    }

    private void CollectGameCoinsAndReturnHome(int coins, int multiplier, int elixirReward, int elixirMultiplier, UnityAction hidePanel)
    {
        SoundManager.PlayButtonClickSound();
        hidePanel.Invoke();

        CollectGameCoins(coins, multiplier);
        CollectElixir(elixirReward, elixirMultiplier);

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void CollectGameCoinsAndStartNextLevel(int coins, int multiplier, int elixirReward, int elixirMultiplier, UnityAction hidePanel)
    {
        SoundManager.PlayButtonClickSound();

        hidePanel.Invoke();

        CollectGameCoins(coins, multiplier);
        CollectElixir(elixirReward, elixirMultiplier);
        ResetGameplaySessionData();
        LevelManager.Instance.LoadNextLevel();
        PlayerCurrencySystem.ResetCoins();
        PlayerUpgradeSystem.ResetProgressionStateToDefaults(GetActiveLevelUpgradeConfig());
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
