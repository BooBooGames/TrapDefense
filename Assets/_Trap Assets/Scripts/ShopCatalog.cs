using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopCatalog", menuName = "TrapDefense/Shop/Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    [SerializeField] private ShopItemDefinition[] items = Array.Empty<ShopItemDefinition>();

    public ShopItemDefinition[] Items => items ?? Array.Empty<ShopItemDefinition>();

    public ShopItemDefinition FindByItemId(string itemId)
    {
        return Find(item => string.Equals(item.itemId, itemId, StringComparison.OrdinalIgnoreCase));
    }

    public ShopItemDefinition FindByProductId(string productId)
    {
        return Find(item => string.Equals(item.productId, productId, StringComparison.OrdinalIgnoreCase));
    }

    public static ShopCatalog CreateRuntimeDefault()
    {
        ShopCatalog catalog = CreateInstance<ShopCatalog>();
        catalog.items = CreateDefaultItems();
        return catalog;
    }

    public static ShopItemDefinition[] CreateDefaultItems()
    {
        return new[]
        {
            Iap("remove_ads", "remove_ads", "Remove Ads", ShopItemCategory.RemoveAds, ShopIapProductKind.NonConsumable, 1100,
                Reward(ShopRewardType.RemoveAds, 1)),

            Iap("gems_pack_1", "gems_pack_1", "Gems 1", ShopItemCategory.Gems, ShopIapProductKind.Consumable, 320,
                Reward(ShopRewardType.Gems, 10)),
            Iap("gems_pack_2", "gems_pack_2", "Gems 2", ShopItemCategory.Gems, ShopIapProductKind.Consumable, 620,
                Reward(ShopRewardType.Gems, 50)),
            Iap("gems_pack_3", "gems_pack_3", "Gems 3", ShopItemCategory.Gems, ShopIapProductKind.Consumable, 1250,
                Reward(ShopRewardType.Gems, 100)),
            Iap("gems_pack_4", "gems_pack_4", "Gems 4", ShopItemCategory.Gems, ShopIapProductKind.Consumable, 2500,
                Reward(ShopRewardType.Gems, 500)),
            Iap("gems_pack_5", "gems_pack_5", "Gems 5", ShopItemCategory.Gems, ShopIapProductKind.Consumable, 5800,
                Reward(ShopRewardType.Gems, 1000)),
            Iap("gems_pack_6", "gems_pack_6", "Gems 6", ShopItemCategory.Gems, ShopIapProductKind.Consumable, 8800,
                Reward(ShopRewardType.Gems, 2500)),
            RewardedVideo("gems_rv_1", "Gems 7", ShopItemCategory.Gems, 3,
                Reward(ShopRewardType.Gems, 1)),

            SoftCurrency("coins_pack_1", "Coins 1", ShopItemCategory.Coins, 5,
                Reward(ShopRewardType.Coins, 500)),
            SoftCurrency("coins_pack_2", "Coins 2", ShopItemCategory.Coins, 50,
                Reward(ShopRewardType.Coins, 5000)),
            SoftCurrency("coins_pack_3", "Coins 3", ShopItemCategory.Coins, 250,
                Reward(ShopRewardType.Coins, 25000)),

            SoftCurrency("elixir_pack_1", "Elixir 1", ShopItemCategory.Elixir, 5,
                Reward(ShopRewardType.Elixir, 20)),
            SoftCurrency("elixir_pack_2", "Elixir 2", ShopItemCategory.Elixir, 30,
                Reward(ShopRewardType.Elixir, 120)),
            SoftCurrency("elixir_pack_3", "Elixir 3", ShopItemCategory.Elixir, 100,
                Reward(ShopRewardType.Elixir, 400)),

            Iap("skip_ads_pack_1", "skip_ads_pack_1", "Skip Ads 1", ShopItemCategory.SkipAds, ShopIapProductKind.Consumable, 220,
                Reward(ShopRewardType.SkipAds, 10)),
            Iap("skip_ads_pack_2", "skip_ads_pack_2", "Skip Ads 2", ShopItemCategory.SkipAds, ShopIapProductKind.Consumable, 550,
                Reward(ShopRewardType.SkipAds, 25)),
            Iap("skip_ads_pack_3", "skip_ads_pack_3", "Skip Ads 3", ShopItemCategory.SkipAds, ShopIapProductKind.Consumable, 1100,
                Reward(ShopRewardType.SkipAds, 60)),
            Iap("skip_ads_pack_4", "skip_ads_pack_4", "Skip Ads 4", ShopItemCategory.SkipAds, ShopIapProductKind.Consumable, 2700,
                Reward(ShopRewardType.SkipAds, 150)),
            Iap("skip_ads_pack_5", "skip_ads_pack_5", "Skip Ads 5", ShopItemCategory.SkipAds, ShopIapProductKind.Consumable, 6500,
                Reward(ShopRewardType.SkipAds, 440)),
            Iap("skip_ads_pack_6", "skip_ads_pack_6", "Skip Ads 6", ShopItemCategory.SkipAds, ShopIapProductKind.Consumable, 16200,
                Reward(ShopRewardType.SkipAds, 1000)),

            Iap("speed_booster", "speed_booster", "Speed Booster", ShopItemCategory.BasicBundle, ShopIapProductKind.NonConsumable, 1550,
                Reward(ShopRewardType.UnlimitedSpeedBoost, 1)),
            Iap("premium_coins", "premium_coins", "Premium Coins", ShopItemCategory.BasicBundle, ShopIapProductKind.NonConsumable, 1550,
                Reward(ShopRewardType.PermanentCoinMultiplier, 3),
                Reward(ShopRewardType.Gems, 30)),
            Iap("starter_coin", "starter_coin", "Starter Coin", ShopItemCategory.BasicBundle, ShopIapProductKind.NonConsumable, 1050,
                Reward(ShopRewardType.PermanentCoinMultiplier, 2),
                Reward(ShopRewardType.Gems, 15)),

            Iap("chest_frenzy", "chest_frenzy", "Chest Frenzy", ShopItemCategory.ChestBundle, ShopIapProductKind.Consumable, 2700,
                ChestReward(ChestType.Common, 1),
                ChestReward(ChestType.Rare, 1),
                ChestReward(ChestType.Epic, 1),
                ChestReward(ChestType.Magic, 1)),
            Iap("chest_storm", "chest_storm", "Chest Storm", ShopItemCategory.ChestBundle, ShopIapProductKind.Consumable, 8100,
                ChestReward(ChestType.Common, 3),
                ChestReward(ChestType.Rare, 3),
                ChestReward(ChestType.Epic, 3),
                ChestReward(ChestType.Magic, 3)),

            Iap("starter_hero_bundle", "starter_hero_bundle", "Starter Hero Bundle", ShopItemCategory.HeroBundle, ShopIapProductKind.Consumable, 850,
                Reward(ShopRewardType.Gems, 20)),
            Iap("gold_hero_bundle", "gold_hero_bundle", "Gold Hero Bundle", ShopItemCategory.HeroBundle, ShopIapProductKind.Consumable, 2700,
                Reward(ShopRewardType.Gems, 50)),
            Iap("diamond_hero_bundle", "diamond_hero_bundle", "Diamond Hero Bundle", ShopItemCategory.HeroBundle, ShopIapProductKind.Consumable, 6500,
                Reward(ShopRewardType.Gems, 100)),
        };
    }

    private ShopItemDefinition Find(Func<ShopItemDefinition, bool> predicate)
    {
        ShopItemDefinition[] resolvedItems = Items;
        for (int i = 0; i < resolvedItems.Length; i++)
        {
            ShopItemDefinition item = resolvedItems[i];
            if (item != null && predicate(item))
            {
                return item;
            }
        }

        return null;
    }

    private static ShopItemDefinition Iap(
        string itemId,
        string productId,
        string displayName,
        ShopItemCategory category,
        ShopIapProductKind productKind,
        int cost,
        params ShopRewardDefinition[] rewards)
    {
        return new ShopItemDefinition
        {
            itemId = itemId,
            productId = productId,
            displayName = displayName,
            category = category,
            purchaseType = ShopPurchaseType.Iap,
            iapProductKind = productKind,
            costCurrency = ShopCurrencyType.Iap,
            costAmount = cost,
            displayPrice = cost.ToString(),
            isEnabled = true,
            rewards = rewards
        };
    }

    private static ShopItemDefinition SoftCurrency(
        string itemId,
        string displayName,
        ShopItemCategory category,
        int gemCost,
        params ShopRewardDefinition[] rewards)
    {
        return new ShopItemDefinition
        {
            itemId = itemId,
            displayName = displayName,
            category = category,
            purchaseType = ShopPurchaseType.SoftCurrency,
            costCurrency = ShopCurrencyType.Gems,
            costAmount = gemCost,
            displayPrice = gemCost.ToString(),
            isEnabled = true,
            rewards = rewards
        };
    }

    private static ShopItemDefinition RewardedVideo(
        string itemId,
        string displayName,
        ShopItemCategory category,
        int rvCost,
        params ShopRewardDefinition[] rewards)
    {
        return new ShopItemDefinition
        {
            itemId = itemId,
            displayName = displayName,
            category = category,
            purchaseType = ShopPurchaseType.RewardedVideo,
            costCurrency = ShopCurrencyType.RewardedVideo,
            costAmount = rvCost,
            displayPrice = rvCost.ToString(),
            isEnabled = true,
            rewards = rewards
        };
    }

    private static ShopRewardDefinition Reward(ShopRewardType rewardType, int amount)
    {
        return new ShopRewardDefinition
        {
            rewardType = rewardType,
            amount = amount
        };
    }

    private static ShopRewardDefinition ChestReward(ChestType chestType, int amount)
    {
        return new ShopRewardDefinition
        {
            rewardType = ShopRewardType.Chest,
            amount = amount,
            chestType = chestType
        };
    }
}

