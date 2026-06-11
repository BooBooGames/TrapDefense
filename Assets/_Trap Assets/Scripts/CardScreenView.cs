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
    // public Button perksButton, heroButton;

    private void OnEnable()
    {
        PlayerCurrencySystem.ElixirChanged += HandleElixirChanged;
        PowerCardUpgradeSystem.CardLevelChanged += HandleCardLevelChanged;
        PowerCardUpgradeSystem.CardCopiesChanged += HandleCardCopiesChanged;
        BindSummonButtons();
        RefreshCardData();
        RefreshSummonButtonStates();
    }

    private void OnDisable()
    {
        PlayerCurrencySystem.ElixirChanged -= HandleElixirChanged;
        PowerCardUpgradeSystem.CardLevelChanged -= HandleCardLevelChanged;
        PowerCardUpgradeSystem.CardCopiesChanged -= HandleCardCopiesChanged;
    }

    private void HandleElixirChanged(int _)
    {
        RefreshSummonButtonStates();
        UIManager.Instance?.RefreshSummonScreenButtonStates();
    }

    private void HandleCardLevelChanged(PowerCardDefinition _, int __)
    {
        RefreshCardData();
    }

    private void HandleCardCopiesChanged(PowerCardDefinition _, int __)
    {
        RefreshCardData();
    }

    private void BindSummonButtons()
    {
        if (summonx1Button != null)
        {
            summonx1Button.onClick.RemoveListener(HandleSummonX1Clicked);
            summonx1Button.onClick.AddListener(HandleSummonX1Clicked);
        }

        if (summonx10Button != null)
        {
            summonx10Button.onClick.RemoveListener(HandleSummonX10Clicked);
            summonx10Button.onClick.AddListener(HandleSummonX10Clicked);
        }
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

    private void HandleSummonX1Clicked()
    {
        TryStartSummon(1);
    }

    private void HandleSummonX10Clicked()
    {
        TryStartSummon(10);
    }

    private bool TryStartSummon(int summonCount)
    {
        PowerCardDefinition[] summonableCards = GetSummonableCards();
        if (summonableCards.Length == 0)
        {
            return false;
        }

        if (UIManager.Instance == null)
        {
            return false;
        }

        int summonCost = GetSummonCost(summonCount);
        if (!PlayerCurrencySystem.TrySpendElixir(summonCost))
        {
            RefreshSummonButtonStates();
            UIManager.Instance?.RefreshSummonScreenButtonStates();
            return false;
        }

        List<PowerCardDefinition> drawnCards = DrawRandomCards(summonableCards, summonCount);
        if (drawnCards.Count == 0)
        {
            PlayerCurrencySystem.AddElixir(summonCost);
            return false;
        }

        PowerCardDefinition displayedCard = drawnCards[drawnCards.Count - 1];
        Sprite cardBackgroundSprite = GetCardBackgroundSprite(displayedCard.cardType);

        UIManager.Instance.ShowSummonScreen(
            displayedCard,
            cardBackgroundSprite,
            CanSummonX1,
            CanSummonX10,
            HandleSummonX1Clicked,
            HandleSummonX10Clicked,
            HandleSummonContinueClicked,
            () => CompleteSummon(drawnCards));

        RefreshSummonButtonStates();
        return true;
    }

    private void CompleteSummon(List<PowerCardDefinition> drawnCards)
    {
        for (int i = 0; i < drawnCards.Count; i++)
        {
            PowerCardUpgradeSystem.AddCardCopies(drawnCards[i], 1);
        }

        RefreshCardData();
        RefreshSummonButtonStates();
        UIManager.Instance?.RefreshSummonScreenButtonStates();
    }

    private void HandleSummonContinueClicked()
    {
        UIManager.Instance?.CloseSummonScreen();
        RefreshCardData();
        RefreshSummonButtonStates();
    }

    private bool CanSummonX1()
    {
        return PlayerCurrencySystem.Elixir >= SummonX1ElixirCost;
    }

    private bool CanSummonX10()
    {
        return PlayerCurrencySystem.Elixir >= SummonX10ElixirCost;
    }

    private int GetSummonCost(int summonCount)
    {
        return summonCount >= 10 ? SummonX10ElixirCost : SummonX1ElixirCost;
    }

    private PowerCardDefinition[] GetSummonableCards()
    {
        PowerCardCatalog catalog = ResolvePowerCardCatalog();
        PowerCardDefinition[] cards = catalog != null && catalog.Cards != null ? catalog.Cards : Array.Empty<PowerCardDefinition>();
        List<PowerCardDefinition> summonableCards = new List<PowerCardDefinition>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                summonableCards.Add(cards[i]);
            }
        }

        return summonableCards.ToArray();
    }

    private List<PowerCardDefinition> DrawRandomCards(PowerCardDefinition[] summonableCards, int summonCount)
    {
        List<PowerCardDefinition> drawnCards = new List<PowerCardDefinition>();
        int drawCount = Mathf.Max(1, summonCount);

        for (int i = 0; i < drawCount; i++)
        {
            drawnCards.Add(summonableCards[UnityEngine.Random.Range(0, summonableCards.Length)]);
        }

        return drawnCards;
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

        cardData.cardId = card.cardId;
        cardData.cardName = card.cardName;
        cardData.cardType = card.cardType;
        cardData.cardSprite = card.cardImage;
        cardData.descriptions = PowerCardUpgradeSystem.GetCurrentDescriptions(card);

        SetImage(cardData.bgImage, GetCardBackgroundSprite(card.cardType));
        SetImage(cardData.iconImage, card.cardImage);
        SetImage(cardData.levelBgImage, GetLevelBackgroundSprite(card.cardType));
        SetText(cardData.levelText, $"Lv. {PowerCardUpgradeSystem.GetCardLevel(card)}");
        SetText(cardData.countText, PowerCardUpgradeSystem.GetCardCopyProgressText(card));
        BindCardButton(cardData, card);
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
        BindCardButton(cardData, null);


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

    private void BindCardButton(UpgradeCardData cardData, PowerCardDefinition card)
    {
        if (cardData.bgImage == null)
        {
            return;
        }

        Button cardButton = cardData.bgImage.GetComponent<Button>();
        if (cardButton == null)
        {
            return;
        }

        cardButton.onClick.RemoveAllListeners();
        cardButton.interactable = card != null;

        if (card == null)
        {
            return;
        }

        PowerCardDefinition selectedCard = card;
        Sprite selectedCardBackgroundSprite = cardData.bgImage.sprite;
        cardButton.onClick.AddListener(() => HandleCardClicked(selectedCard, selectedCardBackgroundSprite));
    }

    private void HandleCardClicked(PowerCardDefinition card, Sprite cardBackgroundSprite)
    {
        if (card == null || UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.ShowPerksCardInfoPanel(card, cardBackgroundSprite);
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
