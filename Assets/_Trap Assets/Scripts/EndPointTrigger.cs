using System.Collections.Generic;
using UnityEngine;

public class EndPointTrigger : MonoBehaviour
{
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private ParticleSystem gateBreakEffectPrefab;
    [SerializeField] private List<GameObject> gateVisuals = new List<GameObject>();

    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ZombieRuntime zombieRuntime))
        {
            if (gateBreakEffectPrefab != null)
            {
                Instantiate(gateBreakEffectPrefab, transform.position, Quaternion.identity);
            }

            foreach (var visual in gateVisuals)
            {
                if (visual != null)
                {
                    visual.SetActive(false);
                }
            }

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }
}
