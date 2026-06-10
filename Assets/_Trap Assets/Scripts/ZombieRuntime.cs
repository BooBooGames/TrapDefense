using System;
using System.Collections;
using UnityEngine;

public class ZombieRuntime : MonoBehaviour
{
    private const float WeakeningStrikeSlowMultiplier = 0.9f;
    private const float WeakeningStrikeSlowDuration = 2f;
    private const float DeathMarkInstantKillChance = 0.05f;
    private const float DoomTrapsSpeedMultiplierPerHit = 0.95f;

    [SerializeField][Min(1f)] private float maxHealth = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private ZombiePathFollower pathFollower;

    [Header("Animation State Names")]
    [SerializeField] private string runAnimStateName = "Run";
    [SerializeField] private string deathAnimStateName = "Death";
    [SerializeField] private string burnAnimStateName = "Burn";

    [SerializeField][Min(0f)] private float destroyDelayAfterDeath = 2f;
    [SerializeField][Min(0f)] private float gravityDelayAfterDeath = 1f;

    [Header("Damage Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private Color emissionFlashColor = Color.white;
    [SerializeField][Min(0f)] private float scalePunch = 0.15f;
    [SerializeField][Min(0.01f)] private float flashDuration = 0.18f;
    [SerializeField]
    private AnimationCurve flashCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.15f, 1f),
        new Keyframe(1f, 0f));

