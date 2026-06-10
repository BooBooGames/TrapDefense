using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInfo : MonoBehaviour
{
    public Image cardBGImage;
    public TextMeshProUGUI cardTypeText;
    public Image cardIconBG, cardIconImage;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText1, cardDescriptionText2, cardDescriptionText3, cardDescriptionText4, cardDescriptionText5;
    public Button selectButton;

    public void Bind(PowerCardDefinition cardData, Sprite bgImage, Sprite iconBGImage, UnityEngine.Events.UnityAction onSelected)
    {
        cardBGImage.sprite = bgImage;
        cardBGImage.enabled = bgImage != null;

        cardTypeText.text = cardData.cardType;

        cardNameText.text = cardData.cardName;

        cardIconBG.sprite = iconBGImage;
        cardIconBG.enabled = iconBGImage != null;

        cardIconImage.sprite = cardData.cardImage;
        cardIconImage.enabled = cardData.cardImage != null;

        TextMeshProUGUI[] descriptionLabels =
        {
            cardDescriptionText1,
            cardDescriptionText2,
            cardDescriptionText3,
            cardDescriptionText4,
            cardDescriptionText5
        };

        string[] descriptions = PowerCardUpgradeSystem.GetCurrentDescriptions(cardData);
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
