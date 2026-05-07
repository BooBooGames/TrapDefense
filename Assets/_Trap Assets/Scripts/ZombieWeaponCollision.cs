using UnityEngine;

public class ZombieWeaponCollision : MonoBehaviour
{
    private WeaponUpgradeController weaponUpgradeController;

    private void Awake()
    {
        weaponUpgradeController = GetComponentInParent<WeaponUpgradeController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ZombieRuntime zombieRuntime))
        {
            zombieRuntime.ApplyDamage(weaponUpgradeController.DamagePower);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent(out ZombieRuntime zombieRuntime))
        {
            zombieRuntime.ApplyDamage(weaponUpgradeController.DamagePower);
        }
    }
}