    [Header("Death Feedback")]
    [SerializeField] private ParticleSystem skillEffectPrefab;
    [SerializeField] private Color deathFlashColor = new Color(1f, 0.05f, 0.02f, 1f);
    [SerializeField] private Color deathEmissionFlashColor = new Color(1f, 0.1f, 0.04f, 1f);
    [SerializeField][Min(0.01f)] private float deathFlashDuration = 0.24f;
    [SerializeField][Min(0f)] private float deathScalePunch = 0.18f;
    [SerializeField][Min(0f)] private float deathShakeMagnitude = 0.04f;
    [SerializeField][Min(0f)] private float deathShakeFrequency = 55f;
    [SerializeField]
    private AnimationCurve deathFeedbackCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 0f));

    private Renderer[] flashRenderers;
    private Collider[] physicsColliders;
    private Rigidbody[] physicsRigidbodies;
    private Color[] originalBaseColors;
    private Color[] originalColors;
    private MaterialPropertyBlock flashBlock;
    private Coroutine flashRoutine;
    private Coroutine deathFeedbackRoutine;
    private Coroutine movementSlowRoutine;
    private Vector3 originalScale;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

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
        flashRenderers = GetComponentsInChildren<Renderer>(true);
        physicsColliders = GetComponentsInChildren<Collider>(true);
        physicsRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        flashBlock = new MaterialPropertyBlock();
        originalBaseColors = new Color[flashRenderers.Length];
        originalColors = new Color[flashRenderers.Length];
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var m = flashRenderers[i] != null ? flashRenderers[i].sharedMaterial : null;
            originalBaseColors[i] = (m != null && m.HasProperty(BaseColorId)) ? m.GetColor(BaseColorId) : Color.white;
            originalColors[i] = (m != null && m.HasProperty(ColorId)) ? m.GetColor(ColorId) : Color.white;
        }
        originalScale = transform.localScale;
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

    public Vector3 GetHeadBillboardPosition()
    {
        Vector3 zombieBounds = transform.position;

        zombieBounds.y += 2.5f; // Approximate head height above pivot; adjust as needed based on zombie model
        return zombieBounds;
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    public void ApplyDamage(float damage)
    {
        if (killNotified || damage <= 0f) return;
        if (TryApplyDeathMarkInstantKill()) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth <= 0f) Die();
        else
        {
            FlashDamage();
            ApplyDoomTrapsSlowIfActive();
            ApplyWeakeningStrikeSlowIfActive();
        }
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
        ClearMovementSlow();
        Killed?.Invoke(this, transform.position);
        pathFollower?.StopMovement();
        DisablePhysicsInteractions();
        // Make sure animator is running so death animation plays even if frozen
        if (animator != null) animator.speed = 1f;
        isFrozen = false;
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
            ClearFlashColor();
        }
        PlayDeathFeedback();
        PlayAnim(deathAnimStateName);
        StartCoroutine(EnableGravityAfterDeathDelay());
        Destroy(gameObject, destroyDelayAfterDeath);
    }

    private bool TryApplyDeathMarkInstantKill()
    {
        PlayerXpSystem playerXpSystem = PlayerXpSystem.Instance;
        if (playerXpSystem == null || !playerXpSystem.DeathMarkActive)
        {
            return false;
        }

        if (UnityEngine.Random.value >= DeathMarkInstantKillChance)
        {
            return false;
        }

        currentHealth = 0f;
        Die();
        return true;
    }

    private void ApplyDoomTrapsSlowIfActive()
    {
        PlayerXpSystem playerXpSystem = PlayerXpSystem.Instance;
        if (playerXpSystem == null || !playerXpSystem.DoomTrapsActive || pathFollower == null)
        {
            return;
        }

        pathFollower.ApplyStackingMovementSpeedMultiplier(DoomTrapsSpeedMultiplierPerHit);
    }

    private void ApplyWeakeningStrikeSlowIfActive()
    {
        PlayerXpSystem playerXpSystem = PlayerXpSystem.Instance;
        if (playerXpSystem == null || !playerXpSystem.WeakeningStrikeActive)
        {
            return;
        }

        ApplyTemporaryMovementSlow(WeakeningStrikeSlowMultiplier, WeakeningStrikeSlowDuration);
    }

    private void ApplyTemporaryMovementSlow(float speedMultiplier, float duration)
    {
        if (pathFollower == null)
        {
            return;
        }

        if (movementSlowRoutine != null)
        {
            StopCoroutine(movementSlowRoutine);
        }

        movementSlowRoutine = StartCoroutine(RunTemporaryMovementSlow(speedMultiplier, duration));
    }

    private IEnumerator RunTemporaryMovementSlow(float speedMultiplier, float duration)
    {
        pathFollower.SetTemporaryMovementSpeedMultiplier(speedMultiplier);
        yield return new WaitForSeconds(duration);
        pathFollower.SetTemporaryMovementSpeedMultiplier(1f);
        movementSlowRoutine = null;
    }

    private void ClearMovementSlow()
    {
        if (movementSlowRoutine != null)
        {
            StopCoroutine(movementSlowRoutine);
            movementSlowRoutine = null;
        }

        pathFollower?.SetTemporaryMovementSpeedMultiplier(1f);
    }

    private void DisablePhysicsInteractions()
    {
        for (int i = 0; i < physicsColliders.Length; i++)
        {
            if (physicsColliders[i] != null)
            {
                physicsColliders[i].enabled = false;
            }
        }

        for (int i = 0; i < physicsRigidbodies.Length; i++)
        {
            Rigidbody physicsRigidbody = physicsRigidbodies[i];
            if (physicsRigidbody == null)
            {
                continue;
            }

            physicsRigidbody.linearVelocity = Vector3.zero;
            physicsRigidbody.angularVelocity = Vector3.zero;
            physicsRigidbody.isKinematic = true;
            physicsRigidbody.detectCollisions = false;
        }
    }

    private IEnumerator EnableGravityAfterDeathDelay()
    {
        yield return new WaitForSeconds(gravityDelayAfterDeath);

        for (int i = 0; i < physicsRigidbodies.Length; i++)
        {
            Rigidbody physicsRigidbody = physicsRigidbodies[i];
            if (physicsRigidbody == null)
            {
                continue;
            }

            physicsRigidbody.isKinematic = false;
            physicsRigidbody.useGravity = true;
            physicsRigidbody.detectCollisions = false;
            physicsRigidbody.WakeUp();
        }
    }

    private void PlayAnim(string stateName)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(stateName))
            animator.Play(stateName, 0, 0f);
    }

    private void FlashDamage()
    {
        if (flashRenderers == null || flashRenderers.Length == 0) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            float t = Mathf.Clamp01(flashCurve.Evaluate(elapsed / flashDuration));
            ApplyFlashIntensity(t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ClearFlashColor();
        flashRoutine = null;
    }

    private void PlayDeathFeedback()
    {
        skillEffectPrefab.Play();
        if (deathFeedbackRoutine != null)
        {
            StopCoroutine(deathFeedbackRoutine);
        }

        deathFeedbackRoutine = StartCoroutine(DeathFeedbackRoutine(transform.localPosition));
    }

    private IEnumerator DeathFeedbackRoutine(Vector3 deathLocalPosition)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, deathFlashDuration);

        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float t = Mathf.Clamp01(deathFeedbackCurve.Evaluate(progress));
            ApplyDeathFeedbackIntensity(t, deathLocalPosition, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = deathLocalPosition;
        ClearFlashColor();
        deathFeedbackRoutine = null;
    }

    private void ApplyFlashIntensity(float t)
    {
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(flashBlock);
            flashBlock.SetColor(BaseColorId, Color.Lerp(originalBaseColors[i], flashColor, t));
            flashBlock.SetColor(ColorId, Color.Lerp(originalColors[i], flashColor, t));
            flashBlock.SetColor(EmissionColorId, emissionFlashColor * t);
            r.SetPropertyBlock(flashBlock);
        }
        transform.localScale = originalScale * (1f + scalePunch * t);
    }

    private void ApplyDeathFeedbackIntensity(float t, Vector3 deathLocalPosition, float elapsed)
    {
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(flashBlock);
            flashBlock.SetColor(BaseColorId, Color.Lerp(originalBaseColors[i], deathFlashColor, t));
            flashBlock.SetColor(ColorId, Color.Lerp(originalColors[i], deathFlashColor, t));
            flashBlock.SetColor(EmissionColorId, deathEmissionFlashColor * t);
            r.SetPropertyBlock(flashBlock);
        }

        transform.localScale = originalScale * (1f + deathScalePunch * t);

        if (deathShakeMagnitude <= 0f || deathShakeFrequency <= 0f)
        {
            transform.localPosition = deathLocalPosition;
            return;
        }

        float shakeX = Mathf.Sin(elapsed * deathShakeFrequency) * deathShakeMagnitude * t;
        float shakeZ = Mathf.Cos(elapsed * deathShakeFrequency * 1.37f) * deathShakeMagnitude * t;
        transform.localPosition = deathLocalPosition + new Vector3(shakeX, 0f, shakeZ);
    }

    private void ClearFlashColor()
    {
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
        transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (despawnNotified) return;
        if (!configured) currentHealth = maxHealth;
        despawnNotified = true;
        Despawned?.Invoke(this);
    }
}
