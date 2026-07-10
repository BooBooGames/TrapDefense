using System.Collections.Generic;
using UnityEngine;
using System;

public enum TutorialType
{
    WeaponDragSystem,
}

public enum HudObjectType
{
    None,
    GameViewScreen,
    FloatingRVs,
    Main_HUD
}

public enum TutorialPopupType
{
    WeaponPlacementSystem = 500,

    None = 1000000,
}

public enum TutorialEventType
{
    StartingGameForFirstTime,
    DraggedWeapon,
}

public enum TutorialStepStartAction
{
    TurnInputOn,
    TurnInputOff,
    UnpauseGameplay,
    PauseGameplay,
}

[CreateAssetMenu(menuName = "Tutorial/Tutorial")]
public class TutorialData : ScriptableObject
{
    public TutorialType _TutorialType;
    public List<TutorialStepData> TutorialSteps;

    public TutorialTrigger StartTrigger;
}