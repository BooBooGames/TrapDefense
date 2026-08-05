using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Constraints/ClickAnywhere")]
public class ClickConstraint : TutorialConstraint
{
    public float duration;

    private Coroutine _timerCoroutine;
    private Action _onComplete;

    public override void Begin(Action onComplete)
    {
        _onComplete = onComplete;
        _timerCoroutine = TutorialManager.Instance.StartCoroutine(Routine(onComplete));
    }

    public override void End()
    {
        if(_timerCoroutine != null)
        {
            TutorialManager.Instance.StopCoroutine(_timerCoroutine);
        }
    }

    public void OnEnd()
    {
        _onComplete?.Invoke();
        if (_timerCoroutine != null)
        {
            TutorialManager.Instance.StopCoroutine(_timerCoroutine);
        }
    }

    private IEnumerator Routine(Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }
}
