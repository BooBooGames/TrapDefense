using System;
using UnityEngine;

public class ZombiePathFollower : MonoBehaviour
{
    [SerializeField] private ZombiePath path;
    [SerializeField][Min(0.1f)] private float moveSpeed = 2.5f;
    [SerializeField][Range(0.1f, 1f)] private float roadWidthUsage = 1f;
    [SerializeField] private float verticalOffset = 0f;
    [SerializeField] private bool orientToMovement = true;

    private float travelledDistance;
    private float lateralOffset;
    private float stackingSpeedMultiplier = 1f;
    private float temporarySpeedMultiplier = 1f;
    private bool initialized;
    private bool movementStopped;  // permanent (death)
    private bool movementPaused;   // temporary (ice / shock)

    private Action<ZombiePathFollower> onReachedEnd;

    public void Initialize(ZombiePath assignedPath, float initialDistance, int seed,
                           Action<ZombiePathFollower> reachedEndCallback = null)
    {
        path = assignedPath;
        travelledDistance = initialDistance;
        lateralOffset = CreateStableOffset(seed);
        onReachedEnd = reachedEndCallback;
        initialized = true;
        movementStopped = false;
        movementPaused = false;
        ResetMovementSpeedMultipliers();
        UpdateTransform();
    }

    public void ConfigureMovement(float speed, float widthUsage)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
        roadWidthUsage = Mathf.Clamp(widthUsage, 0.1f, 1f);
    }

    public void SetTemporaryMovementSpeedMultiplier(float multiplier)
    {
        temporarySpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ApplyStackingMovementSpeedMultiplier(float multiplier)
    {
        stackingSpeedMultiplier = Mathf.Clamp(stackingSpeedMultiplier * multiplier, 0.05f, 1f);
    }

    public void ResetMovementSpeedMultipliers()
    {
        stackingSpeedMultiplier = 1f;
        temporarySpeedMultiplier = 1f;
    }

    /// <summary>Permanent stop used on death.</summary>
    public void StopMovement()
    {
        movementStopped = true;
        movementPaused = false;
        onReachedEnd = null;
    }

    /// <summary>Temporary pause used by Ice / Shock.</summary>
    public void PauseMovement() => movementPaused = true;

    /// <summary>Resume after a temporary pause.</summary>
    public void ResumeMovement() => movementPaused = false;

    private void Start()
    {
        if (!initialized)
            Initialize(path, 0f, Mathf.Abs(GetInstanceID()));
    }

    private void Update()
    {
        if (movementStopped || movementPaused || path == null || !path.HasValidPath) return;

        travelledDistance += moveSpeed
            * stackingSpeedMultiplier
            * temporarySpeedMultiplier
            * GameplaySpeedSystem.CurrentEnemyMovementMultiplier
            * Time.deltaTime;

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
        if (path == null || !path.HasValidPath) return;

        Vector3 pathPosition, forward;

        if (travelledDistance < 0f)
        {
            path.Evaluate(0f, out pathPosition, out forward);
            pathPosition -= forward * Mathf.Abs(travelledDistance);
        }
        else
        {
            path.EvaluateAtDistance(travelledDistance, out pathPosition, out forward);
        }

        Vector3 fwd = forward.sqrMagnitude > 0.0001f ? forward.normalized : transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        transform.position = pathPosition + right * lateralOffset + Vector3.up * verticalOffset;

        if (orientToMovement && fwd.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }

    private float CreateStableOffset(int seed)
    {
        float half = GetAllowedHalfWidth();
        if (half <= Mathf.Epsilon) return 0f;
        float n = Mathf.Repeat(seed * 0.61803398875f, 1f);
        return Mathf.Lerp(-half, half, n);
    }

    private float GetAllowedHalfWidth()
        => path != null ? Mathf.Max(0f, path.UsableHalfWidth * roadWidthUsage) : 0f;
}
