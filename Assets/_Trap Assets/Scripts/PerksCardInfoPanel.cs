using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PerksCardInfoPanel : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI cardNameText, cardLevelText, cardDescriptionText, upgradeDescriptionText;
    public Button upgradeButton, CloseButton;

    public void Show(PowerCardDefinition cardData, UnityAction onClose)
    {
        if (cardData == null)
        {
            return;
        }

        gameObject.SetActive(true);

        SetImage(cardImage, cardData.cardImage);
        SetText(cardNameText, cardData.cardName);
        SetText(cardLevelText, cardData.cardType);
        SetText(cardDescriptionText, FormatDescriptions(cardData));
        SetText(upgradeDescriptionText, string.Empty);

        if (upgradeDescriptionText != null)
        {
            upgradeDescriptionText.gameObject.SetActive(false);
        }

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
