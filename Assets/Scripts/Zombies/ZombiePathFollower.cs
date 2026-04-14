using System;
using UnityEngine;

public class ZombiePathFollower : MonoBehaviour
{
    [SerializeField] private ZombiePath path;
    [SerializeField] [Min(0.1f)] private float moveSpeed = 2.5f;
    [SerializeField] [Range(0.1f, 1f)] private float roadWidthUsage = 1f;
    [SerializeField] private float verticalOffset = 0.5f;
    [SerializeField] private bool orientToMovement = true;

    private float travelledDistance;
    private float lateralOffset;
    private bool initialized;
    private Action<ZombiePathFollower> onReachedEnd;

    public void Initialize(ZombiePath assignedPath, float initialDistance, int seed, Action<ZombiePathFollower> reachedEndCallback = null)
    {
        path = assignedPath;
        travelledDistance = initialDistance;
        lateralOffset = CreateStableOffset(seed);
        onReachedEnd = reachedEndCallback;
        initialized = true;

        UpdateTransform();
    }

    public void ConfigureMovement(float configuredMoveSpeed, float configuredRoadWidthUsage)
    {
        moveSpeed = Mathf.Max(0.1f, configuredMoveSpeed);
        roadWidthUsage = Mathf.Clamp(configuredRoadWidthUsage, 0.1f, 1f);
    }

    private void Start()
    {
        if (!initialized)
        {
            Initialize(path, 0f, Mathf.Abs(GetInstanceID()));
        }
    }

    private void Update()
    {
        if (path == null || !path.HasValidPath)
        {
            return;
        }

        travelledDistance += moveSpeed * Time.deltaTime;

        if (travelledDistance >= path.Length)
        {
            onReachedEnd?.Invoke(this);
            Destroy(gameObject);
            return;
        }

        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (path == null || !path.HasValidPath)
        {
            return;
        }

        Vector3 pathPosition;
        Vector3 forward;

        if (travelledDistance < 0f)
        {
            path.Evaluate(0f, out pathPosition, out forward);
            pathPosition -= forward * Mathf.Abs(travelledDistance);
        }
        else
        {
            path.EvaluateAtDistance(travelledDistance, out pathPosition, out forward);
        }

        Vector3 normalizedForward = forward.sqrMagnitude > 0.0001f ? forward.normalized : transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, normalizedForward).normalized;

        transform.position = pathPosition + (right * lateralOffset) + (Vector3.up * verticalOffset);

        if (orientToMovement && normalizedForward.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(normalizedForward, Vector3.up);
        }
    }

    private float CreateStableOffset(int seed)
    {
        float allowedHalfWidth = GetAllowedHalfWidth();
        if (allowedHalfWidth <= Mathf.Epsilon)
        {
            return 0f;
        }

        float normalized = Mathf.Repeat(seed * 0.61803398875f, 1f);
        return Mathf.Lerp(-allowedHalfWidth, allowedHalfWidth, normalized);
    }

    private float GetAllowedHalfWidth()
    {
        if (path != null)
        {
            return Mathf.Max(0f, path.UsableHalfWidth * roadWidthUsage);
        }

        return 0f;
    }
}
