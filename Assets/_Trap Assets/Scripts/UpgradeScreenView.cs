using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeScreenView : MonoBehaviour
{
    [SerializeField] private UpgradeScreenConfig upgradeConfig;

    public GameObject upgradesBG, evolutionBG;

    [Header("Weapons")]
    public Image weapon1Image, weapon2Image, weapon3Image;
    public TextMeshProUGUI weapon1Text, weapon2Text, weapon3Text;
    public Button weapon2BuyButton, weapon3BuyButton;
    public TextMeshProUGUI weapon2CostText, weapon3CostText;

    [Header("Stat Upgrades")]
    public TextMeshProUGUI gearFlowText, baseHealthText;
    public Button gearFlowUpgradeButton, baseHealthUpgradeButton;
    public TextMeshProUGUI gearFlowUpgradeCostText, baseHealthUpgradeCostText;

    private TextMeshProUGUI weapon2ButtonLabel;
    private TextMeshProUGUI weapon3ButtonLabel;

    public UpgradeScreenConfig ConfigAsset => upgradeConfig;

    public Button upgradeButton, evolutionButton;

    [Header("Evolution")] public TextMeshProUGUI timelineText;
    public Image timeline1Image, timeline2Image;
    public Button evolveButton;
    public TextMeshProUGUI descriptionText1, descriptionText2, evolutionCostText;

    private void Awake()
    {
        CacheButtonLabels();
        BindButtons();
        PlayerUpgradeSystem.Initialize(upgradeConfig);
        RefreshUi();
    }

    private void OnEnable()
    {
        PlayerUpgradeSystem.Initialize(upgradeConfig);
        PlayerUpgradeSystem.UpgradeStateChanged += HandleUpgradeStateChanged;
        PlayerCurrencySystem.CurrencyChanged += HandleCurrencyChanged;
        ShowUpgradeBackground();
        RefreshUi();
    }

    private void OnDisable()
    {
        PlayerUpgradeSystem.UpgradeStateChanged -= HandleUpgradeStateChanged;
        PlayerCurrencySystem.CurrencyChanged -= HandleCurrencyChanged;
    }

    private void HandleUpgradeStateChanged()
    {
        RefreshUi();
    }

    private void HandleCurrencyChanged(int _, int __)
    {
        RefreshUi();
    }

    private void BindButtons()
    {
        BindButton(weapon2BuyButton, HandleWeapon2UnlockClicked);
        BindButton(weapon3BuyButton, HandleWeapon3UnlockClicked);
        BindButton(gearFlowUpgradeButton, HandleGearFlowUpgradeClicked);
        BindButton(baseHealthUpgradeButton, HandleBaseHealthUpgradeClicked);
        BindButton(upgradeButton, HandleUpgradeTabClicked);
        BindButton(evolutionButton, HandleEvolutionTabClicked);
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }

    private void HandleWeapon2UnlockClicked()
    {
        if (PlayerUpgradeSystem.TryUnlockWeapon(1))
        {
            RefreshUi();
        }
    }

    private void HandleWeapon3UnlockClicked()
    {
        if (PlayerUpgradeSystem.TryUnlockWeapon(2))
        {
            RefreshUi();
        }
    }

    private void HandleGearFlowUpgradeClicked()
    {
        if (PlayerUpgradeSystem.TryUpgradeGearFlow())
        {
            RefreshUi();
        }
    }

    private void HandleBaseHealthUpgradeClicked()
    {
        if (PlayerUpgradeSystem.TryUpgradeBaseHealth())
        {
            RefreshUi();
        }
    }


    private void HandleUpgradeTabClicked()
    {
        ShowUpgradeBackground();
    }

    private void HandleEvolutionTabClicked()
    {
        ShowEvolutionBackground();
    }

    private void ShowUpgradeBackground()
    {
        if (upgradesBG != null)
        {
            upgradesBG.SetActive(true);
        }

        if (evolutionBG != null)
        {
            evolutionBG.SetActive(false);
        }
    }

    private void ShowEvolutionBackground()
    {
        if (evolutionBG != null)
        {
            evolutionBG.SetActive(true);
        }

        if (upgradesBG != null)
        {
            upgradesBG.SetActive(false);
        }
    }
    private void RefreshUi()
    {
        PlayerUpgradeSystem.Initialize(upgradeConfig);

        UpgradeScreenConfig config = PlayerUpgradeSystem.Config;
        RefreshWeaponSlot(0, config, weapon1Image, weapon1Text, null, null, null);
        RefreshWeaponSlot(1, config, weapon2Image, weapon2Text, weapon2BuyButton, weapon2CostText, weapon2ButtonLabel);
        RefreshWeaponSlot(2, config, weapon3Image, weapon3Text, weapon3BuyButton, weapon3CostText, weapon3ButtonLabel);
        RefreshStatUpgrades();
        RefreshEvolution(config);
    }

    private void RefreshEvolution(UpgradeScreenConfig config)
    {
        if (config == null)
        {
            return;
        }

        if (timeline1Image != null && config.timeline1Sprite != null)
        {
            timeline1Image.sprite = config.timeline1Sprite;
        }

        if (timeline2Image != null && config.timeline2Sprite != null)
        {
            timeline2Image.sprite = config.timeline2Sprite;
        }

        if (descriptionText1 != null)
        {
            descriptionText1.text = config.evolutionDescription1;
        }

        if (descriptionText2 != null)
        {
            descriptionText2.text = config.evolutionDescription2;
        }

        if (evolutionCostText != null)
        {
            evolutionCostText.text = config.evolutionCost.ToString();
        }

        if (evolveButton != null)
        {
            UpgradeResourceCost evolutionCost = new UpgradeResourceCost
            {
                coins = Mathf.Max(0, config.evolutionCost),
                gears = 0,
            };

            evolveButton.interactable = PlayerUpgradeSystem.CanAfford(evolutionCost);
        }
    }

    private void RefreshWeaponSlot(
        int weaponIndex,
        UpgradeScreenConfig config,
        Image weaponImage,
        TextMeshProUGUI weaponNameLabel,
        Button unlockButton,
        TextMeshProUGUI costLabel,
        TextMeshProUGUI buttonLabel)
    {
        WeaponUnlockDefinition weapon = config.GetWeapon(weaponIndex);
        if (weapon == null)
        {
            return;
        }

        if (weaponImage != null && weapon.weaponSprite != null)
        {
            weaponImage.sprite = weapon.weaponSprite;
        }

        if (weaponNameLabel != null)
        {
            weaponNameLabel.text = weapon.weaponName;
        }

        if (unlockButton == null)
        {
            return;
        }

        bool isUnlocked = PlayerUpgradeSystem.IsWeaponUnlocked(weaponIndex);
        UpgradeResourceCost cost = weapon.unlockCost;

        unlockButton.interactable = !isUnlocked && PlayerUpgradeSystem.CanAfford(cost);

        if (costLabel != null)
        {
            costLabel.text = isUnlocked ? "Unlocked" : cost.coins.ToString();
        }

        if (buttonLabel != null)
        {
            buttonLabel.text = isUnlocked ? "Unlocked" : "Unlock";
        }
    }

    private void RefreshStatUpgrades()
    {
        if (gearFlowText != null)
        {
            gearFlowText.text = $"{PlayerUpgradeSystem.CurrentGearFlowValue:0.00}s";
        }

        if (baseHealthText != null)
        {
            baseHealthText.text = PlayerUpgradeSystem.CurrentBaseHealthValue.ToString();
        }

        RefreshUpgradeButton(
            gearFlowUpgradeButton,
            gearFlowUpgradeCostText,
            PlayerUpgradeSystem.CanUpgradeGearFlow(),
            PlayerUpgradeSystem.GetGearFlowUpgradeCost());

        RefreshUpgradeButton(
            baseHealthUpgradeButton,
            baseHealthUpgradeCostText,
            PlayerUpgradeSystem.CanUpgradeBaseHealth(),
            PlayerUpgradeSystem.GetBaseHealthUpgradeCost());
    }

    private void RefreshUpgradeButton(
        Button button,
        TextMeshProUGUI costLabel,
        bool canUpgrade,
        UpgradeResourceCost cost)
    {
        if (button != null)
        {
            button.interactable = canUpgrade && PlayerUpgradeSystem.CanAfford(cost);
        }

        if (costLabel != null)
        {
            costLabel.text = canUpgrade ? cost.coins.ToString() : "MAX";
        }
    }

    private void CacheButtonLabels()
    {
        weapon2ButtonLabel = FindButtonLabel(weapon2BuyButton, weapon2CostText);
        weapon3ButtonLabel = FindButtonLabel(weapon3BuyButton, weapon3CostText);
    }

    private TextMeshProUGUI FindButtonLabel(Button button, TextMeshProUGUI excludedLabel)
    {
        if (button == null)
        {
            return null;
        }

        TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && labels[i] != excludedLabel)
            {
                return labels[i];
            }
        }

        return null;
    }
}
