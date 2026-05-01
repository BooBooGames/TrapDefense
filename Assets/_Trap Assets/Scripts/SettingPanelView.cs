using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingPanelView : MonoBehaviour
{
    public Sprite musicOnSprite, musicOffSprite;
    public Sprite soundOnSprite, soundOffSprite;
    public Sprite hapticOnSprite, hapticOffSprite;
    public Button closeButton, musicButton, soundButton, hapticButton;
    public Image musicButtonImage, soundButtonImage, hapticButtonImage;

    private void Awake()
    {
        GameSettingsSystem.Initialize();
        BindButtons();
    }

    void Start()
    {
        musicButtonImage = musicButton.GetComponent<Image>();
        soundButtonImage = soundButton.GetComponent<Image>();
        hapticButtonImage = hapticButton.GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameSettingsSystem.SettingsChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        GameSettingsSystem.SettingsChanged -= RefreshUI;
    }

    private void BindButtons()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        musicButton.onClick.AddListener(OnMusicButtonClicked);
        soundButton.onClick.AddListener(OnSoundButtonClicked);
        hapticButton.onClick.AddListener(OnHapticButtonClicked);
    }

    private void RefreshUI()
    {
        // musicButton.interactable = GameSettingsSystem.MusicEnabled;
        // soundButton.interactable = GameSettingsSystem.SoundEnabled;
        // hapticButton.interactable = GameSettingsSystem.HapticEnabled;

        if (musicButtonImage != null)
            musicButtonImage.sprite = GameSettingsSystem.MusicEnabled ? musicOnSprite : musicOffSprite;

        if (soundButtonImage != null)
            soundButtonImage.sprite = GameSettingsSystem.SoundEnabled ? soundOnSprite : soundOffSprite;

        if (hapticButtonImage != null)
            hapticButtonImage.sprite = GameSettingsSystem.HapticEnabled ? hapticOnSprite : hapticOffSprite;
    }

    public void OnCloseButtonClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseSettingsScreen();
            return;
        }

        gameObject.SetActive(false);
    }

    public void OnMusicButtonClicked()
    {
        GameSettingsSystem.ToggleMusic();
    }

    public void OnSoundButtonClicked()
    {
        GameSettingsSystem.ToggleSound();
    }

    public void OnHapticButtonClicked()
    {
        GameSettingsSystem.ToggleHaptic();
    }
}

public static class GameSettingsSystem
{
    private static bool isInitialized;

    private static bool musicEnabled;
    private static bool soundEnabled;
    private static bool hapticEnabled;

    public static event Action SettingsChanged;

    public static bool MusicEnabled => musicEnabled;
    public static bool SoundEnabled => soundEnabled;
    public static bool HapticEnabled => hapticEnabled;

    public static void Initialize()
    {
        if (isInitialized) return;

        SaveGameData data = GameSaveSystem.Load();

        musicEnabled = data.musicEnabled;
        soundEnabled = data.soundEnabled;
        hapticEnabled = data.hapticEnabled;

        isInitialized = true;
    }

    public static void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        Apply();
    }

    public static void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        Apply();
    }

    public static void ToggleHaptic()
    {
        hapticEnabled = !hapticEnabled;
        Apply();
    }

    private static void Apply()
    {
        SaveGameData data = GameSaveSystem.Load();

        data.musicEnabled = musicEnabled;
        data.soundEnabled = soundEnabled;
        data.hapticEnabled = hapticEnabled;

        GameSaveSystem.Save(data);

        SettingsChanged?.Invoke();
    }
}
