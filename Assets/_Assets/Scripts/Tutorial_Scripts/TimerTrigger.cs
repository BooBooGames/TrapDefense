using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/Timer")]
public class TimerTrigger : TutorialTrigger
{
    public float delay;

    public override void Initialize(Action onTriggered)
    {
        TutorialManager.Instance.StartCoroutine(TriggerRoutine(onTriggered));
    }

    private IEnumerator TriggerRoutine(Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
