using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class TranformDoTweenExtensions
{
    public class AnimationParameters
    {
        public float Duration;
        public float ScaleFactor;
        public Ease EaseValue;
    }

    public static Tween DoPump(this Transform transform, bool useUnscaledTime = false, float scaleFactor = 1.1f, float duration = 0.4f)
    {
        transform.DOKill();
        Ease animationEase = Ease.InOutQuad;
        Vector3 originalScale = transform.localScale;

        return transform.DOScale(transform.localScale * scaleFactor, duration)
            .SetEase(animationEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Normal, useUnscaledTime)
            .SetTarget(transform)
            .OnKill(() =>
            {
                transform.localScale = originalScale;
            });
    }

    /// <summary>
    /// Makes a UI element float upward randomly like a damage indicator,
    /// while fading out to 0 alpha.
    /// </summary>
    public static Tween DOFloatingDamage(
        this RectTransform rectTransform,
        float duration = 1f,
        float moveDistance = 120f,
        float randomX = 50f,
        Ease moveEase = Ease.OutCubic,
        Ease fadeEase = Ease.Linear)
    {
        Vector2 startPos = rectTransform.anchoredPosition;

        CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
        Vector2 targetPos = startPos + new Vector2(
            0f,
            moveDistance
        );

        canvasGroup.alpha = 1f;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            rectTransform.DOAnchorPos(targetPos, duration)
                .SetEase(moveEase)
        );

        sequence.Join(
            canvasGroup.DOFade(0f, duration)
                .SetEase(fadeEase)
        );

        return sequence;
    }

    public static Tween DOPlaySelectedAnimation(
        this Transform transform, 
        bool useUnscaledTime = false,
        float moveAmount = 0.025f,
        float duration = 0.4f)
    {
        transform.DOKill();

        Vector3 startPos = transform.localPosition;

        return transform
            .DOLocalMoveY(moveAmount, duration)
            .SetEase(Ease.InOutSine)
            .SetRelative(true)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Normal, useUnscaledTime)
            .SetTarget(transform).
            OnKill(() =>
            {
                transform.localPosition = startPos;
            });
    }

    public static void StopSelectedAnimation(this RectTransform rectTransform)
    {
        rectTransform.DOKill();
    }

    /// <summary>
    /// Simple punch-like scale animation for cannon recoil/shoot feedback.
    /// </summary>
    public static Tween DOShoot(
        this Transform target,
        float scaleMultiplier = 1.15f,
        float duration = 0.12f,
        Ease easeOut = Ease.OutQuad,
        Ease easeBack = Ease.InOutQuad)
    {
        target.DOKill();

        Vector3 originalScale = target.localScale;

        Vector3 targetScale = new Vector3(
            originalScale.x,
            originalScale.y * scaleMultiplier,
            originalScale.z
        );

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            target.DOScaleY(targetScale.y, duration * 0.4f)
                .SetEase(easeOut)
        );

        sequence.Append(
            target.DOScaleY(originalScale.y, duration * 0.6f)
                .SetEase(easeBack)
        );

        sequence.SetTarget(target);

        sequence.OnKill(() =>
        {
            Vector3 scale = target.localScale;
            scale.y = originalScale.y;
            target.localScale = scale;
        });

        return sequence;
    }

    public static void DoHeartbeatPump(this Transform transform)
    {
        transform.DOKill();

        float firstBeatScale = 1.15f;
        float firstBeatDuration = 0.12f;

        float secondBeatScale = 1.05f;
        float secondBeatDuration = 0.08f;

        float restScale = 1f;
        float restDuration = 0.25f;

        transform.DOScale(firstBeatScale, firstBeatDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                transform.DOScale(secondBeatScale, secondBeatDuration).SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        transform.DOScale(restScale, restDuration).SetEase(Ease.OutSine);
                    });
            })
            .SetLoops(-1, LoopType.Restart);
    }

    public static Sequence DOJellyJumpLoop(this Transform target, float jumpPower = 15f, float duration = 0.5f, int numJumps = 1, float verticalDistance = 30f)
    {
        Sequence seq = DOTween.Sequence();

        Vector3 originalScale = target.localScale;

        seq.Append(target.DOScale(
            new Vector3(originalScale.x * 1.35f, originalScale.y * 0.6f, originalScale.z),
            duration * 0.4f
        ).SetEase(Ease.InQuad));

        seq.Append(target.DOLocalJump(
            new Vector3(0, verticalDistance, 0),
            jumpPower,
            numJumps,
            duration * 0.6f
        ).SetRelative(true).SetEase(Ease.OutQuad));

        seq.Join(target.DOScale(
            new Vector3(originalScale.x * 0.8f, originalScale.y * 1.5f, originalScale.z),
            duration * 0.3f
        ).SetEase(Ease.OutQuad));

        seq.Append(target.DOLocalJump(
            new Vector3(0, -verticalDistance, 0),
            jumpPower,
            numJumps,
            duration * 0.4f
        ).SetRelative(true).SetEase(Ease.InQuad));

        seq.Join(target.DOScale(
            new Vector3(originalScale.x * 1.1f, originalScale.y * 0.85f, originalScale.z),
            duration * 0.5f
        ).SetEase(Ease.InOutSine));

        seq.Append(target.DOScale(originalScale, duration * 0.1f));

        seq.SetLoops(-1, LoopType.Restart);

        return seq;
    }

    public static Tween DoScaleUp(this Transform target, float duration = 0.25f, Ease ease = Ease.InOutSine)
    {
        target.DOKill();

        return target.DOScale(Vector3.one, duration).SetEase(ease).SetTarget(target);
    }

    public static Tween DoScaleDown(this Transform target, float duration = 0.25f, Ease ease = Ease.InOutSine)
    {
        target.DOKill();

        return target.DOScale(Vector3.zero, duration).SetEase(ease).SetTarget(target);
    }

    public static Sequence PlayQuestCompleteAnimation(this RectTransform panel, CanvasGroup canvasGroup = null)
    {
        if (canvasGroup == null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
        }

        Sequence seq = DOTween.Sequence();

        // Initial state
        panel.localScale = Vector3.one;
        Vector2 originalPos = panel.anchoredPosition;

        // Punch scale (pop effect)
        seq.Append(panel.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack));

        // Slight upward movement
        seq.Join(panel.DOAnchorPosY(originalPos.y + 40f, 0.3f).SetEase(Ease.OutQuad));

        // Fade slightly (optional reward feel)
        seq.Join(canvasGroup.DOFade(0.8f, 0.2f));

        // Settle back
        seq.Append(panel.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad));
        seq.Join(panel.DOAnchorPos(originalPos, 0.25f).SetEase(Ease.InOutQuad));
        seq.Join(canvasGroup.DOFade(1f, 0.2f));

        return seq;
    }

    public static Sequence DOFullSlotFeedback(
        this RectTransform target,
        float shakeStrength = 20f,
        float duration = 0.4f
    )
    {
        target.DOKill();

        Vector3 originalScale = target.localScale;

        Sequence seq = DOTween.Sequence();

        seq.Append(target.DOShakeAnchorPos(
            duration,
            new Vector2(shakeStrength, 0f),
            vibrato: 20,
            randomness: 90,
            snapping: false,
            fadeOut: true
        ));

        seq.Join(target.DOPunchScale(
            new Vector3(0.15f, 0.15f, 0f),
            0.3f,
            vibrato: 10,
            elasticity: 0.8f
        ));

        seq.OnComplete(() =>
        {
            target.localScale = originalScale;
        });

        seq.SetTarget(target);

        return seq;
    }

    public static Sequence DOHealFeedback(
        this Transform targetTransform,
        SpriteRenderer spriteRenderer,
        float scaleMultiplier = 1.15f,
        float flashDuration = 0.3f,
        float returnDuration = 0.3f,
        Color? healColor = null)
    {

        Color originalColor = spriteRenderer.color;
        Color flashColor = healColor ?? new Color(0.4f, 1f, 0.4f);

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            targetTransform.DOScale(
                targetTransform.localScale * scaleMultiplier,
                flashDuration));

        sequence.Join(
            spriteRenderer.DOColor(
                flashColor,
                flashDuration));

        sequence.Append(
            targetTransform.DOScale(
                targetTransform.localScale,
                returnDuration));

        sequence.Join(
            spriteRenderer.DOColor(
                originalColor,
                returnDuration));

        return sequence;
    }

    public static Tween DOClaimAnimation(this Transform target, float delay = 0.5f)
    {
        target.DOKill();

        target.localScale = Vector3.one;
        target.localRotation = Quaternion.identity;

        Sequence seq = DOTween.Sequence();

        seq.Append(target.DOScale(1.15f, 0.3f).SetEase(Ease.OutBack))
           .Append(target.DOScale(1f, 0.2f).SetEase(Ease.InOutSine))

           .Join(target.DORotate(new Vector3(0, 0, 5f), 0.15f).SetEase(Ease.InOutSine))
           .Append(target.DORotate(new Vector3(0, 0, -5f), 0.3f).SetEase(Ease.InOutSine))
           .Append(target.DORotate(Vector3.zero, 0.15f).SetEase(Ease.InOutSine));

        seq.OnKill(() =>
        {
            target.rotation = Quaternion.identity;
        });
        seq.SetTarget(target);

        return seq.SetLoops(-1, LoopType.Restart)
                  .SetDelay(delay);
    }

    public static void DoClick(this Transform transform)
    {
        DOTween.Kill(transform);

        Vector3 originalScale = transform.localScale;

        const float PRESS_SCALE = 0.93f;
        const float REBOUND_SCALE = 1.04f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            transform.DOScale(originalScale * PRESS_SCALE, 0.06f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            transform.DOScale(originalScale * REBOUND_SCALE, 0.10f)
                .SetEase(Ease.OutBack)
        );

        sequence.Append(
            transform.DOScale(originalScale, 0.08f)
                .SetEase(Ease.InOutQuad)
        );

        sequence.SetTarget(transform);
        sequence.OnKill(() =>
        {
            transform.localScale = originalScale;
        });
    }

    public static Tween DoClick(this Transform target, float duration = 0.1f, float pressedScale = 0.9f)
    {
        Sequence seq = DOTween.Sequence();
        Vector3 original = target.localScale;

        return seq
            .Append(target.DOScale(original * pressedScale, duration * 0.5f)
            .SetEase(Ease.OutQuad))
            .Append(target.DOScale(original, duration * 0.5f)
            .SetEase(Ease.OutBack))
            .SetUpdate(true)
            .OnKill(() =>
        {
            target.localScale = original;
        });
    }

    public static Tween DoSquash(this Transform target, float duration = 0.15f, float factor = 0.15f)
    {
        target.DOKill();
        Sequence seq = DOTween.Sequence();
        Vector3 original = target.localScale;

        seq.SetTarget(target);
        return seq
            .Append(target.DOScale(new Vector3(original.x + factor, original.y - factor, original.z), duration * 0.35f).SetEase(Ease.OutQuad))
            .Append(target.DOScale(original, duration * 0.65f).SetEase(Ease.OutBack))
            .OnKill(() => target.localScale = original);
    }
}
