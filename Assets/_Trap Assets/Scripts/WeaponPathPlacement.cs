using System.Collections.Generic;
using UnityEngine;

public class WeaponPathPlacement : MonoBehaviour
{
    private const float PlacementDistanceEpsilon = 0.001f;
    private const int FirstAllowedWaypointIndex = 1;
    private const int LastAllowedWaypointOffsetFromEnd = 3;

    private static readonly List<WeaponPathPlacement> activePlacements = new List<WeaponPathPlacement>();
    private static WeaponPathPlacement activeDrag;

    [SerializeField] private ZombiePath path;
    [SerializeField] private Camera inputCamera;
    private float minimumPathSpacing = 4f;

    private float lockedDistanceAlongPath;
    private float lockedLateralOffset;
    private float fixedHeight;
    private Quaternion rotationOffsetFromPath = Quaternion.identity;
    private bool hasCachedRotationOffset;

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

    private void OnEnable()
    {
        if (!activePlacements.Contains(this))
        {
            activePlacements.Add(this);
        }
    }

    private void OnDisable()
    {
        activePlacements.Remove(this);

        if (activeDrag == this)
        {
            activeDrag = null;
        }
    }

    private void OnValidate()
    {
        minimumPathSpacing = Mathf.Max(0f, minimumPathSpacing);
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

        float closestDistanceAlongPath = path.FindClosestDistance(transform.position, out _, out _);
        lockedDistanceAlongPath = ClampDistanceToPlacementRange(closestDistanceAlongPath);
        path.EvaluateAtDistance(lockedDistanceAlongPath, out Vector3 closestPoint, out Vector3 forward);
        CacheRotationOffset(forward);
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

        float targetDistanceAlongPath = path.FindClosestDistance(pointerWorld, out _, out _);
        lockedDistanceAlongPath = ResolveAvailableDistance(targetDistanceAlongPath);
        ApplyPlacement();
    }

    private void ApplyPlacement()
    {
        path.EvaluateAtDistance(lockedDistanceAlongPath, out Vector3 centerPoint, out Vector3 forward);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 position = centerPoint + (right * lockedLateralOffset);
        position.y = fixedHeight;
        transform.position = position;
        ApplyPathRotation(forward);
    }

    private void CacheRotationOffset(Vector3 pathForward)
    {
        Quaternion pathRotation = GetPathRotation(pathForward);
        rotationOffsetFromPath = Quaternion.Inverse(pathRotation) * transform.rotation;
        hasCachedRotationOffset = true;
    }

    private void ApplyPathRotation(Vector3 pathForward)
    {
        if (!hasCachedRotationOffset)
        {
            CacheRotationOffset(pathForward);
        }

        transform.rotation = GetPathRotation(pathForward) * rotationOffsetFromPath;
    }

    private Quaternion GetPathRotation(Vector3 pathForward)
    {
        if (pathForward.sqrMagnitude <= Mathf.Epsilon)
        {
            pathForward = transform.forward;
        }

        if (pathForward.sqrMagnitude <= Mathf.Epsilon)
        {
            pathForward = Vector3.forward;
        }

        return Quaternion.LookRotation(pathForward.normalized, Vector3.up);
    }

    private float ResolveAvailableDistance(float targetDistanceAlongPath)
    {
        GetPlacementDistanceRange(out float minimumDistance, out float maximumDistance);
        float clampedDistance = Mathf.Clamp(targetDistanceAlongPath, minimumDistance, maximumDistance);

        if (IsPlacementDistanceAvailable(clampedDistance))
        {
            return clampedDistance;
        }

        return TryFindNearestAvailableDistance(clampedDistance, minimumDistance, maximumDistance, out float availableDistance)
            ? availableDistance
            : Mathf.Clamp(lockedDistanceAlongPath, minimumDistance, maximumDistance);
    }

    private float ClampDistanceToPlacementRange(float distanceAlongPath)
    {
        GetPlacementDistanceRange(out float minimumDistance, out float maximumDistance);
        return Mathf.Clamp(distanceAlongPath, minimumDistance, maximumDistance);
    }

    private void GetPlacementDistanceRange(out float minimumDistance, out float maximumDistance)
    {
        minimumDistance = 0f;
        maximumDistance = Mathf.Max(0f, path.Length);

        int sourceWaypointCount = path.SourceWaypointCount;
        int lastAllowedWaypointIndex = sourceWaypointCount - LastAllowedWaypointOffsetFromEnd;
        if (lastAllowedWaypointIndex < FirstAllowedWaypointIndex)
        {
            return;
        }

        if (!path.TryGetSourceWaypointDistance(FirstAllowedWaypointIndex, out float startDistance) ||
            !path.TryGetSourceWaypointDistance(lastAllowedWaypointIndex, out float endDistance))
        {
            return;
        }

        minimumDistance = Mathf.Min(startDistance, endDistance);
        maximumDistance = Mathf.Max(startDistance, endDistance);
    }

    private bool TryFindNearestAvailableDistance(
        float targetDistanceAlongPath,
        float minimumDistance,
        float maximumDistance,
        out float availableDistance)
    {
        float bestAvailableDistance = Mathf.Clamp(lockedDistanceAlongPath, minimumDistance, maximumDistance);
        bool foundAvailableDistance = false;
        float bestDistanceFromTarget = float.MaxValue;

        TryCandidate(minimumDistance);
        TryCandidate(maximumDistance);

        for (int i = 0; i < activePlacements.Count; i++)
        {
            WeaponPathPlacement otherPlacement = activePlacements[i];
            if (!IsComparablePlacement(otherPlacement))
            {
                continue;
            }

            float requiredSpacing = GetRequiredSpacing(otherPlacement);
            TryCandidate(otherPlacement.lockedDistanceAlongPath - requiredSpacing);
            TryCandidate(otherPlacement.lockedDistanceAlongPath + requiredSpacing);
        }

        availableDistance = bestAvailableDistance;
        return foundAvailableDistance;

        void TryCandidate(float candidateDistance)
        {
            candidateDistance = Mathf.Clamp(candidateDistance, minimumDistance, maximumDistance);
            if (!IsPlacementDistanceAvailable(candidateDistance))
            {
                return;
            }

            float distanceFromTarget = Mathf.Abs(candidateDistance - targetDistanceAlongPath);
            if (distanceFromTarget >= bestDistanceFromTarget)
            {
                return;
            }

            bestAvailableDistance = candidateDistance;
            bestDistanceFromTarget = distanceFromTarget;
            foundAvailableDistance = true;
        }
    }

    private bool IsPlacementDistanceAvailable(float candidateDistance)
    {
        for (int i = activePlacements.Count - 1; i >= 0; i--)
        {
            WeaponPathPlacement otherPlacement = activePlacements[i];
            if (otherPlacement == null)
            {
                activePlacements.RemoveAt(i);
                continue;
            }

            if (!IsComparablePlacement(otherPlacement))
            {
                continue;
            }

            float requiredSpacing = GetRequiredSpacing(otherPlacement);
            if (Mathf.Abs(candidateDistance - otherPlacement.lockedDistanceAlongPath) < requiredSpacing - PlacementDistanceEpsilon)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsComparablePlacement(WeaponPathPlacement otherPlacement)
    {
        return otherPlacement != null &&
            otherPlacement != this &&
            otherPlacement.path == path &&
            otherPlacement.isActiveAndEnabled;
    }

    private float GetRequiredSpacing(WeaponPathPlacement otherPlacement)
    {
        return Mathf.Max(minimumPathSpacing, otherPlacement.minimumPathSpacing);
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
