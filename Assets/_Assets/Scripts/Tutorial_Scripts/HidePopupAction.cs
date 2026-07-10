using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Actions/End/HidePopup")]
public class HidePopupAction : TutorialEndAction
{
    public TutorialPopupType _PopupType;
    public HudObjectType _HudButtonType;

    public override void Execute()
    {
        TutorialActionsHandler.HideTutorialPopup(_PopupType);
        TutorialActionsHandler.ShowAllHudItems(_HudButtonType);
    }
}
