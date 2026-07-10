[System.Serializable]
public class TutorialStepData
{
    public float WaitTimeAfterStepEnd;

    public TutorialAction[] OnStepStartActions;
    public TutorialConstraint[] EndConstraints;
    public TutorialEndAction[] OnStepEndActions;

    public void OnEventOccured(TutorialEventType pTutorialEventType)
    {
        foreach (var tutorialConstraint in EndConstraints)
        {
            if (tutorialConstraint as EventConstraint)
            {
                EventConstraint eventConstraint = (EventConstraint)tutorialConstraint;

                eventConstraint.HandleEventOccurrence(pTutorialEventType);
            }
        }
    }
}