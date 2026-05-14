using System;
using UnityEngine;

public class ZombieRuntime : MonoBehaviour
{
    [SerializeField][Min(1f)] private float maxHealth = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private ZombiePathFollower pathFollower;

    [Header("Animation State Names")]
    [SerializeField] private string runAnimStateName = "Run";
    [SerializeField] private string deathAnimStateName = "Death";
    [SerializeField] private string burnAnimStateName = "Burn";

    [SerializeField][Min(0f)] private float destroyDelayAfterDeath = 2f;

    private float currentHealth;
    private int coinReward;
    private int gemReward;
    private bool isFrozen;
    private bool killNotified;
    private bool escapedNotified;
    private bool despawnNotified;
    private bool configured;

    private ParticleSystem killEffectPrefab;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public int CoinReward => coinReward;
    public int GemReward => gemReward;
    public bool IsFrozen => isFrozen;
    public ParticleSystem KillEffectPrefab => killEffectPrefab;

    public event Action<ZombieRuntime> Despawned;
    public event Action<ZombieRuntime, Vector3> Killed;
    public event Action<ZombieRuntime> Escaped;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (pathFollower == null) pathFollower = GetComponent<ZombiePathFollower>();
        currentHealth = maxHealth;
    }

    // ── Configuration ─────────────────────────────────────────────────────────

    public void Configure(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        configured = true;
    }

    public void ConfigureRewards(int coins, int gems)
    {
        coinReward = Mathf.Max(0, coins);
        gemReward = Mathf.Max(0, gems);
    }

    public void ConfigureKillEffect(ParticleSystem prefab) => killEffectPrefab = prefab;

    // ── Damage ────────────────────────────────────────────────────────────────

    public void ApplyDamage(float damage)
    {
        if (killNotified || damage <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth <= 0f) Die();
    }

    public void Kill()
    {
        if (killNotified) return;
        currentHealth = 0f;
        Die();
    }

    // ── Freeze / Thaw ─────────────────────────────────────────────────────────

    /// <summary>Stops movement and hard-pauses the animator (zombie frozen mid-frame).</summary>
    public void Freeze()
    {
        if (killNotified || isFrozen) return;
        isFrozen = true;
        pathFollower?.PauseMovement();
        if (animator != null) animator.speed = 0f;
    }

    /// <summary>Resumes movement and restores normal animation.</summary>
    public void Thaw()
    {
        if (!isFrozen) return;
        isFrozen = false;
        pathFollower?.ResumeMovement();
        if (animator != null) animator.speed = 1f;
        PlayAnim(runAnimStateName);
    }

    // ── Burn ──────────────────────────────────────────────────────────────────

    public void PlayBurnAnimation()
    {
        if (!killNotified) PlayAnim(burnAnimStateName);
    }

    public void StopBurnAnimation()
    {
        if (!killNotified) PlayAnim(runAnimStateName);
    }

    // ── Escape ────────────────────────────────────────────────────────────────

    public void MarkEscaped()
    {
        if (escapedNotified) return;
        escapedNotified = true;
        Escaped?.Invoke(this);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void Die()
    {
        killNotified = true;
        Killed?.Invoke(this, transform.position);
        pathFollower?.StopMovement();
        // Make sure animator is running so death animation plays even if frozen
        if (animator != null) animator.speed = 1f;
        isFrozen = false;
        PlayAnim(deathAnimStateName);
        Destroy(gameObject, destroyDelayAfterDeath);
    }

    private void PlayAnim(string stateName)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(stateName))
            animator.Play(stateName, 0, 0f);
    }

    private void OnDestroy()
    {
        if (despawnNotified) return;
        if (!configured) currentHealth = maxHealth;
        despawnNotified = true;
        Despawned?.Invoke(this);
    }
}