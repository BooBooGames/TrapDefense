using System.Collections.Generic;
using UnityEngine;

public class RvManager : MonoBehaviour
{
    private static RvManager Instance;

    public static bool IsEffectActive = false;

    [SerializeField] private List<RvRequestSO> availableRvs;
    [SerializeField] private int _maxRvsAtSameTime = 4;
    [SerializeField] private float _cooldownBetweenRvs = 30f;
    [SerializeField] private RvUiManager _rvUiManager;
    [SerializeField] private RvEffectSystem _rvEffectSystem;

    private Queue<RvRequestSO> _queue = new();

    private float _cooldownTimer = 0f;
    private List<RvRequestSO> _activeEffects;

    private bool _shouldSpawnRvs = false;

    private int _instantGearsReward;

    private void Awake()
    {
        Instance = this;

        _activeEffects = new();
        _cooldownTimer = _cooldownBetweenRvs;
    }

    private void Start()
    {
        InitializeQueue();
    }

    private void Update()
    {
        if (!_shouldSpawnRvs) return;

        _cooldownTimer -= Time.deltaTime;

        if (_cooldownTimer <= 0f)
        {
            _cooldownTimer = _cooldownBetweenRvs;
            TrySpawnRV();
        }
    }

    private void InitializeQueue()
    {
        _queue.Clear();

        foreach (var rv in availableRvs)
        {
            _queue.Enqueue(rv);
        }
    }

    private void TrySpawnRV()
    {
        if (_queue.Count == 0) return;

        if (_rvUiManager.GetTotalActiveCount() >= _maxRvsAtSameTime) return;

        var request = _queue.Dequeue();

        _queue.Enqueue(request);

        int minGears = 7;
        int maxGears = 12;

        _instantGearsReward = Random.Range(minGears, maxGears);

        _rvUiManager.Show(request, OnStartEffect, OnEffectEnded);

        _cooldownTimer = _cooldownBetweenRvs;
    }

    private void OnStartEffect(RvUI pRvUI, RvRequestSO pRvRequest)
    {
        _activeEffects.Add(pRvRequest);

        if(pRvRequest._RvType == RvType.GearFlowRv)
        {
            GameViewScreen.Instance.MultiplyGearFlowSpeedWithRv(pRvRequest._EffectMultiplier);
        }
        else if(pRvRequest._RvType == RvType.TrapDamageRv)
        {
            PlayerXpSystem.Instance.SetTrapDamageMultiplierWithRv(pRvRequest._EffectMultiplier);
        }
        else if (pRvRequest._RvType == RvType.InstantGears)
        {
            PlayerXpSystem.Instance.AddGearsWithRv(pRvUI.GetInstantGearsReward());
        }
        else if (pRvRequest._RvType == RvType.FullyHeal)
        {
            GameViewScreen.Instance.FullyHealWithRv();
        }
        else if (pRvRequest._RvType == RvType.TrapAttackSpeed)
        {
            WeaponUpgradeController.ApplySpeedMultiplierToCurrentTraps(pRvRequest._EffectMultiplier);
        }
    }

    private void OnEffectEnded(RvRequestSO pRvRequest)
    {
        _activeEffects.Remove(pRvRequest);

        if (pRvRequest._RvType == RvType.GearFlowRv)
        {
            GameViewScreen.Instance.MultiplyGearFlowSpeedWithRv(1f / pRvRequest._EffectMultiplier);
        }
        else if (pRvRequest._RvType == RvType.TrapDamageRv)
        {
            PlayerXpSystem.Instance.SetTrapDamageMultiplierWithRv(1f);
        }
        else if (pRvRequest._RvType == RvType.TrapAttackSpeed)
        {
            WeaponUpgradeController.ApplySpeedMultiplierToCurrentTraps(1f / pRvRequest._EffectMultiplier);
        }
    }

    public static void ToggleRVsSpawn(bool pToggle)
    {
        Instance._shouldSpawnRvs = pToggle;

        Instance._cooldownTimer = 0f;

        Instance.InitializeQueue();

        Instance._rvUiManager.HandleOnReset();
        Instance._rvEffectSystem.HandleOnReset();

        foreach(var activeEffect in Instance._activeEffects)
        {
            Instance.OnEffectEnded(activeEffect);
        }
    }
}