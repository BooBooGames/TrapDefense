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
    private const string TrapAcceleratorCardName = "Trap Accelerator";
    private const string TrapAcceleratorCardId = "1";
    private const string MinorHealCardName = "Minor Heal";
    private const string MinorHealCardId = "3";
    private const string AngelBlessingCardName = "Angel Blessing";
    private const string AngelBlessingCardId = "4";
    private const string ThinArmorCardName = "Thin Armor";
    private const string ThinArmorCardId = "5";
    private const string InvulnerabilityPulseCardName = "Invulnerability Pulse";
    private const string InvulnerabilityPulseCardId = "8";
    private const string WaveBonusCardName = "Wave Bonus";
    private const string WaveBonusCardId = "6";
    private const string WeakeningStrikeCardName = "Weakening Strike";
    private const string WeakeningStrikeCardId = "7";
    private const string DeathMarkCardName = "Death Mark";
    private const string DeathMarkCardId = "9";
    private const string DoomTrapsCardName = "Doom Traps";
    private const string DoomTrapsCardId = "10";
    private const string SecondWindCardName = "Second Wind";
    private const string SecondWindCardId = "11";
    private const string SharpTrapsCardName = "Sharp Traps";
    private const string SharpTrapsCardId = "12";
    private const string ScrapCollectorCardName = "Scrap Collector";
    private const string ScrapCollectorCardId = "13";
    private const string ResourceMasteryCardName = "Resource Mastery";
    private const string ResourceMasteryCardId = "14";
    private const string ToughBaseCardName = "Tough Base";
    private const string ToughBaseCardId = "15";

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
    private bool thinArmorActive;
    private bool waveBonusActive;
    private bool invulnerabilityPulseActive;
    private bool weakeningStrikeActive;
    private bool deathMarkActive;
    private bool doomTrapsActive;
    private bool secondWindActive;
    private bool secondWindUsed;
    private bool sharpTrapsActive;
    private bool scrapCollectorActive;
    private bool resourceMasteryActive;
    private Coroutine invulnerabilityPulseCoroutine;
    private float previousTimeScale = 1f;
    private int angelBlessingHealAmount;
    private int waveBonusGearAmount;
    private int secondWindHealAmount;
    private float thinArmorEnemyHealthMultiplier = 1f;
    private float weakeningStrikeSlowMultiplier = 1f;
    private float weakeningStrikeSlowDuration;
    private float deathMarkInstantKillChance;
    private float doomTrapsSpeedMultiplierPerHit = 1f;
    private float doomTrapsTrapDamageMultiplier = 1f;
    private float sharpTrapsDamageMultiplier = 1f;
    private float scrapCollectorGearChance;

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
    public bool ThinArmorActive => thinArmorActive;
    public bool SharpTrapsActive => sharpTrapsActive;

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
            AddHealthWithEffect(angelBlessingHealAmount);
        }

        if (waveBonusActive)
        {
            AddGearsWithEffect(waveBonusGearAmount);
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
        healAmount = secondWindHealAmount;
        return true;
    }

    public bool TryRollScrapCollectorGearReward()
    {
        return scrapCollectorActive && UnityEngine.Random.value < scrapCollectorGearChance;
    }

    public float GetGearGenerationDurationMultiplier()
    {
        return resourceMasteryActive ? PowerCardUpgradeSystem.GetResourceMasteryGearGenerationDurationMultiplier() : 1f;
    }

    public int ApplyGearUpgradeCostModifiers(int baseCost)
    {
        if (!resourceMasteryActive || baseCost <= 0)
        {
            return Mathf.Max(0, baseCost);
        }

        return PowerCardUpgradeSystem.ApplyResourceMasteryUpgradeCostReduction(baseCost);
    }

    public UpgradeResourceCost ApplyGearUpgradeCostModifiers(UpgradeResourceCost baseCost)
    {
        baseCost.gears = ApplyGearUpgradeCostModifiers(baseCost.gears);
        return baseCost;
    }

    public float GetTrapDamageMultiplier()
    {
        float multiplier = 1f;
        if (doomTrapsActive)
        {
            multiplier *= doomTrapsTrapDamageMultiplier;
        }

        if (sharpTrapsActive)
        {
            multiplier *= sharpTrapsDamageMultiplier;
        }

        return multiplier;
    }

    public float GetEnemyHealthMultiplier()
    {
        return thinArmorActive ? thinArmorEnemyHealthMultiplier : 1f;
    }

    public float GetWeakeningStrikeSlowMultiplier()
    {
        return weakeningStrikeSlowMultiplier;
    }

    public float GetWeakeningStrikeSlowDuration()
    {
        return weakeningStrikeSlowDuration;
    }

    public float GetDeathMarkInstantKillChance()
    {
        return deathMarkInstantKillChance;
    }

    public float GetDoomTrapsSpeedMultiplierPerHit()
    {
        return doomTrapsSpeedMultiplierPerHit;
    }

    public void ResetSessionData()
    {
        currentXp = 0;
        currentTargetIndex = 0;
        awaitingCardSelection = false;
        pausedByCardSelection = false;
        previousTimeScale = 1f;
        angelBlessingActive = false;
        thinArmorActive = false;
        waveBonusActive = false;
        invulnerabilityPulseActive = false;
        weakeningStrikeActive = false;
        deathMarkActive = false;
        doomTrapsActive = false;
        secondWindActive = false;
        secondWindUsed = false;
        sharpTrapsActive = false;
        scrapCollectorActive = false;
        resourceMasteryActive = false;
        ResetActiveCardEffectValues();

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

    private void ResetActiveCardEffectValues()
    {
        angelBlessingHealAmount = 0;
        waveBonusGearAmount = 0;
        secondWindHealAmount = 0;
        thinArmorEnemyHealthMultiplier = 1f;
        weakeningStrikeSlowMultiplier = 1f;
        weakeningStrikeSlowDuration = 0f;
        deathMarkInstantKillChance = 0f;
        doomTrapsSpeedMultiplierPerHit = 1f;
        doomTrapsTrapDamageMultiplier = 1f;
        sharpTrapsDamageMultiplier = 1f;
        scrapCollectorGearChance = 0f;
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
            WeaponUpgradeController.ApplySpeedMultiplierToCurrentTraps(PowerCardUpgradeSystem.GetTrapAcceleratorSpeedMultiplier(chosenCard.definition));
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
            angelBlessingHealAmount = PowerCardUpgradeSystem.GetAngelBlessingHealAmount();
            return;
        }

        if (IsThinArmorCard(chosenCard))
        {
            if (!thinArmorActive)
            {
                thinArmorActive = true;
                thinArmorEnemyHealthMultiplier = PowerCardUpgradeSystem.GetThinArmorEnemyHealthMultiplier();
                ApplyThinArmorToCurrentEnemies();
            }

            return;
        }

        if (IsWaveBonusCard(chosenCard))
        {
            waveBonusActive = true;
            waveBonusGearAmount = PowerCardUpgradeSystem.GetWaveBonusGears();
            return;
        }

        if (IsWeakeningStrikeCard(chosenCard))
        {
            weakeningStrikeActive = true;
            weakeningStrikeSlowMultiplier = PowerCardUpgradeSystem.GetWeakeningStrikeSlowMultiplier();
            weakeningStrikeSlowDuration = PowerCardUpgradeSystem.GetWeakeningStrikeSlowDuration();
            return;
        }

        if (IsDeathMarkCard(chosenCard))
        {
            deathMarkActive = true;
            deathMarkInstantKillChance = PowerCardUpgradeSystem.GetDeathMarkInstantKillChance();
            return;
        }

        if (IsDoomTrapsCard(chosenCard))
        {
            doomTrapsActive = true;
            doomTrapsSpeedMultiplierPerHit = PowerCardUpgradeSystem.GetDoomTrapsSlowMultiplierPerHit();
            doomTrapsTrapDamageMultiplier = PowerCardUpgradeSystem.GetDoomTrapsTrapDamageMultiplier();
            return;
        }

        if (IsSecondWindCard(chosenCard))
        {
            secondWindActive = true;
            secondWindUsed = false;
            secondWindHealAmount = PowerCardUpgradeSystem.GetSecondWindHealAmount();
            return;
        }

        if (IsSharpTrapsCard(chosenCard))
        {
            sharpTrapsActive = true;
            sharpTrapsDamageMultiplier = PowerCardUpgradeSystem.GetSharpTrapsDamageMultiplier();
            return;
        }

        if (IsScrapCollectorCard(chosenCard))
        {
            scrapCollectorActive = true;
            scrapCollectorGearChance = PowerCardUpgradeSystem.GetScrapCollectorGearChance();
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
            GameViewScreen.Instance?.AddPlayerHealthUpgrade(PowerCardUpgradeSystem.GetToughBaseHealthBonus(chosenCard.definition));
            return;
        }

        if (IsMinorHealCard(chosenCard))
        {
            AddHealthWithEffect(PowerCardUpgradeSystem.GetMinorHealAmount(chosenCard.definition));
            return;
        }

        if (!IsPocketGearsCard(chosenCard))
        {
            return;
        }

        AddGearsWithEffect(PowerCardUpgradeSystem.GetPocketGearsReward(chosenCard.definition));
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
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, TrapAcceleratorCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, TrapAcceleratorCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, TrapAcceleratorCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMinorHealCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, MinorHealCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, MinorHealCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, MinorHealCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAngelBlessingCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, AngelBlessingCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, AngelBlessingCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, AngelBlessingCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsThinArmorCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, ThinArmorCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, ThinArmorCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, ThinArmorCardName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInvulnerabilityPulseCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, InvulnerabilityPulseCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, InvulnerabilityPulseCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, InvulnerabilityPulseCardName, StringComparison.OrdinalIgnoreCase);
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

    private static bool IsSharpTrapsCard(PowerCardChoice chosenCard)
    {
        PowerCardDefinition definition = chosenCard.definition;
        return string.Equals(chosenCard.cardId, SharpTrapsCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardId, SharpTrapsCardId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.cardName, SharpTrapsCardName, StringComparison.OrdinalIgnoreCase);
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

    private void ApplyThinArmorToCurrentEnemies()
    {
        ZombieRuntime[] zombies = FindObjectsByType<ZombieRuntime>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] != null)
            {
                zombies[i].ApplyMaxHealthMultiplier(thinArmorEnemyHealthMultiplier);
            }
        }
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
            yield return new WaitForSeconds(PowerCardUpgradeSystem.GetInvulnerabilityPulseDuration());

            EndPointTrigger.SetBaseInvulnerable(false);
            yield return new WaitForSeconds(PowerCardUpgradeSystem.GetInvulnerabilityPulseCooldown());
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

        GameplaySpeedSystem.ApplyCurrentSpeedToTimeScale(true);
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
