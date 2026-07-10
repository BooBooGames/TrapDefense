using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Constraints/Event")]
public class EventConstraint : TutorialConstraint
{
    public TutorialEventType _RequiredEventType;

    private bool _IsOngoing;
    private Action _OnComplete;

    public void HandleEventOccurrence(TutorialEventType pTutorialEventType)
    {
        if(_IsOngoing && pTutorialEventType == _RequiredEventType)
        {
            End();
        }    
    }

    public override void Begin(Action onComplete)
    {
        _IsOngoing = true;
        _OnComplete = onComplete;
    }

    public override void End()
    {
        _OnComplete?.Invoke();
        _IsOngoing = false;
    }
}
