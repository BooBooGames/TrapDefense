using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeScreenView : MonoBehaviour
{

    public GameObject upgradesBG, evolutionBG;

    [Header("Weapons")]
    public Image weapon1Image;
    public Image weapon2Image, weapon3Image;
    public TextMeshProUGUI weapon1Text, weapon2Text, weapon3Text;
    public Button weapon2BuyButton, weapon3BuyButton;
    public TextMeshProUGUI weapon2CostText, weapon3CostText;


    public TextMeshProUGUI gearFlowCurrentText, baseHealthCurrentText, gearFlowUpgradedText, baseHealthUpgradedText;
    public Button gearFlowUpgradeButton, baseHealthUpgradeButton;
    public TextMeshProUGUI gearFlowUpgradeCostText, baseHealthUpgradeCostText;

    private TextMeshProUGUI weapon2ButtonLabel;
    private TextMeshProUGUI weapon3ButtonLabel;



    public Button upgradeButton;

    private void Awake()
    {
        CacheButtonLabels();
        BindButtons();
        RefreshUi();
    }

    private void OnEnable()
    {
        PlayerUpgradeSystem.Initialize(null);
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
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }

    private void HandleWeapon2UnlockClicked()
    {
        if (PlayerUpgradeSystem.TryUnlockWeapon(1))
        {
            PlayUpgradeSpendEffect(weapon2BuyButton);
            UIManager.Instance.RefreshGameSceneWeaponUnlockState();
            RefreshUi();
            SoundManager.Instance.PlayButtonClickSound();
        }
    }

    private void HandleWeapon3UnlockClicked()
    {
        if (PlayerUpgradeSystem.TryUnlockWeapon(2))
        {
            PlayUpgradeSpendEffect(weapon3BuyButton);
            UIManager.Instance.RefreshGameSceneWeaponUnlockState();
            RefreshUi();
            SoundManager.Instance.PlayButtonClickSound();
        }
    }

    private void HandleGearFlowUpgradeClicked()
    {
        if (PlayerUpgradeSystem.TryUpgradeGearFlow())
        {
            PlayUpgradeSpendEffect(gearFlowUpgradeButton);
            RefreshUi();
            SoundManager.Instance.PlayButtonClickSound();
        }
    }

    private void HandleBaseHealthUpgradeClicked()
    {
        if (PlayerUpgradeSystem.TryUpgradeBaseHealth())
        {
            PlayUpgradeSpendEffect(baseHealthUpgradeButton);
            RefreshUi();
            SoundManager.Instance.PlayButtonClickSound();
        }
    }

    private static void PlayUpgradeSpendEffect(Button upgradeButton)
    {
        UIParticleEffectsManager.Instance.PlayCoinSpendEffect(upgradeButton.transform.position);
    }


    private void HandleUpgradeTabClicked()
    {
        ShowUpgradeBackground();
        SoundManager.Instance.PlayButtonClickSound();
    }

    private void ShowUpgradeBackground()
    {
        upgradesBG.SetActive(true);
        evolutionBG.SetActive(false);
    }


    private void RefreshUi()
    {
        PlayerUpgradeSystem.Initialize(null);

        UpgradeScreenConfig config = PlayerUpgradeSystem.Config;
        RefreshWeaponSlot(0, config, weapon1Image, weapon1Text, null, null, null);
        RefreshWeaponSlot(1, config, weapon2Image, weapon2Text, weapon2BuyButton, weapon2CostText, weapon2ButtonLabel);
        RefreshWeaponSlot(2, config, weapon3Image, weapon3Text, weapon3BuyButton, weapon3CostText, weapon3ButtonLabel);
        RefreshStatUpgrades();
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
        weaponImage.sprite = weapon.weaponSprite;
        weaponNameLabel.text = weapon.weaponName;

        if (unlockButton == null)
        {
            return;
        }

        bool isUnlocked = PlayerUpgradeSystem.IsWeaponUnlocked(weaponIndex);
        UpgradeResourceCost cost = weapon.unlockCost;

        unlockButton.interactable = !isUnlocked && PlayerUpgradeSystem.CanAfford(cost);
        unlockButton.transform.GetChild(0).gameObject.SetActive(!isUnlocked);

        if (costLabel != null)
        {
            costLabel.text = isUnlocked ? "USE" : CoinFormatter.FormatCoins(cost.coins);
        }

        if (buttonLabel != null)
        {
            buttonLabel.text = isUnlocked ? "Unlocked" : "Unlock";
        }

    }

    private void RefreshStatUpgrades()
    {
        UpgradeScreenConfig config = PlayerUpgradeSystem.Config;
        float currentGearFlowValue = PlayerUpgradeSystem.CurrentGearFlowValue;
        float upgradedGearFlowValue = PlayerUpgradeSystem.CanUpgradeGearFlow()
            ? config.GearFlow.EvaluateValue(PlayerUpgradeSystem.GearFlowLevel + 1)
            : currentGearFlowValue;
        int currentBaseHealthValue = PlayerUpgradeSystem.CurrentBaseHealthValue;
        int upgradedBaseHealthValue = PlayerUpgradeSystem.CanUpgradeBaseHealth()
            ? config.BaseHealth.EvaluateValue(PlayerUpgradeSystem.BaseHealthLevel + 1)
            : currentBaseHealthValue;

        gearFlowCurrentText.text = $"{currentGearFlowValue:0.00}s";
        gearFlowUpgradedText.text = $"{upgradedGearFlowValue:0.00}s";
        baseHealthCurrentText.text = currentBaseHealthValue.ToString();
        baseHealthUpgradedText.text = upgradedBaseHealthValue.ToString();

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
        button.interactable = canUpgrade && PlayerUpgradeSystem.CanAfford(cost);
        costLabel.text = canUpgrade ? CoinFormatter.FormatCoins(cost.coins) : "MAX";
    }

    private void CacheButtonLabels()
    {
        weapon2ButtonLabel = FindButtonLabel(weapon2BuyButton, weapon2CostText);
        weapon3ButtonLabel = FindButtonLabel(weapon3BuyButton, weapon3CostText);
    }

    private TextMeshProUGUI FindButtonLabel(Button button, TextMeshProUGUI excludedLabel)
    {
        TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != excludedLabel)
            {
                return labels[i];
            }
        }

        return null;
    }
}
