using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FailPreviewPanel : MonoBehaviour
{
    [FormerlySerializedAs("coinRewardLabel")] public TextMeshProUGUI inGameCoinLabel;
    [FormerlySerializedAs("collectButton")] public Button tryAgainButton;
    public Button reviveButton;
    public GameObject elixirHolder;
    public TextMeshProUGUI elixirAmountLabel;
    public Button rvCollectButton;

    public void Show(int coins, int elixirReward, UnityAction onCollect, UnityAction onRewardedCollect, UnityAction onRevive)
    {
        gameObject.SetActive(true);
        inGameCoinLabel.text = CoinFormatter.FormatCoins(coins);
        RefreshElixirReward(elixirReward);

        SetButton(tryAgainButton, onCollect);
        SetButton(rvCollectButton, onRewardedCollect);
        SetButton(reviveButton, onRevive);
        /*  if (rvCollectButton != null)
         {
             rvCollectButton.gameObject.SetActive(false);
         } */
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void RefreshElixirReward(int elixirReward)
    {
        bool hasElixirReward = elixirReward > 0;

        if (elixirHolder != null)
        {
            elixirHolder.SetActive(hasElixirReward);
        }

        if (elixirAmountLabel != null)
        {
            elixirAmountLabel.text = hasElixirReward ? elixirReward.ToString() : string.Empty;
        }
    }

    private static void SetButton(Button button, UnityAction callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }
}
