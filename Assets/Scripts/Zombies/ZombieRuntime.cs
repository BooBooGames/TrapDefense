using System;
using UnityEngine;

public class ZombieRuntime : MonoBehaviour
{
    [SerializeField] [Min(1f)] private float maxHealth = 10f;

    private bool configured;
    private bool despawnNotified;
    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public event Action<ZombieRuntime> Despawned;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Configure(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        configured = true;
    }

    public void ApplyDamage(float damage)
    {
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (despawnNotified)
        {
            return;
        }

        if (!configured)
        {
            currentHealth = maxHealth;
        }

        despawnNotified = true;
        Despawned?.Invoke(this);
    }
}
