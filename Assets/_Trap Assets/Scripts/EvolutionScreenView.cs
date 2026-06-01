using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionScreenView : MonoBehaviour
{
    [Header("Evolution")] public TextMeshProUGUI timelineText;
    public Image timeline1Image, timeline2Image;
    public Button evolveButton;
    public TextMeshProUGUI descriptionText1, descriptionText2, evolutionCostText;

    public void ShowEvolutionBackground(UpgradeScreenConfig config)
    {
        gameObject.SetActive(true);
        RefreshEvolution(config);
    }

    private void RefreshEvolution(UpgradeScreenConfig config)
    {
        timeline1Image.sprite = config.timeline1Sprite;
        timeline2Image.sprite = config.timeline2Sprite;
        descriptionText1.text = config.evolutionDescription1;
        descriptionText2.text = config.evolutionDescription2;
        evolutionCostText.text = CoinFormatter.FormatCoins(config.evolutionCost);

        UpgradeResourceCost evolutionCost = new UpgradeResourceCost
        {
            coins = Mathf.Max(0, config.evolutionCost),
            gears = 0,
        };

        evolveButton.interactable = PlayerUpgradeSystem.CanAfford(evolutionCost);
    }

}
