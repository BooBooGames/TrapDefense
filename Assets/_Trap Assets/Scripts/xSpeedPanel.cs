using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class xSpeedPanel : MonoBehaviour
{
    private const string FreeButtonDefaultText = "FREE";

    public Button freeButton, unlimitedButton, closeButton;
    public TextMeshProUGUI freeButtonText;

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

        if (freeButton != null)
        {
            freeButton.interactable = !freeActive && !unlimitedActive;
        }

        if (unlimitedButton != null)
        {
            unlimitedButton.interactable = !unlimitedActive;
        }

        if (freeButtonText != null)
        {
            int remainingSeconds = GameplaySpeedSystem.FreeBoostRemainingSeconds;

            if (freeActive != lastDisplayedFreeBoostActive
                || unlimitedActive != lastDisplayedUnlimitedBoostActive
                || remainingSeconds != lastDisplayedFreeBoostSeconds)
            {
                freeButtonText.text = freeActive
                    ? FormatRemainingTime(remainingSeconds)
                    : FreeButtonDefaultText;

                lastDisplayedFreeBoostActive = freeActive;
                lastDisplayedUnlimitedBoostActive = unlimitedActive;
                lastDisplayedFreeBoostSeconds = remainingSeconds;
            }
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
