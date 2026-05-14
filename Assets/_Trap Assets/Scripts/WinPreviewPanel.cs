using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WinPreviewPanel : MonoBehaviour
{
    [FormerlySerializedAs("coinRewardLabel")] public TextMeshProUGUI inGameCoinLabel;
    public TextMeshProUGUI elixirRewardLabel;
    public Button collectButton, rvCollectButton;

    public void Show(int coins, UnityAction onCollect, UnityAction onRewardedCollect)
    {
        gameObject.SetActive(true);
        inGameCoinLabel.text = CoinFormatter.FormatCoins(coins);

        SetButton(collectButton, onCollect);
        SetButton(rvCollectButton, onRewardedCollect);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static void SetButton(Button button, UnityAction callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }
}
