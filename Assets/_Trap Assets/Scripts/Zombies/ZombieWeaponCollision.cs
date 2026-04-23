using UnityEngine;

public class ZombieWeaponCollision : MonoBehaviour
{
    [SerializeField] private string weaponTag = "Weapon";
    [SerializeField][Min(1f)] private float fallbackWeaponDamage = 1f;

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
        if (TryGetWeaponDamage(other, out float weaponDamage))
        {
            zombieRuntime?.ApplyDamage(weaponDamage);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (TryGetWeaponDamage(collision.collider, out float weaponDamage))
        {
            zombieRuntime?.ApplyDamage(weaponDamage);
        }
    }

    private bool TryGetWeaponDamage(Collider other, out float weaponDamage)
    {
        weaponDamage = 0f;
        if (!IsWeaponObject(other))
        {
            return false;
        }

        WeaponDamageSource damageSource = other.GetComponent<WeaponDamageSource>();
        if (damageSource == null)
        {
            damageSource = other.GetComponentInParent<WeaponDamageSource>();
        }

        weaponDamage = damageSource != null ? damageSource.DamagePower : fallbackWeaponDamage;
        return true;
    }

    private bool IsWeaponObject(Component other)
    {
        return other != null && other.CompareTag(weaponTag);
    }
}
