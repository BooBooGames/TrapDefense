using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerCardCatalog", menuName = "TrapDefense/XP/Power Card Catalog")]
public class PowerCardCatalog : ScriptableObject
{
    [SerializeField] private PowerCardDefinition[] cards = Array.Empty<PowerCardDefinition>();

    public PowerCardDefinition[] Cards => cards;
}

[Serializable]
public class PowerCardDefinition
{
    public string cardId;
    public string cardType;
    public string cardName;
    public Sprite cardImage;
    [TextArea] public string[] descriptions = Array.Empty<string>();

    public string[] GetDescriptions()
    {
        if (descriptions == null || descriptions.Length == 0)
        {
            return Array.Empty<string>();
        }

        return descriptions;
    }

    public static PowerCardDefinition CreateFallback(string displayName)
    {
        return new PowerCardDefinition
        {
            cardId = displayName.Replace(" ", string.Empty),
            cardType = "Power",
            cardName = displayName,
            cardImage = null,
            descriptions = new[] { "Effect will be added later." }
        };
    }
}