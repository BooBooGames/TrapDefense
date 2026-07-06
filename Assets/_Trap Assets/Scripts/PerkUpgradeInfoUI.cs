using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkUpgradeInfoUI : MonoBehaviour
{
    [HideInInspector] public string cardId, cardName, cardType;
    [HideInInspector] public Sprite cardSprite;
    [HideInInspector] public string[] descriptions = Array.Empty<string>();

    public Image bgImage, iconImage, levelBgImage;
    public TextMeshProUGUI levelText, countText;

    [SerializeField] private RectTransform _upgradeArrowRect;
    [SerializeField] private ModularProgressBar _upgradeProgressBar;

    private void Awake()
    {
        Vector2 originalPosition = _upgradeArrowRect.anchoredPosition;
        Vector3 originalScale = _upgradeArrowRect.localScale;

        Sequence seq = DOTween.Sequence();

        seq.Append(_upgradeArrowRect.DOAnchorPosY(18f, 0.4f)
            .SetRelative(true)
            .SetEase(Ease.OutQuad));

        seq.Join(_upgradeArrowRect.DOScale(originalScale * 1.15f, 0.4f)
            .SetEase(Ease.OutBack));

        seq.Append(_upgradeArrowRect.DOAnchorPosY(originalPosition.y, 0.4f)
            .SetEase(Ease.InQuad));

        seq.Join(_upgradeArrowRect.DOScale(originalScale, 0.4f)
            .SetEase(Ease.InOutSine));

        seq.AppendInterval(0.3f);
        seq.SetLoops(-1, LoopType.Restart);
    }

    public void UpdateProgress(int pCurrentCount, int pRequiredCount)
    {
        float totalProgress = pCurrentCount / (float)pRequiredCount;

        _upgradeProgressBar.SetProgress(totalProgress);
        countText.text = $"{pCurrentCount}/{pRequiredCount}";

        _upgradeArrowRect.gameObject.SetActive(pCurrentCount >= pRequiredCount);
    }
}
