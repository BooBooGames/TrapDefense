using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class xSpeedPanel : MonoBehaviour
{
    public Button freeButton, unlimitedButton, closeButton;
    public TextMeshProUGUI freeButtonText;

    private Action onFreeButtonClicked;
    private Action onUnlimitedButtonClicked;
    private Action onCloseButtonClicked;

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
    }

    private void HandleUnlimitedButtonClicked()
    {
        onUnlimitedButtonClicked?.Invoke();
    }

    private void HandleCloseButtonClicked()
    {
        onCloseButtonClicked?.Invoke();
    }
}
