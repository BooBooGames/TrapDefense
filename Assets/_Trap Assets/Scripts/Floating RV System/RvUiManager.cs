using System;
using System.Collections.Generic;
using UnityEngine;

public class RvUiManager : MonoBehaviour
{
    [SerializeField] private RvUI[] _rvUiArray;
    [SerializeField] private RvEffectSystem _rvEffectSystem;

    private Action<RvUI, RvRequestSO> OnStartRvEffect;
    private Action<RvRequestSO> OnRvEffectCompleted;

    private List<RvUI> _activeRVs = new();
    private Dictionary<RvRequestSO, RvUI> _rvRequestToUiMap = new();

    private void Awake()
    {
        foreach(RvUI rvUI in _rvUiArray)
        {
            rvUI.gameObject.SetActive(false);
        }
    }

    private RvUI GetFreeSlot()
    {
        foreach(RvUI rvUI in _rvUiArray)
        {
            if(rvUI.IsAvailable())
            {
                return rvUI;
            }
        }

        return null;
    }

    private void RemoveFromActiveRVs(RvUI pRvUI, RvRequestSO pRvRequest)
    {
        _activeRVs.Remove(pRvUI);
        _rvRequestToUiMap.Remove(pRvRequest);
    }

    private void OnCompleted(RvUI pRvUI, RvRequestSO pRvRequest)
    {
        OnRvEffectCompleted?.Invoke(pRvRequest);
        RemoveFromActiveRVs(pRvUI, pRvRequest);
    }

    private void OnExpired(RvUI pRvUI, RvRequestSO pRvRequest)
    {
        RemoveFromActiveRVs(pRvUI, pRvRequest);
    }

    private void HandleOnStartRvEffect(RvUI pRvUI, RvRequestSO pRvRequest)
    {
        OnStartRvEffect?.Invoke(pRvUI, pRvRequest);
        _rvEffectSystem.StartEffect(pRvRequest);
    }

    public void Show(RvRequestSO pRvRequest, Action<RvUI, RvRequestSO> pOnStartRvEffect, Action<RvRequestSO> pOnRvEffectComplete)
    {
        RvUI slot = GetFreeSlot();

        _rvRequestToUiMap.Add(pRvRequest, slot);
        slot.Show(HandleOnStartRvEffect, pRvRequest, OnCompleted, OnExpired);

        OnStartRvEffect = pOnStartRvEffect;
        OnRvEffectCompleted = pOnRvEffectComplete;
        _activeRVs.Add(slot);
    }

    public void HandleOnReset()
    {
        foreach(RvUI rvUI in _rvUiArray)
        {
            rvUI.ForceExpire();
        }

        _activeRVs = new();
        _rvRequestToUiMap = new();
    }

    public int GetTotalActiveCount() => _activeRVs.Count;
}
