using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EvolutionScreenView : MonoBehaviour
{
    private const int LevelsPerTimeline = 4;

    [Header("Evolution")] public TextMeshProUGUI timelineText;
    public Image timeline1Image, timeline2Image;
    public Button evolveButton;
    public TextMeshProUGUI descriptionText1, descriptionText2, evolutionCostText;

    [FormerlySerializedAs("fillBarCurrentAgeIcon")] public Image fillBarCurrentTimelineIcon;
    public Image fillBar, dot1, dot2, dot3, dot4;

    public Sprite UnlockedDotSprite;
    [FormerlySerializedAs("currentDotSprite")] public Sprite CurrentDotSprite;
    public Sprite LockedDotSprite;

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
        RefreshTimelineProgress(config);

        UpgradeResourceCost evolutionCost = new UpgradeResourceCost
        {
            coins = Mathf.Max(0, config.evolutionCost),
            gears = 0,
        };

        evolveButton.interactable = PlayerUpgradeSystem.CanAfford(evolutionCost);
    }

    private void RefreshTimelineProgress(UpgradeScreenConfig config)
    {
        int progressLevel = GetCurrentProgressLevel();
        int zeroBasedProgress = Mathf.Max(0, progressLevel - 1);
        int currentTimelineNumber = (zeroBasedProgress / LevelsPerTimeline) + 1;
        int currentLevelInTimeline = (zeroBasedProgress % LevelsPerTimeline) + 1;

        if (timelineText != null)
        {
            timelineText.text = $"Timeline {currentTimelineNumber}";
        }

        SetImageSprite(fillBarCurrentTimelineIcon, GetTimelineSprite(config, currentTimelineNumber));

        if (fillBar != null)
        {
            fillBar.fillAmount = Mathf.Clamp01(currentLevelInTimeline / (float)LevelsPerTimeline);
        }

        RefreshDot(dot1, 1, currentLevelInTimeline);
        RefreshDot(dot2, 2, currentLevelInTimeline);
        RefreshDot(dot3, 3, currentLevelInTimeline);
        RefreshDot(dot4, 4, currentLevelInTimeline);
    }

    private static int GetCurrentProgressLevel()
    {
        if (LevelManager.Instance != null)
        {
            return Mathf.Max(1, LevelManager.Instance.CurrentDifficultyLevel);
        }

        SaveGameData saveData = GameSaveSystem.Load();
        return Mathf.Max(1, saveData.difficultyLevel > 0 ? saveData.difficultyLevel : saveData.currentLevel);
    }

    private static Sprite GetTimelineSprite(UpgradeScreenConfig config, int currentTimelineNumber)
    {
        if (config == null)
        {
            return null;
        }

        if (currentTimelineNumber <= 1)
        {
            return config.timeline1Sprite != null ? config.timeline1Sprite : config.timeline2Sprite;
        }

        return config.timeline2Sprite != null ? config.timeline2Sprite : config.timeline1Sprite;
    }

    private void RefreshDot(Image dotImage, int dotLevel, int currentLevelInTimeline)
    {
        if (dotImage == null)
        {
            return;
        }

        Sprite dotSprite = dotLevel < currentLevelInTimeline
            ? UnlockedDotSprite
            : dotLevel == currentLevelInTimeline
                ? CurrentDotSprite
                : LockedDotSprite;

        SetImageSprite(dotImage, dotSprite);
    }

    private static void SetImageSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
        }
    }
}
