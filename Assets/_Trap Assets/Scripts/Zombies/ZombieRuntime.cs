using System;
using UnityEngine;

public class ZombieRuntime : MonoBehaviour
{
    [SerializeField] [Min(1f)] private float maxHealth = 10f;

    private bool configured;
    private bool despawnNotified;
    private bool killNotified;
    private bool escapedNotified;
    private float currentHealth;
    private int coinReward;
    private int gemReward;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public int CoinReward => coinReward;
    public int GemReward => gemReward;

    public event Action<ZombieRuntime> Despawned;
    public event Action<ZombieRuntime> Killed;
    public event Action<ZombieRuntime> Escaped;

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

    public void ConfigureRewards(int configuredCoinReward, int configuredGemReward)
    {
        coinReward = Mathf.Max(0, configuredCoinReward);
        gemReward = Mathf.Max(0, configuredGemReward);
    }

    public void ApplyDamage(float damage)
    {
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth <= 0f)
        {
            NotifyKilled();
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        currentHealth = 0f;
        NotifyKilled();
        Destroy(gameObject);
    }

    public void MarkEscaped()
    {
        if (escapedNotified)
        {
            return;
        }

        escapedNotified = true;
        Escaped?.Invoke(this);
    }

    private void NotifyKilled()
    {
        if (killNotified)
        {
            return;
        }

        killNotified = true;
        Killed?.Invoke(this);
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
