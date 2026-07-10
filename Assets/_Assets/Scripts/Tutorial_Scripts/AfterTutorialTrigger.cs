using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Tutorial/Triggers/AfterTutorial")]
public class AfterTutorialTrigger : TutorialTrigger
{
    public TutorialType RequiredTutorialType;

    public override void Initialize(Action onTriggered)
    {
        TutorialManager.OnTutorialCompleted += (id) =>
        {
            if (id == RequiredTutorialType)
            {
                onTriggered?.Invoke();
            }
        };
    }
}
