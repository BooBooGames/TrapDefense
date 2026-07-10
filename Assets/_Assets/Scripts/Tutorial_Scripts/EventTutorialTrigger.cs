using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Triggers/Event")]
public class EventTutorialTrigger : TutorialTrigger
{
    public TutorialEventType _TutorialEventType;

    public override void Initialize(Action onTriggered)
    {
        TutorialManager.OnEventOccurred += pTutorialEventType =>
        {
            if (pTutorialEventType == _TutorialEventType)
            {
                onTriggered?.Invoke();
            }
        };
    }
}
