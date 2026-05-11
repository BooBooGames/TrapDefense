using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXpSystem : MonoBehaviour
{
    public static PlayerXpSystem Instance { get; private set; }

    private const string PocketGearsCardName = "Pocket Gears";
    private const string PocketGearsCardId = "2";
    private const int PocketGearsRewardAmount = 3;

    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private Image xpBarFill;
    public Sprite commonTitleImage, rareTitleImage, epicTitleImage, legendaryTitleImage;
    [SerializeField] private CardInfo cardInfo1;
    [SerializeField] private CardInfo cardInfo2;
    [SerializeField] private CardInfo cardInfo3;
    [SerializeField] private Button rerollButton;
    [SerializeField] private PowerCardCatalog powerCardCatalogData;
    [SerializeField] private int[] xpTargets = { 10, 16, 26, 46, 75, 127, 255, 470, 970, 1430 };

    private readonly List<PowerCardChoice> currentChoices = new List<PowerCardChoice>();
    private readonly List<PowerCardChoice> selectedCards = new List<PowerCardChoice>();

    private int currentXp;
    private int currentTargetIndex;
    private bool awaitingCardSelection;
    private bool pausedByCardSelection;
    private float previousTimeScale = 1f;

    public event Action<float, int, int> XpProgressChanged;
    public event Action<IReadOnlyList<PowerCardChoice>> CardChoicesPresented;
    public event Action<PowerCardChoice> CardSelected;

    public int CurrentXp => currentXp;
    public int CurrentTarget => currentTargetIndex < xpTargets.Length ? xpTargets[currentTargetIndex] : 0;
    public bool AwaitingCardSelection => awaitingCardSelection;
    public IReadOnlyList<PowerCardChoice> CurrentChoices => currentChoices;
    public IReadOnlyList<PowerCardChoice> SelectedCards => selectedCards;

    private void Awake()
    {
        Instance = this;
        SetCardPanelVisible(false);
        NotifyProgressChanged();
        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(() =>
            {
                if (awaitingCardSelection)
                {
                    GenerateCardChoices();
                    RefreshCardUi();
                }
            });
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddXp(int amount)
    {
        if (amount <= 0 || awaitingCardSelection || xpTargets.Length == 0 || currentTargetIndex >= xpTargets.Length)
        {
            return;
        }

        currentXp += amount;
        NotifyProgressChanged();

        if (CurrentTarget > 0 && currentXp >= CurrentTarget)
        {
            currentXp = CurrentTarget;
            awaitingCardSelection = true;
            GenerateCardChoices();
            RefreshCardUi();
            SetCardPanelVisible(true);
        }
    }

    public bool SelectCard(int choiceIndex)
    {
        if (!awaitingCardSelection || choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            return false;
        }

        PowerCardChoice chosenCard = currentChoices[choiceIndex];
        selectedCards.Add(chosenCard);
        CardSelected?.Invoke(chosenCard);
        ApplySelectedCardEffect(chosenCard);

        awaitingCardSelection = false;
        currentChoices.Clear();
        currentXp = 0;
        currentTargetIndex++;
        RefreshCardUi();
        SetCardPanelVisible(false);
        NotifyProgressChanged();
        return true;
    }

    public void UpdateXpProgress(float progress)
    {
        if (xpBarFill != null)
        {
            xpBarFill.fillAmount = Mathf.Clamp01(progress);
        }
    }

    private void GenerateCardChoices()
    {
        currentChoices.Clear();

        PowerCardDefinition[] allCards = powerCardCatalogData.Cards;
        /* if (allCards == null || allCards.Length == 0)
        {
            CreateFallbackChoices();
            CardChoicesPresented?.Invoke(currentChoices);
            return;
        } */

        List<int> usedIndices = new List<int>();
        int choiceCount = Mathf.Min(3, allCards.Length);
        for (int i = 0; i < choiceCount; i++)
        {
            int cardIndex = GetNextCardIndex(usedIndices, allCards.Length);
            usedIndices.Add(cardIndex);

            currentChoices.Add(new PowerCardChoice
            {
                cardId = string.IsNullOrWhiteSpace(allCards[cardIndex].cardId)
                    ? $"PowerCard_{cardIndex + 1}"
                    : allCards[cardIndex].cardId,
                definition = allCards[cardIndex]
            });
        }

        while (currentChoices.Count < 3)
        {
            currentChoices.Add(currentChoices[currentChoices.Count % choiceCount]);
        }

        CardChoicesPresented?.Invoke(currentChoices);
    }

    private void CreateFallbackChoices()
    {
        for (int i = 0; i < 3; i++)
        {
            currentChoices.Add(new PowerCardChoice
            {
                cardId = $"PowerCard_{currentTargetIndex + 1}_{i + 1}",
                definition = PowerCardDefinition.CreateFallback($"Power Card {i + 1}")
            });
        }
    }

    private void RefreshCardUi()
    {
        BindCardInfo(cardInfo1, 0);
        BindCardInfo(cardInfo2, 1);
        BindCardInfo(cardInfo3, 2);
    }

    private void BindCardInfo(CardInfo cardInfo, int choiceIndex)
    {
        if (cardInfo == null)
        {
            return;
        }

        bool hasChoice = choiceIndex < currentChoices.Count;
        PowerCardChoice choice = hasChoice ? currentChoices[choiceIndex] : null;
        cardInfo.gameObject.SetActive(awaitingCardSelection && hasChoice);

        if (!hasChoice || choice == null)
        {
            return;
        }

        cardInfo.Bind(choice.definition, GetTitleSprite(choice.definition), () => SelectCard(choiceIndex));
    }

    private void ApplySelectedCardEffect(PowerCardChoice chosenCard)
    {
        if (!IsPocketGearsCard(chosenCard))
        {
            return;
        }

        GameViewScreen gameViewScreen = GameViewScreen.Instance;
        if (gameViewScreen == null)
        {
            return;
        }

        Vector3 gearCounterPosition = gameViewScreen.GearCounterLabelPosition;
        UIParticleEffectsManager.Instance?.PlayGearEffect(gearCounterPosition);
        gameViewScreen.AddGears(PocketGearsRewardAmount);
    }

    private static bool IsPocketGearsCard(PowerCardChoice chosenCard)
    {
        if (chosenCard == null)
        {
            return false;
        }

        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, PocketGearsCardId, StringComparison.OrdinalIgnoreCase) ||
            (definition != null && string.Equals(definition.cardId, PocketGearsCardId, StringComparison.OrdinalIgnoreCase)) ||
            (definition != null && string.Equals(definition.cardName, PocketGearsCardName, StringComparison.OrdinalIgnoreCase));
    }

    private Sprite GetTitleSprite(PowerCardDefinition cardData)
    {
        if (cardData == null || string.IsNullOrWhiteSpace(cardData.cardType))
        {
            return commonTitleImage;
        }

        switch (cardData.cardType.Trim().ToLowerInvariant())
        {
            case "common":
                return commonTitleImage;
            case "rare":
                return rareTitleImage;
            case "epic":
                return epicTitleImage;
            case "legendary":
                return legendaryTitleImage;
            default:
                return commonTitleImage;
        }
    }

    private void SetCardPanelVisible(bool isVisible)
    {
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(isVisible);
        }

        if (isVisible)
        {
            if (!pausedByCardSelection)
            {
                previousTimeScale = Time.timeScale;
                pausedByCardSelection = true;
            }

            Time.timeScale = 0f;
            return;
        }

        if (!pausedByCardSelection)
        {
            return;
        }

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        pausedByCardSelection = false;
    }

    private void NotifyProgressChanged()
    {
        float progress = CurrentTarget > 0 ? Mathf.Clamp01(currentXp / (float)CurrentTarget) : 1f;
        UpdateXpProgress(progress);
        XpProgressChanged?.Invoke(progress, currentXp, CurrentTarget);
    }

    private int GetNextCardIndex(List<int> usedIndices, int totalCards)
    {
        if (totalCards <= usedIndices.Count)
        {
            return (currentTargetIndex + usedIndices.Count) % totalCards;
        }

        int attempts = 0;
        while (attempts < 16)
        {
            int candidate = UnityEngine.Random.Range(0, totalCards);
            if (!usedIndices.Contains(candidate))
            {
                return candidate;
            }

            attempts++;
        }

        for (int i = 0; i < totalCards; i++)
        {
            if (!usedIndices.Contains(i))
            {
                return i;
            }
        }

        return 0;
    }
}

[Serializable]
public class PowerCardChoice
{
    public string cardId;
    public PowerCardDefinition definition;
}

