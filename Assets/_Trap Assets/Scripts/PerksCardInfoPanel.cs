using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PerksCardInfoPanel : MonoBehaviour
{
    public Sprite commonTitleSprite, rareTitleSprite, epicTitleSprite, legendaryTitleSprite;
    public Image titleImage, cardBgImage, cardImage, fillerImage;
    public TextMeshProUGUI cardNameText, cardTypeText, cardLevelText, fillerText, cardDescriptionText, upgradeDescriptionText;
    public Button upgradeButton, CloseButton;

    private PowerCardDefinition currentCardData;
    private Sprite currentCardBackgroundSprite;

    public void Show(PowerCardDefinition cardData, Sprite cardBackgroundSprite, UnityAction onClose)
    {
        if (cardData == null)
        {
            return;
        }

        gameObject.SetActive(true);
        currentCardData = cardData;
        currentCardBackgroundSprite = cardBackgroundSprite;
        RefreshCardInfo();

        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(true);
            upgradeButton.interactable = true;
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(HandleUpgradeClicked);
        }

        if (CloseButton != null)
        {
            CloseButton.onClick.RemoveAllListeners();
            CloseButton.onClick.AddListener(onClose);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void HandleUpgradeClicked()
    {
        if (currentCardData == null)
        {
            return;
        }

        PowerCardUpgradeSystem.UpgradeCard(currentCardData);
        RefreshCardInfo();
    }

    private void RefreshCardInfo()
    {
        if (currentCardData == null)
        {
            return;
        }

        int currentLevel = PowerCardUpgradeSystem.GetCardLevel(currentCardData);

        SetImage(titleImage, GetTitleSprite(currentCardData.cardType));
        SetImage(cardBgImage, currentCardBackgroundSprite);
        SetImage(cardImage, currentCardData.cardImage);
        SetText(cardNameText, currentCardData.cardName);
        SetText(cardTypeText, currentCardData.cardType);
        SetText(cardLevelText, $"Lv. {currentLevel}");
        SetText(cardDescriptionText, FormatDescriptions(PowerCardUpgradeSystem.GetCurrentDescriptions(currentCardData)));
        if (upgradeDescriptionText != null)
        {
            upgradeDescriptionText.gameObject.SetActive(true);
        }

        SetText(upgradeDescriptionText, FormatDescriptions(PowerCardUpgradeSystem.GetNextLevelDescriptions(currentCardData)));
    }

    private Sprite GetTitleSprite(string cardType)
    {
        switch (NormalizeCardType(cardType))
        {
            case "rare":
                return rareTitleSprite != null ? rareTitleSprite : commonTitleSprite;
            case "epic":
                return epicTitleSprite != null ? epicTitleSprite : commonTitleSprite;
            case "legendary":
                return legendaryTitleSprite != null ? legendaryTitleSprite : commonTitleSprite;
            case "common":
            default:
                return commonTitleSprite;
        }
    }

    private static string NormalizeCardType(string cardType)
    {
        return string.IsNullOrWhiteSpace(cardType) ? string.Empty : cardType.Trim().ToLowerInvariant();
    }

    private static string FormatDescriptions(string[] descriptions)
    {
        if (descriptions == null)
        {
            return string.Empty;
        }

        return descriptions.Length == 0 ? string.Empty : string.Join("\n", descriptions);
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
    }
}
