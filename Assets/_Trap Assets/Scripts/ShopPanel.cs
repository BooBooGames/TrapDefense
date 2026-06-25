using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    [SerializeField] private ShopCatalog shopCatalog;

    public Button coinCollectUsingGemsButton1, coinCollectUsingGemsButton2, coinCollectUsingGemsButton3;

    public Button gemsPack1Button, gemsPack2Button, gemsPack3Button, gemsPack4Button, gemsPack5Button, gemsPack6Button, rvGemsButton;
    public TextMeshProUGUI gemsPack1ButtonText, gemsPack2ButtonText, gemsPack3ButtonText, gemsPack4ButtonText, gemsPack5ButtonText, gemsPack6ButtonText, rvGemsButtonText;
    public Button elixirPack1Button, elixirPack2Button, elixirPack3Button;
    public TextMeshProUGUI elixirPack1ButtonText, elixirPack2ButtonText, elixirPack3ButtonText;
    public Button rvTicketPack1Button, rvTicketPack2Button, rvTicketPack3Button, rvTicketPack4Button, rvTicketPack5Button, rvTicketPack6Button;
    public TextMeshProUGUI rvTicketPack1ButtonText, rvTicketPack2ButtonText, rvTicketPack3ButtonText, rvTicketPack4ButtonText, rvTicketPack5ButtonText, rvTicketPack6ButtonText;
    public Button speedBoosterPackButton;
    public TextMeshProUGUI speedBoosterPackButtonText;
    public Button premiumCoinPackButton;
    public TextMeshProUGUI premiumCoinPackButtonText;
    public Button starterCoinCoinsPackButton;
    public TextMeshProUGUI starterCoinPackButtonText;
    public Button removeAdsButton;
    public TextMeshProUGUI removeAdsButtonText;


    private readonly List<ShopButtonBinding> buttonBindings = new List<ShopButtonBinding>();
    private readonly Dictionary<Button, UnityAction> boundButtonActions = new Dictionary<Button, UnityAction>();
    private ShopCatalog activeCatalog;
    private ShopIapService iapService;
    private bool isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        PlayerCurrencySystem.CurrencyChanged += HandleCurrencyChanged;
        RefreshAllButtonStates();
    }

    private void OnDisable()
    {
        PlayerCurrencySystem.CurrencyChanged -= HandleCurrencyChanged;
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void HandleCurrencyChanged(int _, int __)
    {
        RefreshAllButtonStates();
    }

    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        activeCatalog = ResolveCatalog();
        iapService = GetComponent<ShopIapService>();
        if (iapService == null)
        {
            iapService = gameObject.AddComponent<ShopIapService>();
        }

        iapService.Initialize(activeCatalog, HandleIapPurchaseSucceeded);
        BindButtons();
        isInitialized = true;
    }

    private ShopCatalog ResolveCatalog()
    {
        if (shopCatalog != null)
        {
            return shopCatalog;
        }

        ShopCatalog loadedCatalog = Resources.Load<ShopCatalog>("ShopCatalog");
        return loadedCatalog != null ? loadedCatalog : ShopCatalog.CreateRuntimeDefault();
    }

    private Button ResolveButton(Button currentButton, params string[] names)
    {
        if (currentButton != null)
        {
            return currentButton;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            if (MatchesAnyName(button.gameObject.name, names))
            {
                return button;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null || !MatchesAnyName(label.text, names))
            {
                continue;
            }

            return button;
        }

        return null;
    }

    private static bool MatchesAnyName(string value, string[] names)
    {
        string normalizedValue = NormalizeName(value);
        for (int i = 0; i < names.Length; i++)
        {
            if (normalizedValue == NormalizeName(names[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeName(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
    }

    private void BindButtons()
    {
        buttonBindings.Clear();

        BindButton(coinCollectUsingGemsButton1, null, "coins_pack_1");
        BindButton(coinCollectUsingGemsButton2, null, "coins_pack_2");
        BindButton(coinCollectUsingGemsButton3, null, "coins_pack_3");

        BindButton(gemsPack1Button, gemsPack1ButtonText, "gems_pack_1");
        BindButton(gemsPack2Button, gemsPack2ButtonText, "gems_pack_2");
        BindButton(gemsPack3Button, gemsPack3ButtonText, "gems_pack_3");
        BindButton(gemsPack4Button, gemsPack4ButtonText, "gems_pack_4");
        BindButton(gemsPack5Button, gemsPack5ButtonText, "gems_pack_5");
        BindButton(gemsPack6Button, gemsPack6ButtonText, "gems_pack_6");
        BindButton(rvGemsButton, rvGemsButtonText, "gems_rv_1");

        BindButton(elixirPack1Button, elixirPack1ButtonText, "elixir_pack_1");
        BindButton(elixirPack2Button, elixirPack2ButtonText, "elixir_pack_2");
        BindButton(elixirPack3Button, elixirPack3ButtonText, "elixir_pack_3");

        BindButton(rvTicketPack1Button, rvTicketPack1ButtonText, "skip_ads_pack_1");
        BindButton(rvTicketPack2Button, rvTicketPack2ButtonText, "skip_ads_pack_2");
        BindButton(rvTicketPack3Button, rvTicketPack3ButtonText, "skip_ads_pack_3");
        BindButton(rvTicketPack4Button, rvTicketPack4ButtonText, "skip_ads_pack_4");
        BindButton(rvTicketPack5Button, rvTicketPack5ButtonText, "skip_ads_pack_5");
        BindButton(rvTicketPack6Button, rvTicketPack6ButtonText, "skip_ads_pack_6");

        BindButton(speedBoosterPackButton, speedBoosterPackButtonText, "speed_booster");
        BindButton(premiumCoinPackButton, premiumCoinPackButtonText, "premium_coins");
        BindButton(starterCoinCoinsPackButton, starterCoinPackButtonText, "starter_coin");
        BindButton(removeAdsButton, removeAdsButtonText, "remove_ads");
    }

    private void BindButton(Button button, TextMeshProUGUI priceLabel, string itemId)
    {
        if (button == null)
        {
            return;
        }

        if (boundButtonActions.TryGetValue(button, out UnityAction oldAction))
        {
            button.onClick.RemoveListener(oldAction);
            boundButtonActions.Remove(button);
        }

        ShopButtonBinding binding = new ShopButtonBinding(button, priceLabel, itemId);
        buttonBindings.Add(binding);

        UnityAction action = () => HandleShopButtonClicked(itemId);
        boundButtonActions[button] = action;
        button.onClick.AddListener(action);
        RefreshButtonState(binding);
    }

    private void UnbindButtons()
    {
        foreach (KeyValuePair<Button, UnityAction> boundAction in boundButtonActions)
        {
            if (boundAction.Key != null)
            {
                boundAction.Key.onClick.RemoveListener(boundAction.Value);
            }
        }

        boundButtonActions.Clear();
        buttonBindings.Clear();
    }

    private void HandleShopButtonClicked(string itemId)
    {
        ShopItemDefinition item = activeCatalog != null ? activeCatalog.FindByItemId(itemId) : null;
        if (item == null || !item.isEnabled)
        {
            return;
        }

        SoundManager.Instance?.PlayButtonClickSound();

        if (item.IsNonConsumableIap && ShopRewardSystem.IsNonConsumablePurchased(item))
        {
            RefreshAllButtonStates();
            return;
        }

        switch (item.purchaseType)
        {
            case ShopPurchaseType.Iap:
                iapService?.Purchase(item);
                break;
            case ShopPurchaseType.SoftCurrency:
                if (TrySpendSoftCurrency(item))
                {
                    GrantShopItem(item);
                }
                break;
            case ShopPurchaseType.RewardedVideo:
                GrantShopItem(item);
                break;
        }
    }

    private bool TrySpendSoftCurrency(ShopItemDefinition item)
    {
        switch (item.costCurrency)
        {
            case ShopCurrencyType.Gems:
                return PlayerCurrencySystem.TrySpendGems(item.costAmount);
            case ShopCurrencyType.None:
                return true;
            default:
                Debug.LogWarning($"Unsupported shop soft currency: {item.costCurrency}");
                return false;
        }
    }

    private void HandleIapPurchaseSucceeded(string productId)
    {
        ShopItemDefinition item = activeCatalog != null ? activeCatalog.FindByProductId(productId) : null;
        if (item == null)
        {
            Debug.LogWarning($"Purchased product is not present in ShopCatalog: {productId}");
            return;
        }

        GrantShopItem(item);
    }

    private void GrantShopItem(ShopItemDefinition item)
    {
        ShopRewardSystem.GrantItem(item);
        RefreshAllButtonStates();
    }

    private void RefreshAllButtonStates()
    {
        for (int i = 0; i < buttonBindings.Count; i++)
        {
            RefreshButtonState(buttonBindings[i]);
        }
    }

    private void RefreshButtonState(ShopButtonBinding binding)
    {
        if (binding == null || binding.button == null)
        {
            return;
        }

        ShopItemDefinition item = activeCatalog != null ? activeCatalog.FindByItemId(binding.itemId) : null;
        if (item == null || !item.isEnabled)
        {
            binding.button.interactable = false;
            SetPriceLabel(binding, string.Empty);
            return;
        }

        bool isOwned = item.IsNonConsumableIap && ShopRewardSystem.IsNonConsumablePurchased(item);
        binding.button.interactable = !isOwned && CanStartPurchase(item);
        SetPriceLabel(binding, isOwned ? "Owned" : item.GetDisplayPrice());
    }

    private bool CanStartPurchase(ShopItemDefinition item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.purchaseType != ShopPurchaseType.SoftCurrency)
        {
            return true;
        }

        return item.costCurrency switch
        {
            ShopCurrencyType.Gems => PlayerCurrencySystem.Gems >= item.costAmount,
            ShopCurrencyType.None => true,
            _ => false,
        };
    }

    private static void SetPriceLabel(ShopButtonBinding binding, string value)
    {
        TextMeshProUGUI label = binding.priceLabel != null
            ? binding.priceLabel
            : binding.button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.text = value;
        }
    }

    private class ShopButtonBinding
    {
        public readonly Button button;
        public readonly TextMeshProUGUI priceLabel;
        public readonly string itemId;

        public ShopButtonBinding(Button button, TextMeshProUGUI priceLabel, string itemId)
        {
            this.button = button;
            this.priceLabel = priceLabel;
            this.itemId = itemId;
        }
    }
}
