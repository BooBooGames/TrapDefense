using UnityEngine;

public class ZombieWeaponCollision : MonoBehaviour
{
    [SerializeField] private string weaponTag = "Weapon";

    private ZombieRuntime zombieRuntime;

    private void Awake()
    {
        zombieRuntime = GetComponent<ZombieRuntime>();
        if (zombieRuntime == null)
        {
            zombieRuntime = GetComponentInParent<ZombieRuntime>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsWeaponTrigger(other))
        {
            zombieRuntime?.Kill();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsWeaponObject(collision.collider))
        {
            zombieRuntime?.Kill();
        }
    }

    private bool IsWeaponTrigger(Collider other)
    {
        return IsWeaponObject(other);
    }

    private bool IsWeaponObject(Component other)
    {
        return other != null && other.CompareTag(weaponTag);
    }
}
