using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FailPreviewPanel : MonoBehaviour
{
    [FormerlySerializedAs("coinRewardLabel")] public TextMeshProUGUI inGameCoinLabel;
    public Button collectButton, rvCollectButton;

    public void Show(int coins, UnityAction onCollect, UnityAction onRewardedCollect)
    {
        gameObject.SetActive(true);

        if (inGameCoinLabel != null)
        {
            inGameCoinLabel.text = CoinFormatter.FormatCoins(coins);
        }

        SetButton(collectButton, onCollect);
        SetButton(rvCollectButton, onRewardedCollect);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static void SetButton(Button button, UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        if (callback != null)
        {
            button.onClick.AddListener(callback);
        }
    }
}
