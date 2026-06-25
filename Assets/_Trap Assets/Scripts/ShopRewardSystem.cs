using System;
using System.Collections.Generic;
using UnityEngine;

public static class ShopRewardSystem
{
    public static bool GrantItem(ShopItemDefinition item)
    {
        if (item == null)
        {
            return false;
        }

        bool alreadyPurchased = IsNonConsumablePurchased(item);
        bool grantConsumableRewards = !alreadyPurchased;
        ShopRewardDefinition[] rewards = item.rewards ?? Array.Empty<ShopRewardDefinition>();

        for (int i = 0; i < rewards.Length; i++)
        {
            GrantReward(rewards[i], grantConsumableRewards);
        }

        if (item.IsNonConsumableIap && !alreadyPurchased)
        {
            MarkNonConsumablePurchased(item);
        }

        return true;
    }

    public static bool IsNonConsumablePurchased(ShopItemDefinition item)
    {
        if (item == null || !item.IsNonConsumableIap)
        {
            return false;
        }

        string purchaseKey = GetPurchaseKey(item);
        if (string.IsNullOrWhiteSpace(purchaseKey))
        {
            return false;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        string[] purchasedIds = saveData.purchasedShopProductIds ?? Array.Empty<string>();
        for (int i = 0; i < purchasedIds.Length; i++)
        {
            if (string.Equals(purchasedIds[i], purchaseKey, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void GrantReward(ShopRewardDefinition reward, bool grantConsumableReward)
    {
        if (reward == null)
        {
            return;
        }

        int amount = Mathf.Max(0, reward.amount);
        switch (reward.rewardType)
        {
            case ShopRewardType.RemoveAds:
                PlayerCurrencySystem.SetRemoveAdsActive(true);
                break;
            case ShopRewardType.UnlimitedSpeedBoost:
                GameplaySpeedSystem.ActivateUnlimitedBoost();
                break;
            case ShopRewardType.PermanentCoinMultiplier:
                PlayerCurrencySystem.SetPermanentCoinMultiplier(amount);
                break;
            case ShopRewardType.Coins:
                if (grantConsumableReward) PlayerCurrencySystem.AddCoins(amount);
                break;
            case ShopRewardType.Gems:
                if (grantConsumableReward) PlayerCurrencySystem.AddGems(amount);
                break;
            case ShopRewardType.Elixir:
                if (grantConsumableReward) PlayerCurrencySystem.AddElixir(amount);
                break;
            case ShopRewardType.SkipAds:
                if (grantConsumableReward) PlayerCurrencySystem.AddSkipAds(amount);
                break;
            case ShopRewardType.Chest:
                if (grantConsumableReward) GrantChests(reward.chestType, amount);
                break;
        }
    }

    private static void GrantChests(ChestType chestType, int amount)
    {
        HomeViewScreen.AwardShopChests(chestType, amount);
    }

    private static void MarkNonConsumablePurchased(ShopItemDefinition item)
    {
        string purchaseKey = GetPurchaseKey(item);
        if (string.IsNullOrWhiteSpace(purchaseKey))
        {
            return;
        }

        SaveGameData saveData = GameSaveSystem.Load();
        List<string> purchasedIds = new List<string>(saveData.purchasedShopProductIds ?? Array.Empty<string>());
        for (int i = 0; i < purchasedIds.Count; i++)
        {
            if (string.Equals(purchasedIds[i], purchaseKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        purchasedIds.Add(purchaseKey);
        saveData.purchasedShopProductIds = purchasedIds.ToArray();
        GameSaveSystem.Save(saveData);
    }

    private static string GetPurchaseKey(ShopItemDefinition item)
    {
        return !string.IsNullOrWhiteSpace(item.productId) ? item.productId.Trim() : item.itemId;
    }
}
