using System;
using UnityEngine;

public enum RvType
{
    GearFlowRv,
    TrapDamageRv,
    InstantGears,
    FullyHeal,
    TrapAttackSpeed,
}

[CreateAssetMenu()]
public class RvRequestSO : ScriptableObject
{
    public RvType _RvType;
    public string _RvEventName;
    public string _RvDisplayName;
    public string _RvEffectDescriptionText;

    public Sprite _RvIcon;
    public float _DisplayDuration;

    public bool _HasTimedEffect;
    public float _EffectDuration;
    public float _EffectMultiplier;
}
