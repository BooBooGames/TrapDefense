using System;
using UnityEngine;

public enum ButtonClickType
{
    None,

    WeaponPurchase,

    EditWeaponPlacementButton,
    SlotButton,
    UnequipButton,
    SelectWeaponForSlotButton,
    DoneButtonInSlotsSystem,
    CancelSlotButton,
}

[CreateAssetMenu(menuName = "Tutorial/Constraints/ButtonClick")]
public class ButtonClickConstraint : TutorialConstraint
{
    public ButtonClickType ButtonClickType;

    private Action OnComplete;

    public override void Begin(Action onComplete)
    {
        TutorialButton.OnButtonClicked += HandleClick;

        OnComplete = onComplete;
    }

    private void HandleClick(ButtonClickType pButtonClickType)
    {
        if (pButtonClickType == ButtonClickType)
        {
            TutorialButton.OnButtonClicked -= HandleClick;
            OnComplete?.Invoke();
        }
    }

    public override void End()
    {
        TutorialButton.OnButtonClicked -= HandleClick;
    }
}
