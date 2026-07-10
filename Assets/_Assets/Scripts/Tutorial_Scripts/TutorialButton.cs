using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    public ButtonClickType _buttonClickType;

    public static event Action<ButtonClickType> OnButtonClicked;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        OnButtonClicked?.Invoke(_buttonClickType);
    }
}
