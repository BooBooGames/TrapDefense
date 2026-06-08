using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXpSystem : MonoBehaviour
{
    public static PlayerXpSystem Instance { get; private set; }

    private const string PocketGearsCardName = "Pocket Gears";
    private const string PocketGearsCardId = "2";
    private const int PocketGearsRewardAmount = 3;
    private const string TrapAcceleratorCardName = "Trap Accelerator";
    private const float TrapAcceleratorSpeedMultiplier = 1.08f;
    private const string MinorHealCardName = "Minor Heal";
    private const int MinorHealRewardAmount = 5;
    private const string AngelBlessingCardName = "Angel Blessing";
    private const int AngelBlessingRewardAmount = 5;
    private const string InvulnerabilityPulseCardName = "Invulnerability Pulse";
    private const float InvulnerabilityPulseDuration = 5f;
    private const float InvulnerabilityPulseCooldown = 30f;
    private const string WaveBonusCardName = "Wave Bonus";
    private const string WaveBonusCardId = "6";
    private const int WaveBonusRewardAmount = 5;
    private const string WeakeningStrikeCardName = "Weakening Strike";
    private const string WeakeningStrikeCardId = "7";
    private const string DeathMarkCardName = "Death Mark";
    private const string DeathMarkCardId = "9";
    private const string DoomTrapsCardName = "Doom Traps";
    private const string DoomTrapsCardId = "10";
    private const string SecondWindCardName = "Second Wind";
    private const string SecondWindCardId = "11";
    private const int SecondWindHealAmount = 10;
    private const string ScrapCollectorCardName = "Scrap Collector";
    private const string ScrapCollectorCardId = "13";
    private const float ScrapCollectorGearChance = 0.05f;
    private const string ResourceMasteryCardName = "Resource Mastery";
    private const string ResourceMasteryCardId = "14";
    private const float ResourceMasteryGearGenerationSpeedMultiplier = 1.25f;
    private const int ResourceMasteryGearCostReduction = 1;
    private const string ToughBaseCardName = "Tough Base";
    private const string ToughBaseCardId = "15";
    private const int ToughBaseHealthBonus = 5;

    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private Image xpBarFill;
    public Sprite commonBGImage, rareBGImage, epicBGImage, legendaryBGImage, commonBGIcon, rareBGIcon, epicBGIcon, legendaryBGIcon;
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
    private bool angelBlessingActive;
    private bool waveBonusActive;
    private bool invulnerabilityPulseActive;
    private bool weakeningStrikeActive;
    private bool deathMarkActive;
    private bool doomTrapsActive;
    private bool secondWindActive;
    private bool secondWindUsed;
    private bool scrapCollectorActive;
    private bool resourceMasteryActive;
    private Coroutine invulnerabilityPulseCoroutine;
    private float previousTimeScale = 1f;

    public event Action<float, int, int> XpProgressChanged;
    public event Action<IReadOnlyList<PowerCardChoice>> CardChoicesPresented;
    public event Action<PowerCardChoice> CardSelected;

    public int CurrentXp => currentXp;
    public int CurrentTarget => currentTargetIndex < xpTargets.Length ? xpTargets[currentTargetIndex] : 0;
    public bool AwaitingCardSelection => awaitingCardSelection;
    public IReadOnlyList<PowerCardChoice> CurrentChoices => currentChoices;
    public IReadOnlyList<PowerCardChoice> SelectedCards => selectedCards;
    public PowerCardCatalog PowerCardCatalogData => powerCardCatalogData;
    public bool WeakeningStrikeActive => weakeningStrikeActive;
    public bool DeathMarkActive => deathMarkActive;
    public bool DoomTrapsActive => doomTrapsActive;
    public bool ResourceMasteryActive => resourceMasteryActive;

    private void Awake()
    {
        Instance = this;
        SetCardPanelVisible(false);
        NotifyProgressChanged();
        rerollButton.onClick.AddListener(() =>
        {
            if (awaitingCardSelection)
            {
                GenerateCardChoices();
                RefreshCardUi();
                SoundManager.Instance.PlayButtonClickSound();
            }
        });
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        EndPointTrigger.SetBaseInvulnerable(false);
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
        SoundManager.Instance.PlayButtonClickSound();
        return true;
    }

    public void UpdateXpProgress(float progress)
    {
        xpBarFill.fillAmount = Mathf.Clamp01(progress);
    }

    public void AwardWaveCompletionBonus()
    {
        if (angelBlessingActive)
        {
            AddHealthWithEffect(AngelBlessingRewardAmount);
        }

        if (waveBonusActive)
        {
            AddGearsWithEffect(WaveBonusRewardAmount);
        }
    }

    public bool TryConsumeSecondWind(out int healAmount)
    {
        healAmount = 0;
        if (!secondWindActive || secondWindUsed)
        {
            return false;
        }

        secondWindUsed = true;
        healAmount = SecondWindHealAmount;
        return true;
    }

    public bool TryRollScrapCollectorGearReward()
    {
        return scrapCollectorActive && UnityEngine.Random.value < ScrapCollectorGearChance;
    }

    public float GetGearGenerationDurationMultiplier()
    {
        return resourceMasteryActive ? 1f / ResourceMasteryGearGenerationSpeedMultiplier : 1f;
    }

    public int ApplyGearUpgradeCostModifiers(int baseCost)
    {
        if (!resourceMasteryActive || baseCost <= 0)
        {
            return Mathf.Max(0, baseCost);
        }

        return Mathf.Max(1, baseCost - ResourceMasteryGearCostReduction);
    }

    public UpgradeResourceCost ApplyGearUpgradeCostModifiers(UpgradeResourceCost baseCost)
    {
        baseCost.gears = ApplyGearUpgradeCostModifiers(baseCost.gears);
        return baseCost;
    }

    public void ResetSessionData()
    {
        currentXp = 0;
        currentTargetIndex = 0;
        awaitingCardSelection = false;
        pausedByCardSelection = false;
        previousTimeScale = 1f;
        angelBlessingActive = false;
        waveBonusActive = false;
        invulnerabilityPulseActive = false;
        weakeningStrikeActive = false;
        deathMarkActive = false;
        doomTrapsActive = false;
        secondWindActive = false;
        secondWindUsed = false;
        scrapCollectorActive = false;
        resourceMasteryActive = false;

        if (invulnerabilityPulseCoroutine != null)
        {
            StopCoroutine(invulnerabilityPulseCoroutine);
            invulnerabilityPulseCoroutine = null;
        }

        EndPointTrigger.SetBaseInvulnerable(false);
        currentChoices.Clear();
        selectedCards.Clear();
        SetCardPanelVisible(false);
        NotifyProgressChanged();
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
        bool hasChoice = choiceIndex < currentChoices.Count;
        PowerCardChoice choice = hasChoice ? currentChoices[choiceIndex] : null;
        cardInfo.gameObject.SetActive(awaitingCardSelection && hasChoice);

        if (!hasChoice || choice == null)
        {
            return;
        }

        cardInfo.Bind(choice.definition, GetTitleSprite(choice.definition), GetIconSprite(choice.definition), () => SelectCard(choiceIndex));
    }

    private void ApplySelectedCardEffect(PowerCardChoice chosenCard)
    {
        if (IsTrapAcceleratorCard(chosenCard))
        {
            WeaponUpgradeController.ApplySpeedMultiplierToCurrentTraps(TrapAcceleratorSpeedMultiplier);
            return;
        }

        if (IsInvulnerabilityPulseCard(chosenCard))
        {
            ActivateInvulnerabilityPulse();
            return;
        }

        if (IsAngelBlessingCard(chosenCard))
        {
            angelBlessingActive = true;
            return;
        }

        if (IsWaveBonusCard(chosenCard))
        {
            waveBonusActive = true;
            return;
        }

        if (IsWeakeningStrikeCard(chosenCard))
        {
            weakeningStrikeActive = true;
            return;
        }

        if (IsDeathMarkCard(chosenCard))
        {
            deathMarkActive = true;
            return;
        }

        if (IsDoomTrapsCard(chosenCard))
        {
            doomTrapsActive = true;
            return;
        }

        if (IsSecondWindCard(chosenCard))
        {
            secondWindActive = true;
            secondWindUsed = false;
            return;
        }

        if (IsScrapCollectorCard(chosenCard))
        {
            scrapCollectorActive = true;
            return;
        }

        if (IsResourceMasteryCard(chosenCard))
        {
            resourceMasteryActive = true;
            GameViewScreen.Instance?.RefreshResourceMasteryModifiers();
            return;
        }

        if (IsToughBaseCard(chosenCard))
        {
            GameViewScreen.Instance?.AddPlayerHealthUpgrade(ToughBaseHealthBonus);
            return;
        }

        if (IsMinorHealCard(chosenCard))
        {
            AddHealthWithEffect(MinorHealRewardAmount);
            return;
        }

        if (!IsPocketGearsCard(chosenCard))
        {
            return;
        }

        AddGearsWithEffect(PocketGearsRewardAmount);
    }

    private static bool IsPocketGearsCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, PocketGearsCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, PocketGearsCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, PocketGearsCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTrapAcceleratorCard(PowerCardChoice chosenCard)
    {
        return HasCardName(chosenCard, TrapAcceleratorCardName);
    }

    private static bool IsMinorHealCard(PowerCardChoice chosenCard)
    {
        return HasCardName(chosenCard, MinorHealCardName);
    }

    private static bool IsAngelBlessingCard(PowerCardChoice chosenCard)
    {
        return HasCardName(chosenCard, AngelBlessingCardName);
    }

    private static bool IsInvulnerabilityPulseCard(PowerCardChoice chosenCard)
    {
        return HasCardName(chosenCard, InvulnerabilityPulseCardName);
    }

    private static bool IsWaveBonusCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, WaveBonusCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, WaveBonusCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, WaveBonusCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWeakeningStrikeCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, WeakeningStrikeCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, WeakeningStrikeCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, WeakeningStrikeCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDeathMarkCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, DeathMarkCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, DeathMarkCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, DeathMarkCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDoomTrapsCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, DoomTrapsCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, DoomTrapsCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, DoomTrapsCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSecondWindCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, SecondWindCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, SecondWindCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, SecondWindCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsScrapCollectorCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, ScrapCollectorCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, ScrapCollectorCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, ScrapCollectorCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsResourceMasteryCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, ResourceMasteryCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, ResourceMasteryCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, ResourceMasteryCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsToughBaseCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, ToughBaseCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, ToughBaseCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, ToughBaseCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasCardName(PowerCardChoice chosenCard, string cardName)
    {
        return string.Equals(chosenCard.definition.cardName, cardName, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddGearsWithEffect(int amount)
    {
        GameViewScreen gameViewScreen = GameViewScreen.Instance;
        Vector3 gearCounterPosition = gameViewScreen.GearCounterLabelPosition;
        UIParticleEffectsManager.Instance.PlayGearEffect(gearCounterPosition);
        gameViewScreen.AddGears(amount);
    }

    private static void AddHealthWithEffect(int amount)
    {
        GameViewScreen gameViewScreen = GameViewScreen.Instance;
        Vector3 healthBarPosition = gameViewScreen.HealthBarLabelPosition;
        UIParticleEffectsManager.Instance.PlayHealthEffect(healthBarPosition);
        gameViewScreen.AddHealth(amount);
    }

    private void ActivateInvulnerabilityPulse()
    {
        if (invulnerabilityPulseActive)
        {
            return;
        }

        invulnerabilityPulseActive = true;
        invulnerabilityPulseCoroutine = StartCoroutine(RunInvulnerabilityPulseCycle());
    }

    private IEnumerator RunInvulnerabilityPulseCycle()
    {
        while (invulnerabilityPulseActive)
        {
            EndPointTrigger.SetBaseInvulnerable(true);
            yield return new WaitForSeconds(InvulnerabilityPulseDuration);

            EndPointTrigger.SetBaseInvulnerable(false);
            yield return new WaitForSeconds(InvulnerabilityPulseCooldown);
        }

        EndPointTrigger.SetBaseInvulnerable(false);
        invulnerabilityPulseCoroutine = null;
    }

    private Sprite GetTitleSprite(PowerCardDefinition cardData)
    {
        if (string.IsNullOrWhiteSpace(cardData.cardType))
        {
            return commonBGImage;
        }

        switch (cardData.cardType.Trim().ToLowerInvariant())
        {
            case "common":
                return commonBGImage;
            case "rare":
                return rareBGImage;
            case "epic":
                return epicBGImage;
            case "legendary":
                return legendaryBGImage;
            default:
                return commonBGImage;
        }
    }

    private Sprite GetIconSprite(PowerCardDefinition cardData)
    {
        if (string.IsNullOrWhiteSpace(cardData.cardType))
        {
            return commonBGIcon;
        }

        switch (cardData.cardType.Trim().ToLowerInvariant())
        {
            case "common":
                return commonBGIcon;
            case "rare":
                return rareBGIcon;
            case "epic":
                return epicBGIcon;
            case "legendary":
                return legendaryBGIcon;
            default:
                return commonBGIcon;
        }
    }

    private void SetCardPanelVisible(bool isVisible)
    {
        cardSelectionPanel.SetActive(isVisible);

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
