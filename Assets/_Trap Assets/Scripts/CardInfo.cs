using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInfo : MonoBehaviour
{
    public Image titleImage;
    public TextMeshProUGUI cardTypeText;
    public Image cardImage;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText1, cardDescriptionText2, cardDescriptionText3, cardDescriptionText4, cardDescriptionText5;
    public Button selectButton;

    public void Bind(PowerCardDefinition cardData, Sprite titleSprite, UnityEngine.Events.UnityAction onSelected)
    {
        if (titleImage != null)
        {
            titleImage.sprite = titleSprite;
            titleImage.enabled = titleSprite != null;
        }

        if (cardTypeText != null)
        {
            cardTypeText.text = cardData != null ? cardData.cardType : string.Empty;
        }

        if (cardNameText != null)
        {
            cardNameText.text = cardData != null ? cardData.cardName : string.Empty;
        }

        if (cardImage != null)
        {
            cardImage.sprite = cardData != null ? cardData.cardImage : null;
            cardImage.enabled = cardData != null && cardData.cardImage != null;
        }

        TextMeshProUGUI[] descriptionLabels =
        {
            cardDescriptionText1,
            cardDescriptionText2,
            cardDescriptionText3,
            cardDescriptionText4,
            cardDescriptionText5
        };

        string[] descriptions = cardData != null ? cardData.GetDescriptions() : System.Array.Empty<string>();
        for (int i = 0; i < descriptionLabels.Length; i++)
        {
            TextMeshProUGUI label = descriptionLabels[i];
            if (label == null)
            {
                continue;
            }

            bool hasDescription = i < descriptions.Length && !string.IsNullOrWhiteSpace(descriptions[i]);
            label.gameObject.SetActive(hasDescription);
            label.text = hasDescription ? descriptions[i] : string.Empty;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            if (onSelected != null)
            {
                selectButton.onClick.AddListener(onSelected);
            }
        }
    }
}
