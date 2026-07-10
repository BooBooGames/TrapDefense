using UnityEngine;

public class UIHoleRaycast : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private RectTransform holeTarget;

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (holeTarget == null) return true;

        bool insideHole = RectTransformUtility.RectangleContainsScreenPoint(
            holeTarget,
            screenPoint,
            eventCamera
        );

        return !insideHole;
    }
}
