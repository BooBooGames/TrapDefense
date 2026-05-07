using System;
using UnityEngine;

public class ZombieRuntime : MonoBehaviour
{
    [SerializeField][Min(1f)] private float maxHealth = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private ZombiePathFollower pathFollower;
    [SerializeField] private string deathAnimationStateName = "Death";
    [SerializeField][Min(0f)] private float destroyDelayAfterDeath = 2f;

    private bool configured;
    private bool despawnNotified;
    private bool killNotified;
    private bool escapedNotified;
    private float currentHealth;
    private int coinReward;
    private int gemReward;
    private ParticleSystem killEffectPrefab;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public int CoinReward => coinReward;
    public int GemReward => gemReward;
    public ParticleSystem KillEffectPrefab => killEffectPrefab;

    public event Action<ZombieRuntime> Despawned;
    public event Action<ZombieRuntime, Vector3> Killed;
    public event Action<ZombieRuntime> Escaped;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (pathFollower == null)
        {
            pathFollower = GetComponent<ZombiePathFollower>();
        }

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

    public void ConfigureKillEffect(ParticleSystem configuredKillEffectPrefab)
    {
        killEffectPrefab = configuredKillEffectPrefab;
    }

    public void ApplyDamage(float damage)
    {
        if (killNotified || damage <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Kill()
    {
        if (killNotified)
        {
            return;
        }

        currentHealth = 0f;
        Die();
    }

    private void Die()
    {
        NotifyKilled();
        pathFollower?.StopMovement();

        if (animator != null && !string.IsNullOrWhiteSpace(deathAnimationStateName))
        {
            animator.Play(deathAnimationStateName, 0, 0f);
        }

        Destroy(gameObject, destroyDelayAfterDeath);
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
        Vector3 deathPosition = transform.position;
        Killed?.Invoke(this, deathPosition);
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
