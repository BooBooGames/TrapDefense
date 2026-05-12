using System.Collections.Generic;
using UnityEngine;

public class EndPointTrigger : MonoBehaviour
{
    private static readonly List<EndPointTrigger> ActiveTriggers = new List<EndPointTrigger>();
    private static bool baseInvulnerable;

    [SerializeField] private Collider triggerCollider;
    [SerializeField] private GameViewScreen gameViewScreen;
    [SerializeField] private ParticleSystem gateBreakEffectPrefab, baseZoneWallEffectPrefab;
    [SerializeField] private List<GameObject> gateVisuals = new List<GameObject>();
    [SerializeField] private List<Collider> gateVisualColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> gateVisualRigidbodies = new List<Rigidbody>();
    [SerializeField][Min(1)] private int damagePerZombie = 1;
    [SerializeField] private bool disableTriggerAfterGateBreak;

    private readonly HashSet<ZombieRuntime> damagedZombies = new HashSet<ZombieRuntime>();
    private readonly List<Vector3> initialGatePositions = new List<Vector3>();
    private readonly List<Quaternion> initialGateRotations = new List<Quaternion>();
    private ParticleSystem baseZoneWallEffectInstance;
    private bool gateBroken;

    private void Awake()
    {
        CacheGatePhysicsReferences();
        CacheInitialGateTransforms();
        SetGatePhysicsEnabled(false);
    }

    private void OnEnable()
    {
        if (!ActiveTriggers.Contains(this))
        {
            ActiveTriggers.Add(this);
        }

        if (baseInvulnerable)
        {
            SetBaseZoneWallEffectActive(true);
        }
    }

    private void OnDisable()
    {
        SetBaseZoneWallEffectActive(false);
        ActiveTriggers.Remove(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ZombieRuntime zombieRuntime))
        {
            return;
        }

        if (baseInvulnerable)
        {
            return;
        }

        if (!damagedZombies.Add(zombieRuntime))
        {
            return;
        }

        ResolveGameViewScreen()?.DamagePlayer(damagePerZombie);

        if (!gateBroken)
        {
            BreakGate();
        }
    }

    private void BreakGate()
    {
        gateBroken = true;

        if (gateBreakEffectPrefab != null)
        {
            ParticleSystem effectInstance = Instantiate(gateBreakEffectPrefab, transform.position, Quaternion.identity);
            effectInstance.Play();
        }

        SetGatePhysicsEnabled(true);

        if (disableTriggerAfterGateBreak && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private GameViewScreen ResolveGameViewScreen()
    {
        if (gameViewScreen == null)
        {
            gameViewScreen = GameViewScreen.Instance;
        }

        return gameViewScreen;
    }

    private void CacheGatePhysicsReferences()
    {
        if (gateVisuals == null)
        {
            return;
        }

        for (int i = 0; i < gateVisuals.Count; i++)
        {
            GameObject visual = gateVisuals[i];
            if (visual == null)
            {
                continue;
            }

            if (i >= gateVisualColliders.Count || gateVisualColliders[i] == null)
            {
                if (visual.TryGetComponent(out Collider gateCollider))
                {
                    AddOrSet(gateVisualColliders, i, gateCollider);
                }
            }

            if (i >= gateVisualRigidbodies.Count || gateVisualRigidbodies[i] == null)
            {
                if (visual.TryGetComponent(out Rigidbody gateRigidbody))
                {
                    AddOrSet(gateVisualRigidbodies, i, gateRigidbody);
                }
            }
        }
    }

    private void SetGatePhysicsEnabled(bool isEnabled)
    {
        for (int i = 0; i < gateVisualColliders.Count; i++)
        {
            if (gateVisualColliders[i] != null)
            {
                gateVisualColliders[i].enabled = isEnabled;
            }
        }

        for (int i = 0; i < gateVisualRigidbodies.Count; i++)
        {
            Rigidbody gateRigidbody = gateVisualRigidbodies[i];
            if (gateRigidbody == null)
            {
                continue;
            }

            gateRigidbody.isKinematic = !isEnabled;
            gateRigidbody.useGravity = isEnabled;
            if (!isEnabled)
            {
                gateRigidbody.linearVelocity = Vector3.zero;
                gateRigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                gateRigidbody.WakeUp();
            }
        }
    }

    private static void AddOrSet<T>(List<T> list, int index, T value)
    {
        while (list.Count <= index)
        {
            list.Add(default);
        }

        list[index] = value;
    }

    public void ResetGate()
    {
        gateBroken = false;
        damagedZombies.Clear();
        RestoreInitialGateTransforms();
        SetGatePhysicsEnabled(false);

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }

    public static void ResetAllGates()
    {
        for (int i = 0; i < ActiveTriggers.Count; i++)
        {
            ActiveTriggers[i]?.ResetGate();
        }
    }

    public static void SetBaseInvulnerable(bool isInvulnerable)
    {
        baseInvulnerable = isInvulnerable;

        for (int i = 0; i < ActiveTriggers.Count; i++)
        {
            ActiveTriggers[i]?.SetBaseZoneWallEffectActive(isInvulnerable);
        }
    }

    private void SetBaseZoneWallEffectActive(bool isActive)
    {
        if (baseZoneWallEffectPrefab == null)
        {
            return;
        }

        if (isActive)
        {
            if (baseZoneWallEffectInstance == null)
            {
                baseZoneWallEffectInstance = Instantiate(baseZoneWallEffectPrefab, transform.position, Quaternion.identity, transform);
            }

            baseZoneWallEffectInstance.gameObject.SetActive(true);
            baseZoneWallEffectInstance.Play();
            return;
        }

        if (baseZoneWallEffectInstance != null)
        {
            baseZoneWallEffectInstance.Stop();
            baseZoneWallEffectInstance.gameObject.SetActive(false);
        }
    }

    private void CacheInitialGateTransforms()
    {
        initialGatePositions.Clear();
        initialGateRotations.Clear();

        if (gateVisuals == null)
        {
            return;
        }

        for (int i = 0; i < gateVisuals.Count; i++)
        {
            Transform visualTransform = gateVisuals[i] != null ? gateVisuals[i].transform : null;
            initialGatePositions.Add(visualTransform != null ? visualTransform.localPosition : Vector3.zero);
            initialGateRotations.Add(visualTransform != null ? visualTransform.localRotation : Quaternion.identity);
        }
    }

    private void RestoreInitialGateTransforms()
    {
        if (gateVisuals == null)
        {
            return;
        }

        for (int i = 0; i < gateVisuals.Count; i++)
        {
            Transform visualTransform = gateVisuals[i] != null ? gateVisuals[i].transform : null;
            if (visualTransform == null || i >= initialGatePositions.Count || i >= initialGateRotations.Count)
            {
                continue;
            }

            visualTransform.localPosition = initialGatePositions[i];
            visualTransform.localRotation = initialGateRotations[i];
        }
    }
}
