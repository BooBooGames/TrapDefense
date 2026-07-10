using UnityEngine;

public abstract class TutorialTrigger : ScriptableObject
{
    public abstract void Initialize(System.Action onTriggered);
}
