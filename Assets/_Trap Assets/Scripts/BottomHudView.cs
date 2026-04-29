using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomHudView : MonoBehaviour
{
    public const int LockButtonIndex = 0;
    public const int UpgradeButtonIndex = 1;
    public const int HomeButtonIndex = 2;
    public const int CardButtonIndex = 3;
    public const int ShopButtonIndex = 4;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject selectedButtonImage;
    [SerializeField] private List<Button> bottomHudButtons = new List<Button>();
    [SerializeField] private float selectedButtonYPosition = 0f;
    [SerializeField][Min(1f)] private float selectedButtonScale = 1.2f;
    [SerializeField][Min(0.1f)] private float unselectedButtonScale = 1f;

    private readonly List<float> defaultBottomButtonYPositions = new List<float>();

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        AutoResolveReferences();
        CacheBottomHudButtonDefaults();
        BindNavigationButtons();
    }

    public void SetSelectedButton(int selectedIndex)
    {
        if (bottomHudButtons == null)
        {
            return;
        }

        for (int i = 0; i < bottomHudButtons.Count; i++)
        {
            Button button = bottomHudButtons[i];
            if (button == null)
            {
                continue;
            }

            RectTransform buttonRect = button.transform as RectTransform;
            if (buttonRect == null)
            {
                continue;
            }

            bool isSelected = i == selectedIndex;
            Vector2 anchoredPosition = buttonRect.anchoredPosition;
            float defaultY = i < defaultBottomButtonYPositions.Count ? defaultBottomButtonYPositions[i] : anchoredPosition.y;
            anchoredPosition.y = isSelected ? selectedButtonYPosition : defaultY;
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.localScale = Vector3.one * (isSelected ? selectedButtonScale : unselectedButtonScale);

            if (!isSelected || selectedButtonImage == null)
            {
                continue;
            }

            RectTransform selectionRect = selectedButtonImage.transform as RectTransform;
            if (selectionRect != null)
            {
                selectionRect.position = buttonRect.position;
            }
            else
            {
                selectedButtonImage.transform.position = button.transform.position;
            }
        }
    }

    private void BindNavigationButtons()
    {
        BindBottomHudButton(LockButtonIndex, HandleLockButtonClicked);
        BindBottomHudButton(UpgradeButtonIndex, () => uiManager?.ShowUpgradeScreen());
        BindBottomHudButton(HomeButtonIndex, () => uiManager?.ShowHomeScreen());
        BindBottomHudButton(CardButtonIndex, () => uiManager?.ShowCardScreen());
        BindBottomHudButton(ShopButtonIndex, () => uiManager?.ShowShopScreen());
    }

    private void BindBottomHudButton(int buttonIndex, UnityEngine.Events.UnityAction callback)
    {
        if (bottomHudButtons == null || buttonIndex < 0 || buttonIndex >= bottomHudButtons.Count)
        {
            return;
        }

        Button button = bottomHudButtons[buttonIndex];
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(() => HandleBottomHudButtonClicked(buttonIndex, callback));
    }

    private void HandleBottomHudButtonClicked(int buttonIndex, UnityEngine.Events.UnityAction callback)
    {
        SetSelectedButton(buttonIndex);
        callback?.Invoke();
    }

    private void HandleLockButtonClicked()
    {
    }

    private void CacheBottomHudButtonDefaults()
    {
        defaultBottomButtonYPositions.Clear();

        if (bottomHudButtons == null)
        {
            return;
        }

        for (int i = 0; i < bottomHudButtons.Count; i++)
        {
            RectTransform buttonRect = bottomHudButtons[i] != null ? bottomHudButtons[i].transform as RectTransform : null;
            defaultBottomButtonYPositions.Add(buttonRect != null ? buttonRect.anchoredPosition.y : 0f);
        }
    }

    private void AutoResolveReferences()
    {
        if (selectedButtonImage == null)
        {
            Transform selectedImageTransform = transform.Find("Selected Image");
            if (selectedImageTransform != null)
            {
                selectedButtonImage = selectedImageTransform.gameObject;
            }
        }

        if (bottomHudButtons != null && bottomHudButtons.Count > 0)
        {
            return;
        }

        bottomHudButtons = new List<Button>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                bottomHudButtons.Add(button);
            }
        }
    }
}
