using UnityEngine;

public class WeaponDamageSource : MonoBehaviour
{
    [SerializeField] [Min(1f)] private float damagePower = 1f;

    public float DamagePower => damagePower;

    public void SetDamagePower(float newDamagePower)
    {
        damagePower = Mathf.Max(1f, newDamagePower);
    }
}
