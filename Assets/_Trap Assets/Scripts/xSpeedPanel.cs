using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class xSpeedPanel : MonoBehaviour
{
    private const string FreeButtonDefaultText = "FREE";
    private const string UnlimitedButtonDefaultText = "INR1550"; // Do the actual IAP price here
    private const string BoostActiveText = "ACTIVE";

    public GameObject _mainContainer;

    public Button freeButton, unlimitedButton, closeButton;
    public TextMeshProUGUI freeButtonText;
    public TextMeshProUGUI unlimitedButtonText;

    [SerializeField] private GameObject _freeButtonRvIconObject;
    [SerializeField] private GameObject _bonusInfoContainer;
    [SerializeField] private TextMeshProUGUI _remainingDurationText;
    [SerializeField] private GameObject _infiniteDurationIcon;

    private Action onFreeButtonClicked;
    private Action onUnlimitedButtonClicked;
    private Action onCloseButtonClicked;

    private int lastDisplayedFreeBoostSeconds = -1;
    private bool lastDisplayedFreeBoostActive;
    private bool lastDisplayedUnlimitedBoostActive;

    private void Awake()
    {
        ResolveFreeButtonText();
    }

    private void OnEnable()
    {
        RefreshState();
    }

    private void Update()
    {
        RefreshState();
    }

    public void Bind(Action freeClicked, Action unlimitedClicked, Action closeClicked)
    {
        Unbind();

        onFreeButtonClicked = freeClicked;
        onUnlimitedButtonClicked = unlimitedClicked;
        onCloseButtonClicked = closeClicked;

        if (freeButton != null)
        {
            freeButton.onClick.AddListener(HandleFreeButtonClicked);
        }

        if (unlimitedButton != null)
        {
            unlimitedButton.onClick.AddListener(HandleUnlimitedButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleCloseButtonClicked);
        }

        RefreshState();
    }

    public void RefreshState()
    {
        GameplaySpeedSystem.RefreshSavedState();
        ResolveFreeButtonText();

        bool unlimitedActive = GameplaySpeedSystem.UnlimitedBoostActive;
        bool freeActive = !unlimitedActive && GameplaySpeedSystem.FreeBoostActive;
        _bonusInfoContainer.SetActive(freeActive || unlimitedActive);

        if (freeButton != null)
        {
            freeButton.transform.parent.gameObject.SetActive(!unlimitedActive);

            freeButton.interactable = !freeActive;
            _freeButtonRvIconObject.SetActive(!freeActive);
            _remainingDurationText.gameObject.SetActive(freeActive);
        }

        if (unlimitedButton != null)
        {
            unlimitedButton.transform.parent.gameObject.SetActive(!freeActive);

            unlimitedButton.interactable = !unlimitedActive;

            _infiniteDurationIcon.SetActive(unlimitedActive);
        }

        int remainingSeconds = GameplaySpeedSystem.FreeBoostRemainingSeconds;

        if (freeActive != lastDisplayedFreeBoostActive
            || unlimitedActive != lastDisplayedUnlimitedBoostActive
            || remainingSeconds != lastDisplayedFreeBoostSeconds)
        {
            if (freeActive)
            {
                _remainingDurationText.text = FormatRemainingTime(remainingSeconds);
            }

            freeButtonText.text = freeActive ? BoostActiveText : FreeButtonDefaultText;

            // TODO: THIS WILL WORK WITH IAPS, SO MAKE SURE TO FETCH THE PRICE FROM THE STORE, WHEN NOT ACTIVE
            unlimitedButtonText.text = unlimitedActive ? BoostActiveText : UnlimitedButtonDefaultText;

            lastDisplayedFreeBoostActive = freeActive;
            lastDisplayedUnlimitedBoostActive = unlimitedActive;
            lastDisplayedFreeBoostSeconds = remainingSeconds;
        }
    }

    public void Unbind()
    {
        if (freeButton != null)
        {
            freeButton.onClick.RemoveListener(HandleFreeButtonClicked);
        }

        if (unlimitedButton != null)
        {
            unlimitedButton.onClick.RemoveListener(HandleUnlimitedButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseButtonClicked);
        }

        onFreeButtonClicked = null;
        onUnlimitedButtonClicked = null;
        onCloseButtonClicked = null;
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void HandleFreeButtonClicked()
    {
        onFreeButtonClicked?.Invoke();
        RefreshState();
    }

    private void HandleUnlimitedButtonClicked()
    {
        onUnlimitedButtonClicked?.Invoke();
        RefreshState();
    }

    private void HandleCloseButtonClicked()
    {
        onCloseButtonClicked?.Invoke();
    }

    private void ResolveFreeButtonText()
    {
        if (freeButtonText != null || freeButton == null)
        {
            return;
        }

        freeButtonText = freeButton.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private static string FormatRemainingTime(int remainingSeconds)
    {
        int clampedSeconds = Mathf.Max(0, remainingSeconds);
        int minutes = clampedSeconds / 60;
        int seconds = clampedSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
