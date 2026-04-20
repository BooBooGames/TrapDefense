using UnityEngine;

public class WeaponPathPlacement : MonoBehaviour
{
    private static WeaponPathPlacement activeDrag;

    [SerializeField] private ZombiePath path;
    [SerializeField] private Camera inputCamera;

    private float lockedDistanceAlongPath;
    private float lockedLateralOffset;
    private float fixedHeight;

    private void Awake()
    {
        if (path == null)
        {
            path = FindFirstObjectByType<ZombiePath>();
        }

        if (inputCamera == null)
        {
            inputCamera = Camera.main;
        }

        fixedHeight = transform.position.y;
        CacheInitialPlacement();
    }

    private void Update()
    {
        if (path == null || inputCamera == null)
        {
            return;
        }

        if (activeDrag == null)
        {
            if (PointerPressedThisFrame() && IsPointerOverThisWeapon())
            {
                activeDrag = this;
            }

            return;
        }

        if (activeDrag != this)
        {
            return;
        }

        if (PointerReleasedThisFrame())
        {
            activeDrag = null;
            return;
        }

        UpdateDraggedPlacement();
    }

    private void CacheInitialPlacement()
    {
        if (path == null || !path.HasValidPath)
        {
            return;
        }

        lockedDistanceAlongPath = path.FindClosestDistance(transform.position, out Vector3 closestPoint, out Vector3 forward);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        lockedLateralOffset = Mathf.Clamp(Vector3.Dot(transform.position - closestPoint, right), -path.UsableHalfWidth, path.UsableHalfWidth);
        ApplyPlacement();
    }

    private void UpdateDraggedPlacement()
    {
        if (!TryGetPointerWorldPosition(out Vector3 pointerWorld))
        {
            return;
        }

        lockedDistanceAlongPath = path.FindClosestDistance(pointerWorld, out _, out _);
        ApplyPlacement();
    }

    private void ApplyPlacement()
    {
        path.EvaluateAtDistance(lockedDistanceAlongPath, out Vector3 centerPoint, out Vector3 forward);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 position = centerPoint + (right * lockedLateralOffset);
        position.y = fixedHeight;
        transform.position = position;
    }

    private bool IsPointerOverThisWeapon()
    {
        Ray ray = inputCamera.ScreenPointToRay(GetPointerScreenPosition());
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, 500f))
        {
            return false;
        }

        return hitInfo.transform == transform || hitInfo.transform.IsChildOf(transform);
    }

    private bool TryGetPointerWorldPosition(out Vector3 worldPosition)
    {
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, fixedHeight, 0f));
        Ray ray = inputCamera.ScreenPointToRay(GetPointerScreenPosition());
        if (dragPlane.Raycast(ray, out float enterDistance))
        {
            worldPosition = ray.GetPoint(enterDistance);
            return true;
        }

        worldPosition = default;
        return false;
    }

    private static bool PointerPressedThisFrame()
    {
        return Input.touchCount > 0
            ? Input.GetTouch(0).phase == TouchPhase.Began
            : Input.GetMouseButtonDown(0);
    }

    private static bool PointerReleasedThisFrame()
    {
        return Input.touchCount > 0
            ? Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled
            : Input.GetMouseButtonUp(0);
    }

    private static Vector3 GetPointerScreenPosition()
    {
        return Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;
    }
}
