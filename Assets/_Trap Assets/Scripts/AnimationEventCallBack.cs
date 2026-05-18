using UnityEngine;
using UnityEngine.Events;

public class AnimationEventCallBack : MonoBehaviour
{
    public UnityEvent onAnimationEvent;
    public void CallBack()
    {
        onAnimationEvent.Invoke();
    }
}
