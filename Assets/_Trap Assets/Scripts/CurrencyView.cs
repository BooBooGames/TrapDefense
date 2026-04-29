using TMPro;
using UnityEngine;

public class CurrencyView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinCounterLabel;
    [SerializeField] private TextMeshProUGUI gemsCounterLabel;

    private void Awake()
    {
        AutoResolveReferences();
        PlayerCurrencySystem.Initialize(ParseLabelValue(coinCounterLabel), ParseLabelValue(gemsCounterLabel));
        Refresh(PlayerCurrencySystem.Coins, PlayerCurrencySystem.Gems);
    }

    private void OnEnable()
    {
        PlayerCurrencySystem.CurrencyChanged += HandleCurrencyChanged;
    }

    private void OnDisable()
    {
        PlayerCurrencySystem.CurrencyChanged -= HandleCurrencyChanged;
    }

    private void HandleCurrencyChanged(int coins, int gems)
    {
        Refresh(coins, gems);
    }

    private void Refresh(int coins, int gems)
    {
        if (coinCounterLabel != null)
        {
            coinCounterLabel.text = coins.ToString();
        }

        if (gemsCounterLabel != null)
        {
            gemsCounterLabel.text = gems.ToString();
        }
    }

    private void AutoResolveReferences()
    {
        if (coinCounterLabel == null)
        {
            coinCounterLabel = FindLabelByName("Coin count Text (TMP)");
        }

        if (gemsCounterLabel == null)
        {
            gemsCounterLabel = FindLabelByName("Gems count Text (TMP)");
        }
    }

    private TextMeshProUGUI FindLabelByName(string objectName)
    {
        TextMeshProUGUI[] labels = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && labels[i].name == objectName)
            {
                return labels[i];
            }
        }

        return null;
    }

    private int ParseLabelValue(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return 0;
        }

        return int.TryParse(label.text, out int parsedValue) ? Mathf.Max(0, parsedValue) : 0;
    }
}
