using UnityEngine;

public abstract class TutorialConstraint : ScriptableObject
{
    public abstract void Begin(System.Action onComplete);
    public abstract void End();
}
