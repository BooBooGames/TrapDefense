using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotator : MonoBehaviour
{
    private static readonly List<WeaponRotator> ActiveRotators = new List<WeaponRotator>();
    private static bool gameplayMotionEnabled;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime;

    [Header("Rotation")]
    [SerializeField] private bool rotationEnabled = true;
    [SerializeField] private WeaponRotationMode rotationMode = WeaponRotationMode.Continuous360;
    [SerializeField] private WeaponMotionLoopType rotationLoop = WeaponMotionLoopType.Restart;
    [SerializeField] private Space rotationSpace = Space.Self;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private WeaponMotionAxis rotationAxes = WeaponMotionAxis.Y;
    [SerializeField][Min(0f)] private float rotationSpeed = 90f;
    [SerializeField] private AnimationCurve rotationEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private Vector3 targetEulerAngles;
    [SerializeField] private bool targetRotationIsRelative = true;

    [Header("Position")]
    [SerializeField] private bool movementEnabled;
    [SerializeField] private WeaponMovementMode movementMode = WeaponMovementMode.ToOffset;
    [SerializeField] private WeaponMotionLoopType movementLoop = WeaponMotionLoopType.PingPong;
    [SerializeField] private Space movementSpace = Space.Self;
    [SerializeField] private WeaponMotionAxis movementAxes = WeaponMotionAxis.Y;
    [SerializeField] private Vector3 movementOffset = Vector3.up;
    [SerializeField][Min(0f)] private float movementSpeed = 1f;
    [SerializeField] private AnimationCurve movementEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private Quaternion initialLocalRotation;
    private Quaternion initialWorldRotation;
    private Vector3 initialLocalPosition;
    private Vector3 initialWorldPosition;
    private float rotationProgress;
    private float movementProgress;
    private int rotationDirection = 1;
    private int movementDirection = 1;
    private bool initialTransformCaptured;

    public void SetRotationSpeed(float newRotationSpeed)
    {
        rotationSpeed = Mathf.Max(0f, newRotationSpeed);
    }

    public void SetMovementSpeed(float newMovementSpeed)
    {
        movementSpeed = Mathf.Max(0f, newMovementSpeed);
    }

    public static void SetGameplayMotionEnabled(bool isEnabled)
    {
        if (gameplayMotionEnabled == isEnabled)
        {
            return;
        }

        gameplayMotionEnabled = isEnabled;

        if (!isEnabled)
        {
            return;
        }

        for (int i = 0; i < ActiveRotators.Count; i++)
        {
            ActiveRotators[i]?.RestartMotion(true);
        }
    }

    public void RestartMotion()
    {
        RestartMotion(false);
    }

    public void RestartMotion(bool resetToInitialTransform)
    {
        if (!initialTransformCaptured)
        {
            CaptureInitialTransform();
        }

        if (resetToInitialTransform)
        {
            ApplyInitialTransform();
        }

        rotationProgress = 0f;
        movementProgress = 0f;
        rotationDirection = 1;
        movementDirection = 1;
    }

    private void Awake()
    {
        RestartMotion();
    }

    private void OnEnable()
    {
        if (!ActiveRotators.Contains(this))
        {
            ActiveRotators.Add(this);
        }

        RestartMotion();
    }

    private void OnDisable()
    {
        ActiveRotators.Remove(this);
    }

    private void Update()
    {
        if (!gameplayMotionEnabled)
        {
            return;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (rotationEnabled)
        {
            UpdateRotation(deltaTime);
        }

        if (movementEnabled)
        {
            UpdateMovement(deltaTime);
        }
    }

    private void UpdateRotation(float deltaTime)
    {
        switch (rotationMode)
        {
            case WeaponRotationMode.Continuous360:
                RotateContinuously(deltaTime);
                break;
            case WeaponRotationMode.ToTarget:
                RotateToTarget(deltaTime);
                break;
        }
    }

    private void RotateContinuously(float deltaTime)
    {
        Vector3 resolvedAxis = ResolveAxis(rotationAxis, rotationAxes);
        if (resolvedAxis == Vector3.zero || rotationSpeed <= 0f)
        {
            return;
        }

        transform.Rotate(resolvedAxis, rotationSpeed * deltaTime, rotationSpace);
    }

    private void RotateToTarget(float deltaTime)
    {
        Quaternion startRotation = rotationSpace == Space.Self ? initialLocalRotation : initialWorldRotation;
        Quaternion targetRotation = GetTargetRotation(startRotation);
        float progressStep = GetRotationProgressStep(startRotation, targetRotation, deltaTime);
        float progress = AdvanceProgress(ref rotationProgress, ref rotationDirection, rotationLoop, progressStep);
        progress = EvaluateCurve(rotationEase, progress);
        Quaternion currentRotation = Quaternion.LerpUnclamped(startRotation, targetRotation, progress);

        if (rotationSpace == Space.Self)
        {
            transform.localRotation = currentRotation;
        }
        else
        {
            transform.rotation = currentRotation;
        }
    }

    private Quaternion GetTargetRotation(Quaternion startRotation)
    {
        Vector3 filteredTarget = FilterVector(targetEulerAngles, rotationAxes);
        Quaternion targetDelta = Quaternion.Euler(filteredTarget);
        return targetRotationIsRelative ? startRotation * targetDelta : targetDelta;
    }

    private float GetRotationProgressStep(Quaternion startRotation, Quaternion targetRotation, float deltaTime)
    {
        if (rotationSpeed <= 0f)
        {
            return 0f;
        }

        float targetAngle = Quaternion.Angle(startRotation, targetRotation);
        return targetAngle > 0.001f ? rotationSpeed * deltaTime / targetAngle : 1f;
    }

    private void UpdateMovement(float deltaTime)
    {
        Vector3 offset = FilterVector(movementOffset, movementAxes);
        if (offset == Vector3.zero)
        {
            return;
        }

        switch (movementMode)
        {
            case WeaponMovementMode.ToOffset:
                MoveToOffset(offset, deltaTime);
                break;
            case WeaponMovementMode.Continuous:
                MoveContinuously(offset.normalized, deltaTime);
                break;
        }
    }

    private void MoveToOffset(Vector3 offset, float deltaTime)
    {
        Vector3 startPosition = movementSpace == Space.Self ? initialLocalPosition : initialWorldPosition;
        Vector3 targetPosition = startPosition + ResolveMovementOffset(offset);
        float progressStep = GetMovementProgressStep(startPosition, targetPosition, deltaTime);
        float progress = AdvanceProgress(ref movementProgress, ref movementDirection, movementLoop, progressStep);
        progress = EvaluateCurve(movementEase, progress);
        Vector3 currentPosition = Vector3.LerpUnclamped(startPosition, targetPosition, progress);

        if (movementSpace == Space.Self)
        {
            transform.localPosition = currentPosition;
        }
        else
        {
            transform.position = currentPosition;
        }
    }

    private void MoveContinuously(Vector3 direction, float deltaTime)
    {
        if (movementSpeed <= 0f)
        {
            return;
        }

        Vector3 movement = ResolveMovementOffset(direction) * (movementSpeed * deltaTime);
        if (movementSpace == Space.Self)
        {
            transform.Translate(movement, Space.Self);
        }
        else
        {
            transform.position += movement;
        }
    }

    private Vector3 ResolveMovementOffset(Vector3 offset)
    {
        return offset;
    }

    private float GetMovementProgressStep(Vector3 startPosition, Vector3 targetPosition, float deltaTime)
    {
        if (movementSpeed <= 0f)
        {
            return 0f;
        }

        float targetDistance = Vector3.Distance(startPosition, targetPosition);
        return targetDistance > 0.001f ? movementSpeed * deltaTime / targetDistance : 1f;
    }

    private static float AdvanceProgress(
        ref float progress,
        ref int direction,
        WeaponMotionLoopType loopType,
        float progressStep)
    {
        progressStep = Mathf.Max(0f, progressStep);

        if (loopType == WeaponMotionLoopType.None)
        {
            progress = Mathf.Min(1f, progress + progressStep);
            return progress;
        }

        if (loopType == WeaponMotionLoopType.Restart)
        {
            progress = Mathf.Repeat(progress + progressStep, 1f);
            return progress;
        }

        progress += progressStep * direction;
        while (progress > 1f || progress < 0f)
        {
            if (progress > 1f)
            {
                progress = 2f - progress;
                direction = -1;
            }
            else if (progress < 0f)
            {
                progress = -progress;
                direction = 1;
            }
        }

        return Mathf.Clamp01(progress);
    }

    private static Vector3 ResolveAxis(Vector3 configuredAxis, WeaponMotionAxis axisMask)
    {
        Vector3 axis = configuredAxis == Vector3.zero ? Vector3.one : configuredAxis.normalized;
        return FilterVector(axis, axisMask).normalized;
    }

    private static Vector3 FilterVector(Vector3 value, WeaponMotionAxis axisMask)
    {
        return new Vector3(
            axisMask.HasFlag(WeaponMotionAxis.X) ? value.x : 0f,
            axisMask.HasFlag(WeaponMotionAxis.Y) ? value.y : 0f,
            axisMask.HasFlag(WeaponMotionAxis.Z) ? value.z : 0f);
    }

    private static float EvaluateCurve(AnimationCurve curve, float progress)
    {
        return curve != null ? curve.Evaluate(Mathf.Clamp01(progress)) : Mathf.Clamp01(progress);
    }

    private void CaptureInitialTransform()
    {
        initialLocalRotation = transform.localRotation;
        initialWorldRotation = transform.rotation;
        initialLocalPosition = transform.localPosition;
        initialWorldPosition = transform.position;
        initialTransformCaptured = true;
    }

    private void ApplyInitialTransform()
    {
        if (rotationSpace == Space.Self)
        {
            transform.localRotation = initialLocalRotation;
        }
        else
        {
            transform.rotation = initialWorldRotation;
        }

        if (movementSpace == Space.Self)
        {
            transform.localPosition = initialLocalPosition;
        }
        else
        {
            transform.position = initialWorldPosition;
        }
    }
}

public enum WeaponRotationMode
{
    Continuous360 = 0,
    ToTarget = 1
}

public enum WeaponMovementMode
{
    ToOffset = 0,
    Continuous = 1
}

public enum WeaponMotionLoopType
{
    None = 0,
    Restart = 1,
    PingPong = 2
}

[Flags]
public enum WeaponMotionAxis
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4
}