[Serializable]
public class ShopItemDefinition
{
    public string itemId;
    public string productId;
    public string displayName;
    public ShopItemCategory category;
    public ShopPurchaseType purchaseType;
    public ShopIapProductKind iapProductKind = ShopIapProductKind.Consumable;
    public ShopCurrencyType costCurrency;
    [Min(0)] public int costAmount;
    public string displayPrice;
    public bool isEnabled = true;
    public ShopRewardDefinition[] rewards = Array.Empty<ShopRewardDefinition>();

    public bool IsIap => purchaseType == ShopPurchaseType.Iap;
    public bool IsNonConsumableIap => IsIap && iapProductKind == ShopIapProductKind.NonConsumable;

    public string GetDisplayPrice()
    {
        if (!string.IsNullOrWhiteSpace(displayPrice))
        {
            return displayPrice;
        }

        return costCurrency switch
        {
            ShopCurrencyType.Gems => costAmount.ToString(),
            ShopCurrencyType.RewardedVideo => costAmount.ToString(),
            ShopCurrencyType.Iap => costAmount.ToString(),
            _ => string.Empty,
        };
    }
}

[Serializable]
public class ShopRewardDefinition
{
    public ShopRewardType rewardType;
    [Min(0)] public int amount;
    public ChestType chestType;
    public string rewardId;
}

public enum ShopItemCategory
{
    RemoveAds,
    Gems,
    Coins,
    Elixir,
    SkipAds,
    BasicBundle,
    ChestBundle,
    HeroBundle
}

public enum ShopPurchaseType
{
    Iap,
    SoftCurrency,
    RewardedVideo
}

public enum ShopCurrencyType
{
    None,
    Iap,
    Gems,
    RewardedVideo
}

public enum ShopIapProductKind
{
    Consumable,
    NonConsumable
}

public enum ShopRewardType
{
    Coins,
    Gems,
    Elixir,
    SkipAds,
    RemoveAds,
    UnlimitedSpeedBoost,
    PermanentCoinMultiplier,
    Chest
}
