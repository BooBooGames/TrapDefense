using System;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeScreen : MonoBehaviour
{
    [SerializeField] private GameObject _mainContainer;
    [SerializeField] private Button _continueButton;

    public static event Action OnContinue;

    private void Start()
    {
        if (!PlayerPrefsExtension.GetBool(PlayerPrefsKeys.IS_WELCOMED, false))
        {
            Show();
        }

        _continueButton.onClick.AddListener(() =>
        {
            PlayerPrefsExtension.SetBool(PlayerPrefsKeys.IS_WELCOMED, true);
            OnContinue?.Invoke();
            Hide();
        });
    }

    private void Show()
    {
        _mainContainer.SetActive(true);
    }

    private void Hide()
    {
        _mainContainer.SetActive(false);
    }
}
