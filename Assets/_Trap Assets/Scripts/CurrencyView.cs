using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyView : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TextMeshProUGUI coinCounterLabel;
    [SerializeField] private TextMeshProUGUI gemsCounterLabel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Image gameViewBGImage, upgradeScreenBGImage;


    private void Awake()
    {
        // AutoResolveReferences();
        PlayerCurrencySystem.Initialize(ParseLabelValue(coinCounterLabel), ParseLabelValue(gemsCounterLabel));
        Refresh(PlayerCurrencySystem.Coins, PlayerCurrencySystem.Gems);
        BindBottomHudButton(() => uiManager?.ShowSettingsScreen());
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
            coinCounterLabel.text = CoinFormatter.FormatCoins(coins);
        }

        if (gemsCounterLabel != null)
        {
            gemsCounterLabel.text = gems.ToString();
        }
    }

    public void SetGameViewBGImageVisible(bool isVisible)
    {
        if (gameViewBGImage != null)
        {
            gameViewBGImage.gameObject.SetActive(isVisible);
        }
    }

    public void SetUpgradeScreenBGImageVisible(bool isVisible)
    {
        if (upgradeScreenBGImage != null)
        {
            upgradeScreenBGImage.gameObject.SetActive(isVisible);
        }
    }

    /*     private void AutoResolveReferences()
        {
            if (coinCounterLabel == null)
            {
                coinCounterLabel = FindLabelByName("Coin count Text (TMP)");
            }

            if (gemsCounterLabel == null)
            {
                gemsCounterLabel = FindLabelByName("Gems count Text (TMP)");
            }

            if (gameViewBGImage == null)
            {
                gameViewBGImage = FindImageByName("Game view BG Image");
            }
        } */

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

    private Image FindImageByName(string objectName)
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == objectName)
            {
                return images[i];
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

        return CoinFormatter.ParseCoins(label.text);
    }

    private void BindBottomHudButton(UnityEngine.Events.UnityAction callback)
    {
        if (settingsButton == null)
        {
            return;
        }
        settingsButton.onClick.AddListener(() => callback?.Invoke());
    }
}

public static class CoinFormatter
{
    private static readonly string[] Suffixes = { "K", "M", "B" };

    public static string FormatCoins(long amount)
    {
        long absoluteAmount = System.Math.Abs(amount);
        if (absoluteAmount < 1000)
        {
            return amount.ToString(CultureInfo.InvariantCulture);
        }

        double value = absoluteAmount;
        int suffixIndex = -1;

        while (value >= 1000d && suffixIndex < Suffixes.Length - 1)
        {
            value /= 1000d;
            suffixIndex++;
        }

        double displayValue = System.Math.Floor(value * 10d) / 10d;
        string formattedValue = displayValue.ToString("0.#", CultureInfo.InvariantCulture);
        return $"{(amount < 0 ? "-" : string.Empty)}{formattedValue}{Suffixes[suffixIndex]}";
    }

    public static int ParseCoins(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        string trimmedText = text.Trim().Replace(",", string.Empty);
        float multiplier = 1f;

        for (int i = 0; i < Suffixes.Length; i++)
        {
            string suffix = Suffixes[i];
            if (!trimmedText.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            multiplier = Mathf.Pow(1000f, i + 1);
            trimmedText = trimmedText.Substring(0, trimmedText.Length - suffix.Length);
            break;
        }

        return float.TryParse(trimmedText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue)
            ? Mathf.Max(0, Mathf.RoundToInt(parsedValue * multiplier))
            : 0;
    }
}
