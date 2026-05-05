using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ChestConfig", menuName = "TrapDefense/Chests/Chest Config")]
public class ChestConfig : ScriptableObject
{
    [SerializeField] private ChestDefinition[] chests = Array.Empty<ChestDefinition>();

    public ChestDefinition[] Chests => chests;

    public ChestDefinition GetChest(ChestType chestType)
    {
        if (chests != null)
        {
            for (int i = 0; i < chests.Length; i++)
            {
                if (chests[i] != null && chests[i].chestType == chestType)
                {
                    return chests[i];
                }
            }
        }

        return ChestDefinition.CreateFallback(chestType);
    }
}

public enum ChestType
{
    Common = 0,
    Rare = 1,
    Epic = 2,
    Magic = 3
}

public enum ChestSlotState
{
    Empty = 0,
    Locked = 1,
    Unlocking = 2,
    ReadyToOpen = 3
}

[Serializable]
public class ChestDefinition
{
    public ChestType chestType = ChestType.Common;
    public string displayName = "Common Chest";
    [Min(0f)] public float unlockDurationSeconds = 300f;
    [Min(0)] public int unlockGemCost = 5;
    public Sprite icon;
    public ChestReward rewards = new ChestReward();

    public static ChestDefinition CreateFallback(ChestType chestType)
    {
        int tier = Mathf.Max(0, (int)chestType);
        return new ChestDefinition
        {
            chestType = chestType,
            displayName = $"{chestType} Chest",
            unlockDurationSeconds = 300f * (tier + 1),
            unlockGemCost = 5 * (tier + 1),
            rewards = new ChestReward
            {
                coins = 25 * (tier + 1),
                gems = tier,
                cards = 2 + tier
            }
        };
    }
}

[Serializable]
public class ChestReward
{
    [Min(0)] public int coins = 25;
    [Min(0)] public int gems;
    [Min(0)] public int cards = 2;
}
