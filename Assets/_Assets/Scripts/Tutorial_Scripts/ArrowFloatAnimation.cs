using DG.Tweening;
using UnityEngine;

public class ArrowFloatAnimation : MonoBehaviour
{
    [SerializeField] private float moveAmount = 1f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool moveVertically = true;
    [SerializeField] private Vector3 direction;

    private void Start()
    {
        if (moveVertically)
        {
            transform.DOMoveY(moveAmount, duration)
                .SetRelative(true)
                .SetUpdate(UpdateType.Normal, true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            transform.DOMove(direction.normalized * moveAmount, duration)
                .SetRelative(true)
                .SetUpdate(UpdateType.Normal, true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}
