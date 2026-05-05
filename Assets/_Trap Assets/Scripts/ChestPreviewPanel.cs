using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChestPreviewPanel : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI coinRewardLabel, gemRewardLabel, cardCountLabel, unlockTimeLabel, unlockGemsLabel;
    public Button unlockButton, OpenWithGemsButton, closeButton;

    public void Show(
        ChestDefinition chestData,
        ChestSlotSaveData slotData,
        bool canUnlockWithTime,
        UnityAction onUnlockWithTime,
        UnityAction onUnlockWithGems)
    {
        if (chestData == null)
        {
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null && chestData.icon != null)
        {
            iconImage.sprite = chestData.icon;
        }

        if (coinRewardLabel != null)
        {
            coinRewardLabel.text = GetCoinsReward(chestData, slotData).ToString();
        }

        if (cardCountLabel != null)
        {
            cardCountLabel.text = GetCardsReward(chestData, slotData).ToString();
        }

        if (gemRewardLabel != null)
        {
            gemRewardLabel.text = GetGemsReward(chestData, slotData).ToString();
        }

        if (unlockTimeLabel != null)
        {
            unlockTimeLabel.text = HomeViewScreen.FormatDuration(chestData.unlockDurationSeconds);
        }

        if (unlockGemsLabel != null)
        {
            unlockGemsLabel.text = chestData.unlockGemCost.ToString();
        }

        SetButton(unlockButton, onUnlockWithTime);
        SetButtonInteractable(unlockButton, canUnlockWithTime);
        SetButton(OpenWithGemsButton, onUnlockWithGems);
        SetButtonInteractable(OpenWithGemsButton, true);
        SetButton(closeButton, Hide);
        SetButtonInteractable(closeButton, true);
    }

    public void Show(ChestDefinition chestData, UnityAction onUnlockWithTime, UnityAction onUnlockWithGems)
    {
        Show(chestData, null, true, onUnlockWithTime, onUnlockWithGems);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static void SetButton(Button button, UnityAction onClicked)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (onClicked != null)
        {
            button.onClick.AddListener(onClicked);
        }
    }

    private static void SetButtonInteractable(Button button, bool isInteractable)
    {
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    private static int GetCoinsReward(ChestDefinition chestData, ChestSlotSaveData slotData)
    {
        return slotData != null ? slotData.coinsReward : chestData.rewards != null ? chestData.rewards.coins : 0;
    }

    private static int GetCardsReward(ChestDefinition chestData, ChestSlotSaveData slotData)
    {
        return slotData != null ? slotData.cardsReward : chestData.rewards != null ? chestData.rewards.cards : 0;
    }

    private static int GetGemsReward(ChestDefinition chestData, ChestSlotSaveData slotData)
    {
        return slotData != null ? slotData.gemsReward : chestData.rewards != null ? chestData.rewards.gems : 0;
    }
}
