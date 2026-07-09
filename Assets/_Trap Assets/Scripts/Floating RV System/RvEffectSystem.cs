using System;
using System.Collections;
using UnityEngine;

public class RvEffectSystem : MonoBehaviour
{
    private Coroutine activeEffect;

    public event Action<RvRequestSO> OnStartRvEffect;
    public event Action<RvRequestSO> OnEndRvEffect;

    private IEnumerator EffectRoutine(RvRequestSO pRvRequest)
    {
        RvManager.IsEffectActive = true;

        OnStartRvEffect?.Invoke(pRvRequest);

        float timer = pRvRequest._EffectDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        OnEndRvEffect?.Invoke(pRvRequest);

        RvManager.IsEffectActive = false;
        activeEffect = null;
    }

    public void StartEffect(RvRequestSO request)
    {
        activeEffect = StartCoroutine(EffectRoutine(request));
    }

    public void HandleOnReset()
    {
        RvManager.IsEffectActive = false;
        activeEffect = null;
    }
}
