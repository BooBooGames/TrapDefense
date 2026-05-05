using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChestInfo : MonoBehaviour
{
    public TextMeshProUGUI typeLabel;
    public Image iconImage, emptySlotImage;
    public GameObject timerHolder;
    public TextMeshProUGUI unlockTimeLabel;
    public Button unlockButton, rvTimeSkipButton, openButton;

    public void Bind(
        ChestSlotSaveData slotData,
        ChestDefinition chestData,
        UnityAction onUnlockClicked,
        UnityAction onOpenClicked,
        UnityAction onRvClicked)
    {
        bool hasChest = slotData != null && slotData.hasChest;

        if (!hasChest)
        {
            BindEmptySlot();
            return;
        }

        ChestSlotState state = (ChestSlotState)slotData.state;
        bool isUnlocking = state == ChestSlotState.Unlocking;

        gameObject.SetActive(true);

        if (typeLabel != null)
        {
            typeLabel.text = chestData != null ? chestData.displayName : "Chest";
        }

        if (emptySlotImage != null)
        {
            emptySlotImage.gameObject.SetActive(false);
        }

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);
            if (chestData != null && chestData.icon != null)
            {
                iconImage.sprite = chestData.icon;
            }
        }

        if (openButton == null)
        {
            SetButton(
                unlockButton,
                state == ChestSlotState.Locked || state == ChestSlotState.ReadyToOpen,
                state == ChestSlotState.ReadyToOpen ? onOpenClicked : onUnlockClicked);
            SetButtonLabel(unlockButton, state == ChestSlotState.ReadyToOpen ? "Open" : "Unlock");
        }
        else
        {
            SetButton(unlockButton, state == ChestSlotState.Locked, onUnlockClicked);
            SetButton(openButton, state == ChestSlotState.ReadyToOpen, onOpenClicked);
            SetButtonLabel(unlockButton, "Unlock");
            SetButtonLabel(openButton, "Open");
        }

        SetButton(rvTimeSkipButton, isUnlocking, onRvClicked);

        if (timerHolder != null)
        {
            timerHolder.SetActive(isUnlocking);
        }

        if (unlockTimeLabel != null)
        {
            unlockTimeLabel.gameObject.SetActive(isUnlocking);
            if (isUnlocking)
            {
                unlockTimeLabel.text = FormatRemainingTime(slotData.unlockEndUtc);
            }
        }
    }

    public void RefreshTimer(ChestSlotSaveData slotData)
    {
        if (slotData == null || (ChestSlotState)slotData.state != ChestSlotState.Unlocking)
        {
            return;
        }

        if (unlockTimeLabel != null)
        {
            unlockTimeLabel.text = FormatRemainingTime(slotData.unlockEndUtc);
        }
    }

    private void BindEmptySlot()
    {
        gameObject.SetActive(true);

        if (typeLabel != null)
        {
            typeLabel.text = "Empty Slot";
        }

        if (emptySlotImage != null)
        {
            emptySlotImage.gameObject.SetActive(true);
        }

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        if (timerHolder != null)
        {
            timerHolder.SetActive(false);
        }

        if (unlockTimeLabel != null)
        {
            unlockTimeLabel.gameObject.SetActive(false);
        }

        SetButton(unlockButton, false, null);
        SetButton(rvTimeSkipButton, false, null);
        SetButton(openButton, false, null);
    }

    private static void SetButton(Button button, bool isVisible, UnityAction onClicked)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(isVisible);
        button.onClick.RemoveAllListeners();

        if (onClicked != null)
        {
            button.onClick.AddListener(onClicked);
        }
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }

    private static string FormatRemainingTime(long unlockEndUtc)
    {
        long remainingSeconds = System.Math.Max(0, unlockEndUtc - HomeViewScreen.GetUtcNowSeconds());
        int totalSeconds = Mathf.CeilToInt((float)remainingSeconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        return hours > 0
            ? $"{hours:00}:{minutes:00}:{seconds:00}"
            : $"{minutes:00}:{seconds:00}";
    }
}
