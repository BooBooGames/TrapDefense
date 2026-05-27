using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleWeaponDamage : MonoBehaviour
{
    [SerializeField] private ParticleSystem weaponParticles;
    [SerializeField][Min(0.01f)] private float particleHitRadius = 0.35f;
    [SerializeField] private LayerMask zombieLayerMask = ~0;

    private readonly List<ParticleSystem.Particle> triggerParticles = new List<ParticleSystem.Particle>();
    private readonly Collider[] overlapResults = new Collider[16];
    private readonly HashSet<ZombieRuntime> damagedZombiesThisFrame = new HashSet<ZombieRuntime>();
    private WeaponUpgradeController weaponUpgradeController;

    private void Awake()
    {
        weaponParticles = weaponParticles != null ? weaponParticles : GetComponent<ParticleSystem>();
        weaponUpgradeController = GetComponentInParent<WeaponUpgradeController>();
    }

    private void OnParticleTrigger()
    {
        damagedZombiesThisFrame.Clear();
        DamageZombiesInsideTriggeredParticles(ParticleSystemTriggerEventType.Enter);
        DamageZombiesInsideTriggeredParticles(ParticleSystemTriggerEventType.Inside);
    }

    private void DamageZombiesInsideTriggeredParticles(ParticleSystemTriggerEventType triggerEventType)
    {
        int particleCount = weaponParticles.GetTriggerParticles(triggerEventType, triggerParticles);

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 particlePosition = GetParticleWorldPosition(triggerParticles[i].position);
            int overlapCount = Physics.OverlapSphereNonAlloc(
                particlePosition,
                particleHitRadius,
                overlapResults,
                zombieLayerMask,
                QueryTriggerInteraction.Collide);

            for (int overlapIndex = 0; overlapIndex < overlapCount; overlapIndex++)
            {
                ZombieRuntime zombieRuntime = overlapResults[overlapIndex].GetComponentInParent<ZombieRuntime>();
                if (zombieRuntime == null || !damagedZombiesThisFrame.Add(zombieRuntime))
                {
                    continue;
                }

                zombieRuntime.ApplyDamage(weaponUpgradeController.DamagePower);
            }
        }
    }

    private Vector3 GetParticleWorldPosition(Vector3 particlePosition)
    {
        ParticleSystem.MainModule mainModule = weaponParticles.main;
        if (mainModule.simulationSpace == ParticleSystemSimulationSpace.World)
        {
            return particlePosition;
        }

        if (mainModule.simulationSpace == ParticleSystemSimulationSpace.Custom)
        {
            return mainModule.customSimulationSpace.TransformPoint(particlePosition);
        }

        return weaponParticles.transform.TransformPoint(particlePosition);
    }
}
