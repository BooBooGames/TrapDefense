using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardScreenView : MonoBehaviour
{
    private const int SummonX1ElixirCost = 8;
    private const int SummonX10ElixirCost = 64;

    public Sprite commonCardSprite, rareCardSprite, epicCardSprite, legendaryCardSprite, lockCardSprite, commonLevelBgSprite, rareLevelBgSprite, epicLevelBgSprite, legendaryLevelBgSprite, lockLevelBgSprite, lockIconSprite;

    public Button summonx1Button, summonx10Button;
    [SerializeField] private PowerCardCatalog powerCardCatalogData;

    [Serializable]
    public class UpgradeCardData
    {
        [HideInInspector] public string cardId, cardName, cardType;
        [HideInInspector] public Sprite cardSprite;
        [HideInInspector] public string[] descriptions = Array.Empty<string>();

        public Image bgImage, iconImage, levelBgImage;
        public TextMeshProUGUI levelText, countText;
    }
    public List<UpgradeCardData> upgradeCardDatas;
    public Button perksButton, heroButton;

    private void OnEnable()
    {
        PlayerCurrencySystem.ElixirChanged += HandleElixirChanged;
        RefreshCardData();
        RefreshSummonButtonStates();
    }

    private void OnDisable()
    {
        PlayerCurrencySystem.ElixirChanged -= HandleElixirChanged;
    }

    private void HandleElixirChanged(int _)
    {
        RefreshSummonButtonStates();
    }

    private void RefreshSummonButtonStates()
    {
        PlayerCurrencySystem.ReloadElixirFromSave();
        int currentElixir = PlayerCurrencySystem.Elixir;

        if (summonx1Button != null)
        {
            summonx1Button.interactable = currentElixir >= SummonX1ElixirCost;
        }

        if (summonx10Button != null)
        {
            summonx10Button.interactable = currentElixir >= SummonX10ElixirCost;
        }
    }

    private void RefreshCardData()
    {
        if (upgradeCardDatas == null)
        {
            return;
        }

        PowerCardCatalog catalog = ResolvePowerCardCatalog();
        PowerCardDefinition[] cards = catalog != null && catalog.Cards != null ? catalog.Cards : Array.Empty<PowerCardDefinition>();

        for (int i = 0; i < upgradeCardDatas.Count; i++)
        {
            PowerCardDefinition card = i < cards.Length ? cards[i] : null;
            ApplyCardData(upgradeCardDatas[i], card);
        }
    }

    private PowerCardCatalog ResolvePowerCardCatalog()
    {
        if (powerCardCatalogData != null)
        {
            return powerCardCatalogData;
        }

        PlayerXpSystem playerXpSystem = PlayerXpSystem.Instance != null
            ? PlayerXpSystem.Instance
            : FindFirstObjectByType<PlayerXpSystem>(FindObjectsInactive.Include);

        return playerXpSystem != null ? playerXpSystem.PowerCardCatalogData : null;
    }

    private void ApplyCardData(UpgradeCardData cardData, PowerCardDefinition card)
    {
        if (cardData == null)
        {
            return;
        }

        if (card == null)
        {
            ClearCardData(cardData);
            return;
        }

        string description = FormatDescriptions(card);
        cardData.cardId = card.cardId;
        cardData.cardName = card.cardName;
        cardData.cardType = card.cardType;
        cardData.cardSprite = card.cardImage;
        cardData.descriptions = card.GetDescriptions();

        SetImage(cardData.bgImage, GetCardBackgroundSprite(card.cardType));
        SetImage(cardData.iconImage, card.cardImage);
        SetImage(cardData.levelBgImage, GetLevelBackgroundSprite(card.cardType));
    }

    private void ClearCardData(UpgradeCardData cardData)
    {
        cardData.cardId = string.Empty;
        cardData.cardName = string.Empty;
        cardData.cardType = string.Empty;
        cardData.cardSprite = null;
        cardData.descriptions = Array.Empty<string>();

        SetImage(cardData.bgImage, lockCardSprite);
        SetImage(cardData.iconImage, lockIconSprite);
        SetImage(cardData.levelBgImage, lockLevelBgSprite);


        SetText(cardData.levelText, string.Empty);
        SetText(cardData.countText, string.Empty);
    }

    private Sprite GetCardBackgroundSprite(string cardType)
    {
        switch (NormalizeCardType(cardType))
        {
            case "rare":
                return rareCardSprite != null ? rareCardSprite : commonCardSprite;
            case "epic":
                return epicCardSprite != null ? epicCardSprite : commonCardSprite;
            case "legendary":
                return legendaryCardSprite != null ? legendaryCardSprite : commonCardSprite;
            case "common":
            default:
                return commonCardSprite;
        }
    }

    private Sprite GetLevelBackgroundSprite(string cardType)
    {
        switch (NormalizeCardType(cardType))
        {
            case "rare":
                return rareLevelBgSprite != null ? rareLevelBgSprite : commonLevelBgSprite;
            case "epic":
                return epicLevelBgSprite != null ? epicLevelBgSprite : commonLevelBgSprite;
            case "legendary":
                return legendaryLevelBgSprite != null ? legendaryLevelBgSprite : commonLevelBgSprite;
            case "common":
            default:
                return commonLevelBgSprite;
        }
    }

    private string NormalizeCardType(string cardType)
    {
        return string.IsNullOrWhiteSpace(cardType) ? string.Empty : cardType.Trim().ToLowerInvariant();
    }

    private string FormatDescriptions(PowerCardDefinition card)
    {
        string[] descriptions = card.GetDescriptions();
        return descriptions.Length == 0 ? string.Empty : string.Join("\n", descriptions);
    }

    private void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private void SetText(TextMeshProUGUI text, string value)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
    }
}
