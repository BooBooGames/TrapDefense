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
        titleImage.sprite = titleSprite;
        titleImage.enabled = titleSprite != null;

        cardTypeText.text = cardData.cardType;

        cardNameText.text = cardData.cardName;

        cardImage.sprite = cardData.cardImage;
        cardImage.enabled = cardData.cardImage != null;

        TextMeshProUGUI[] descriptionLabels =
        {
            cardDescriptionText1,
            cardDescriptionText2,
            cardDescriptionText3,
            cardDescriptionText4,
            cardDescriptionText5
        };

        string[] descriptions = cardData.GetDescriptions();
        for (int i = 0; i < descriptionLabels.Length; i++)
        {
            TextMeshProUGUI label = descriptionLabels[i];
            bool hasDescription = i < descriptions.Length && !string.IsNullOrWhiteSpace(descriptions[i]);
            label.gameObject.SetActive(hasDescription);
            label.text = hasDescription ? descriptions[i] : string.Empty;
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(onSelected);
    }
}
