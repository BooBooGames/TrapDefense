using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ZombiePath : MonoBehaviour
{
    [SerializeField] private bool useChildrenAsWaypoints = true;
    [SerializeField] private Transform[] waypoints;
    [SerializeField][Min(0.5f)] private float roadWidth = 3f;
    [SerializeField][Min(0f)] private float boundaryPadding = 0.25f;
    [SerializeField] private bool smoothPath = true;
    [SerializeField][Min(4)] private int samplesPerSegment = 24;
    [SerializeField] private Color pathColor = Color.red;

    private readonly List<Vector3> sampledPoints = new List<Vector3>();
    private readonly List<Vector3> sampledForwards = new List<Vector3>();
    private readonly List<float> cumulativeLengths = new List<float>();
    private float totalLength;

    public float Length
    {
        get
        {
            RebuildIfNeeded();
            return totalLength;
        }
    }

    public bool HasValidPath
    {
        get
        {
            RebuildIfNeeded();
            return sampledPoints.Count > 1;
        }
    }

    public float RoadWidth => roadWidth;

    public float UsableHalfWidth => Mathf.Max(0f, (roadWidth * 0.5f) - boundaryPadding);

    public void SetRoadWidth(float newRoadWidth)
    {
        roadWidth = Mathf.Max(0.5f, newRoadWidth);
    }

    private void Awake()
    {
        RebuildCache();
    }

    private void OnEnable()
    {
        RebuildCache();
    }

    private void OnValidate()
    {
        roadWidth = Mathf.Max(0.5f, roadWidth);
        boundaryPadding = Mathf.Max(0f, boundaryPadding);
        samplesPerSegment = Mathf.Max(4, samplesPerSegment);
        RebuildCache();
    }

    public void RebuildCache()
    {
        sampledPoints.Clear();
        sampledForwards.Clear();
        cumulativeLengths.Clear();
        totalLength = 0f;

        List<Vector3> sourcePoints = GetSourcePoints();
        if (sourcePoints.Count == 0)
        {
            return;
        }

        if (sourcePoints.Count == 1)
        {
            sampledPoints.Add(sourcePoints[0]);
            sampledForwards.Add(transform.forward);
            cumulativeLengths.Add(0f);
            return;
        }

        sampledPoints.Add(sourcePoints[0]);
        cumulativeLengths.Add(0f);

        for (int segmentIndex = 0; segmentIndex < sourcePoints.Count - 1; segmentIndex++)
        {
            for (int sampleIndex = 1; sampleIndex <= samplesPerSegment; sampleIndex++)
            {
                float t = sampleIndex / (float)samplesPerSegment;
                Vector3 point = smoothPath
                    ? GetCatmullRomPoint(sourcePoints, segmentIndex, t)
                    : Vector3.Lerp(sourcePoints[segmentIndex], sourcePoints[segmentIndex + 1], t);

                float distance = Vector3.Distance(sampledPoints[sampledPoints.Count - 1], point);
                totalLength += distance;
                sampledPoints.Add(point);
                cumulativeLengths.Add(totalLength);
            }
        }

        RebuildForwardCache();
    }

    public void Evaluate(float normalizedDistance, out Vector3 position, out Vector3 forward)
    {
        RebuildIfNeeded();

        if (sampledPoints.Count == 0)
        {
            position = transform.position;
            forward = transform.forward;
            return;
        }

        if (sampledPoints.Count == 1 || totalLength <= Mathf.Epsilon)
        {
            position = sampledPoints[0];
            forward = sampledForwards.Count > 0 ? sampledForwards[0] : transform.forward;
            return;
        }

        float clampedDistance = Mathf.Clamp01(normalizedDistance) * totalLength;
        EvaluateAtDistance(clampedDistance, out position, out forward);
    }

    public void EvaluateAtDistance(float distance, out Vector3 position, out Vector3 forward)
    {
        RebuildIfNeeded();

        if (sampledPoints.Count == 0)
        {
            position = transform.position;
            forward = transform.forward;
            return;
        }

        if (sampledPoints.Count == 1 || totalLength <= Mathf.Epsilon)
        {
            position = sampledPoints[0];
            forward = sampledForwards.Count > 0 ? sampledForwards[0] : transform.forward;
            return;
        }

        float clampedDistance = Mathf.Clamp(distance, 0f, totalLength);
        int segmentIndex = FindSegmentIndex(clampedDistance);

        float previousDistance = cumulativeLengths[segmentIndex];
        float nextDistance = cumulativeLengths[segmentIndex + 1];
        float segmentLength = Mathf.Max(nextDistance - previousDistance, Mathf.Epsilon);
        float segmentT = (clampedDistance - previousDistance) / segmentLength;

        Vector3 from = sampledPoints[segmentIndex];
        Vector3 to = sampledPoints[segmentIndex + 1];
        position = Vector3.Lerp(from, to, segmentT);

        Vector3 fromForward = sampledForwards[segmentIndex];
        Vector3 toForward = sampledForwards[Mathf.Min(segmentIndex + 1, sampledForwards.Count - 1)];
        forward = Vector3.Slerp(fromForward, toForward, segmentT).normalized;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            Vector3 fallback = to - from;
            forward = fallback.sqrMagnitude > 0.0001f ? fallback.normalized : transform.forward;
        }
    }

    public float FindClosestDistance(Vector3 worldPosition, out Vector3 closestPoint, out Vector3 forward)
    {
        RebuildIfNeeded();

        if (sampledPoints.Count == 0)
        {
            closestPoint = transform.position;
            forward = transform.forward;
            return 0f;
        }

        if (sampledPoints.Count == 1)
        {
            closestPoint = sampledPoints[0];
            forward = sampledForwards.Count > 0 ? sampledForwards[0] : transform.forward;
            return 0f;
        }

        float bestDistance = 0f;
        float bestDistanceSquared = float.MaxValue;
        closestPoint = sampledPoints[0];
        forward = sampledForwards[0];

        for (int i = 0; i < sampledPoints.Count - 1; i++)
        {
            Vector3 from = sampledPoints[i];
            Vector3 to = sampledPoints[i + 1];
            Vector3 segment = to - from;
            float segmentLengthSquared = segment.sqrMagnitude;
            if (segmentLengthSquared <= Mathf.Epsilon)
            {
                continue;
            }

            float t = Mathf.Clamp01(Vector3.Dot(worldPosition - from, segment) / segmentLengthSquared);
            Vector3 projectedPoint = from + (segment * t);
            float projectedDistanceSquared = (worldPosition - projectedPoint).sqrMagnitude;

            if (projectedDistanceSquared >= bestDistanceSquared)
            {
                continue;
            }

            bestDistanceSquared = projectedDistanceSquared;
            float segmentStartDistance = cumulativeLengths[i];
            float segmentLength = Mathf.Sqrt(segmentLengthSquared);
            bestDistance = segmentStartDistance + (segmentLength * t);
            closestPoint = projectedPoint;

            Vector3 fromForward = sampledForwards[i];
            Vector3 toForward = sampledForwards[Mathf.Min(i + 1, sampledForwards.Count - 1)];
            forward = Vector3.Slerp(fromForward, toForward, t).normalized;
        }

        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = transform.forward;
        }

        return bestDistance;
    }

    private void OnDrawGizmos()
    {
        RebuildIfNeeded();
        if (sampledPoints.Count < 2)
        {
            return;
        }

        Gizmos.color = pathColor;
        for (int i = 0; i < sampledPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(sampledPoints[i], sampledPoints[i + 1]);

            Vector3 segmentForward = (sampledPoints[i + 1] - sampledPoints[i]).normalized;
            if (segmentForward.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            Vector3 right = Vector3.Cross(Vector3.up, segmentForward).normalized;
            Vector3 leftEdgeStart = sampledPoints[i] - right * UsableHalfWidth;
            Vector3 leftEdgeEnd = sampledPoints[i + 1] - right * UsableHalfWidth;
            Vector3 rightEdgeStart = sampledPoints[i] + right * UsableHalfWidth;
            Vector3 rightEdgeEnd = sampledPoints[i + 1] + right * UsableHalfWidth;

            Color edgeColor = new Color(pathColor.r, pathColor.g, pathColor.b, 0.35f);
            Gizmos.color = edgeColor;
            Gizmos.DrawLine(leftEdgeStart, leftEdgeEnd);
            Gizmos.DrawLine(rightEdgeStart, rightEdgeEnd);
            Gizmos.color = pathColor;
        }

        Gizmos.color = Color.white;
        foreach (Vector3 point in GetSourcePoints())
        {
            Gizmos.DrawSphere(point, 0.18f);
        }
    }

    private List<Vector3> GetSourcePoints()
    {
        List<Vector3> points = new List<Vector3>();

        if (useChildrenAsWaypoints)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                points.Add(transform.GetChild(i).position);
            }
        }
        else if (waypoints != null)
        {
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    points.Add(waypoint.position);
                }
            }
        }

        return points;
    }

    private Vector3 GetCatmullRomPoint(IReadOnlyList<Vector3> points, int segmentIndex, float t)
    {
        Vector3 p0 = points[Mathf.Max(segmentIndex - 1, 0)];
        Vector3 p1 = points[segmentIndex];
        Vector3 p2 = points[Mathf.Min(segmentIndex + 1, points.Count - 1)];
        Vector3 p3 = points[Mathf.Min(segmentIndex + 2, points.Count - 1)];

        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private int FindSegmentIndex(float targetDistance)
    {
        for (int i = 0; i < cumulativeLengths.Count - 1; i++)
        {
            if (targetDistance <= cumulativeLengths[i + 1])
            {
                return i;
            }
        }

        return Mathf.Max(0, cumulativeLengths.Count - 2);
    }

    private void RebuildForwardCache()
    {
        sampledForwards.Clear();

        if (sampledPoints.Count == 0)
        {
            return;
        }

        if (sampledPoints.Count == 1)
        {
            sampledForwards.Add(transform.forward);
            return;
        }

        for (int i = 0; i < sampledPoints.Count; i++)
        {
            Vector3 tangent;

            if (i == 0)
            {
                tangent = sampledPoints[1] - sampledPoints[0];
            }
            else if (i == sampledPoints.Count - 1)
            {
                tangent = sampledPoints[i] - sampledPoints[i - 1];
            }
            else
            {
                tangent = sampledPoints[i + 1] - sampledPoints[i - 1];
            }

            sampledForwards.Add(tangent.sqrMagnitude > 0.0001f ? tangent.normalized : transform.forward);
        }
    }

    private void RebuildIfNeeded()
    {
        if (sampledPoints.Count == 0 && (useChildrenAsWaypoints ? transform.childCount > 0 : waypoints != null))
        {
            RebuildCache();
        }
    }
}
