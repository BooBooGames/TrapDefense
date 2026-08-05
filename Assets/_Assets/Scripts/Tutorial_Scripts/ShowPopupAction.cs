using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Actions/Start/ShowPopup")]
public class ShowPopupAction : TutorialAction
{
    public TutorialPopupType _PopupType;
    public HudObjectType[] _HudButtonTypeArray;

    public override void Execute()
    {
        TutorialActionsHandler.ShowTutorialPopup(_PopupType);

        TutorialActionsHandler.ShowTutorialHudItems(_HudButtonTypeArray);
    }
}
