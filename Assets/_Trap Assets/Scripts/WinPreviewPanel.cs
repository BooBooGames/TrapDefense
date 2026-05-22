using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WinPreviewPanel : MonoBehaviour
{
    private const float SliderMinValue = 0.08f;
    private const float SliderMaxValue = 0.918f;
    private const float SliderPingPongDuration = 1.25f;
    private static readonly int[] RewardMultipliers = { 2, 3, 4, 3, 2 };

    [FormerlySerializedAs("coinRewardLabel")] public TextMeshProUGUI inGameCoinLabel;
    public TextMeshProUGUI elixirRewardLabel;
    public Button collectButton, rvCollectButton;
    public Slider rewardProgressSlider;

    private bool isSliderAnimating;
    private float sliderDirection = 1f;

    public void Show(int coins, UnityAction onCollect, UnityAction<int> onRewardedCollect)
    {
        gameObject.SetActive(true);
        inGameCoinLabel.text = CoinFormatter.FormatCoins(coins);
        rewardProgressSlider.value = SliderMinValue;
        sliderDirection = 1f;
        isSliderAnimating = true;

        SetButton(collectButton, onCollect);
        SetButton(rvCollectButton, () => onRewardedCollect.Invoke(GetCurrentRewardMultiplier()));
    }

    public void Hide()
    {
        isSliderAnimating = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isSliderAnimating)
        {
            return;
        }

        float sliderRange = SliderMaxValue - SliderMinValue;
        float step = sliderRange / SliderPingPongDuration * Time.unscaledDeltaTime * sliderDirection;
        rewardProgressSlider.value += step;

        if (rewardProgressSlider.value >= SliderMaxValue)
        {
            rewardProgressSlider.value = SliderMaxValue;
            sliderDirection = -1f;
        }
        else if (rewardProgressSlider.value <= SliderMinValue)
        {
            rewardProgressSlider.value = SliderMinValue;
            sliderDirection = 1f;
        }
    }

    private static void SetButton(Button button, UnityAction callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }

    private int GetCurrentRewardMultiplier()
    {
        int sectionIndex = Mathf.Min(
            RewardMultipliers.Length - 1,
            Mathf.FloorToInt(Mathf.Clamp01(rewardProgressSlider.value) * RewardMultipliers.Length));

        return RewardMultipliers[sectionIndex];
    }
}
