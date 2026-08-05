using System.Collections.Generic;
using UnityEngine;
using System;

public enum TutorialType
{
    WeaponDragSystem,
    FirstPlay,
    TrapUpgrades,
    MainUpgrades,
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
    StartGame,
    GameIntro,
    GearSystem,
    TrapUpgrade,
    GoToUpgrades,
    ExplainGearFlowUpgrade,
    ExplainMaxHealthUpgrade,
    BuyTrapsExplain,
    WeaponPlacementSystem = 500,
    SatoshiPanel,

    None = 1000000,
}

public enum TutorialEventType
{
    SecondPlay,
    StartingGameplayForFirstTime,
    StartingGameplayForSecondTime,
    DraggedWeapon,
    TrapUpgradeAvailable,
    LostGameForFirstTime,
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