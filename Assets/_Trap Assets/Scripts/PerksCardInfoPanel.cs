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

    public void Show(PowerCardDefinition cardData, Sprite cardBackgroundSprite, UnityAction onClose)
    {
        if (cardData == null)
        {
            return;
        }

        gameObject.SetActive(true);

        SetImage(titleImage, GetTitleSprite(cardData.cardType));
        SetImage(cardBgImage, cardBackgroundSprite);
        SetImage(cardImage, cardData.cardImage);
        SetText(cardNameText, cardData.cardName);
        SetText(cardTypeText, cardData.cardType);
        SetText(cardDescriptionText, FormatDescriptions(cardData));

        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(false);
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

    private static string FormatDescriptions(PowerCardDefinition cardData)
    {
        string[] descriptions = cardData.GetDescriptions();
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
