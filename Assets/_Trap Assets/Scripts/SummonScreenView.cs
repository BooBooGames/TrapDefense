using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SummonScreenView : MonoBehaviour
{
    private const float RevealDuration = 0.55f;
    private const float RevealStartScale = 0.72f;
    private const float RevealOvershootScale = 1.12f;

    public Image cardBg, cardIcon;
    public Button summonx1Button, summonx10Button, tapToContinueButton;

    private Func<bool> canSummonX1;
    private Func<bool> canSummonX10;
    private Coroutine revealRoutine;
    private Vector3 cardBgBaseScale = Vector3.one;
    private Vector3 cardIconBaseScale = Vector3.one;
    private Quaternion cardBgBaseRotation = Quaternion.identity;
    private Quaternion cardIconBaseRotation = Quaternion.identity;
    private bool hasCachedBaseTransforms;
    private UnityAction currentSummonX1Action;
    private UnityAction currentSummonX10Action;
    private UnityAction currentContinueAction;

    public bool IsRevealing => revealRoutine != null;

    public void Show(
        PowerCardDefinition cardData,
        Sprite cardBackgroundSprite,
        Func<bool> canSummonX1Callback,
        Func<bool> canSummonX10Callback,
        UnityAction onSummonX1,
        UnityAction onSummonX10,
        UnityAction onContinue,
        UnityAction onRevealComplete)
    {
        gameObject.SetActive(true);
        canSummonX1 = canSummonX1Callback;
        canSummonX10 = canSummonX10Callback;

        BindButton(summonx1Button, ref currentSummonX1Action, onSummonX1);
        BindButton(summonx10Button, ref currentSummonX10Action, onSummonX10);
        BindButton(tapToContinueButton, ref currentContinueAction, onContinue);

        PlayReveal(cardData, cardBackgroundSprite, onRevealComplete);
    }

    public void Hide()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        ResetCardTransform();
        gameObject.SetActive(false);
    }

    public void RefreshButtonStates()
    {
        bool canUseButtons = revealRoutine == null;

        SetButtonInteractable(summonx1Button, canUseButtons && (canSummonX1 == null || canSummonX1()));
        SetButtonInteractable(summonx10Button, canUseButtons && (canSummonX10 == null || canSummonX10()));
        SetButtonInteractable(tapToContinueButton, canUseButtons);
    }

    private void PlayReveal(PowerCardDefinition cardData, Sprite cardBackgroundSprite, UnityAction onRevealComplete)
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
            ResetCardTransform();
        }

        revealRoutine = StartCoroutine(RunReveal(cardData, cardBackgroundSprite, onRevealComplete));
    }

    private IEnumerator RunReveal(PowerCardDefinition cardData, Sprite cardBackgroundSprite, UnityAction onRevealComplete)
    {
        CacheBaseTransforms();
        SetImage(cardBg, cardBackgroundSprite);
        SetImage(cardIcon, cardData != null ? cardData.cardImage : null);
        SetImagesAlpha(0f);
        SetCardScale(RevealStartScale);
        SetCardRotation(0f);
        RefreshButtonStates();

        float elapsed = 0f;
        while (elapsed < RevealDuration)
        {
            float progress = Mathf.Clamp01(elapsed / RevealDuration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            float scale = progress < 0.72f
                ? Mathf.Lerp(RevealStartScale, RevealOvershootScale, easedProgress / 0.72f)
                : Mathf.Lerp(RevealOvershootScale, 1f, (progress - 0.72f) / 0.28f);

            SetImagesAlpha(Mathf.SmoothStep(0f, 1f, progress));
            SetCardScale(scale);
            SetCardRotation(Mathf.Sin(progress * Mathf.PI * 2f) * 5f * (1f - progress));

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        SetImagesAlpha(1f);
        SetCardScale(1f);
        SetCardRotation(0f);

        revealRoutine = null;
        onRevealComplete?.Invoke();
        RefreshButtonStates();
    }

    private static void BindButton(Button button, ref UnityAction currentCallback, UnityAction newCallback)
    {
        if (button == null)
        {
            return;
        }

        if (currentCallback != null)
        {
            button.onClick.RemoveListener(currentCallback);
        }

        currentCallback = newCallback;
        if (currentCallback != null)
        {
            button.onClick.AddListener(currentCallback);
        }
    }

    private static void SetButtonInteractable(Button button, bool isInteractable)
    {
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private void SetImagesAlpha(float alpha)
    {
        SetImageAlpha(cardBg, alpha);
        SetImageAlpha(cardIcon, alpha);
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void SetCardScale(float scale)
    {
        if (cardBg != null)
        {
            cardBg.transform.localScale = cardBgBaseScale * scale;
        }

        if (ShouldTransformIconSeparately())
        {
            cardIcon.transform.localScale = cardIconBaseScale * scale;
        }
    }

    private void SetCardRotation(float zRotation)
    {
        Quaternion rotationOffset = Quaternion.Euler(0f, 0f, zRotation);
        if (cardBg != null)
        {
            cardBg.transform.localRotation = cardBgBaseRotation * rotationOffset;
        }

        if (ShouldTransformIconSeparately())
        {
            cardIcon.transform.localRotation = cardIconBaseRotation * rotationOffset;
        }
    }

    private void CacheBaseTransforms()
    {
        if (cardBg != null)
        {
            cardBgBaseScale = cardBg.transform.localScale;
            cardBgBaseRotation = cardBg.transform.localRotation;
        }

        if (cardIcon != null)
        {
            cardIconBaseScale = cardIcon.transform.localScale;
            cardIconBaseRotation = cardIcon.transform.localRotation;
        }

        hasCachedBaseTransforms = true;
    }

    private void ResetCardTransform()
    {
        if (!hasCachedBaseTransforms)
        {
            return;
        }

        SetCardScale(1f);
        SetCardRotation(0f);
    }

    private bool ShouldTransformIconSeparately()
    {
        return cardIcon != null && (cardBg == null || !cardIcon.transform.IsChildOf(cardBg.transform));
    }
}
