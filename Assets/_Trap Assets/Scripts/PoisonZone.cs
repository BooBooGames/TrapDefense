using System.Collections;
using UnityEngine;

public class PoisonZone : MonoBehaviour
{
    [Header("Zone Config")]
    public float radius = 5f;
    public float duration = 5f;
    public float fadeDuration = 1.5f;

    [Header("Damage")]
    public float damagePerTick = 1f;
    public float tickInterval = 0.5f;

    private float timer = 0f;
    private float damageTimer = 0f;
    private Renderer[] renderers;
    private ParticleSystem[] particles;
    private bool isFading = false;

    public void Init(float r, float dur, float fade)
    {
        radius = r;
        duration = dur;
        fadeDuration = fade;
    }

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        damageTimer += Time.deltaTime;

        // Damage tick
        if (damageTimer >= tickInterval)
        {
            damageTimer = 0f;
            ApplyDamageTick();
        }

        // Start fading when (duration - fadeDuration) is reached
        float fadeStartTime = duration - fadeDuration;
        if (timer >= fadeStartTime && !isFading)
        {
            isFading = true;
            StartCoroutine(FadeOut());
        }

        // Destroy after full duration
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    void ApplyDamageTick()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider col in hits)
        {
            ZombieRuntime zombie = col.GetComponent<ZombieRuntime>();
            if (zombie != null)
            {
                zombie.ApplyDamage(damagePerTick);
            }
        }
    }

    IEnumerator FadeOut()
    {
        // Stop particles from emitting new ones
        foreach (ParticleSystem ps in particles)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        float elapsed = 0f;

        // Collect original alpha values
        float[] originalAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalAlphas[i] = renderers[i].material.color.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = renderers[i].material.color;
                c.a = Mathf.Lerp(originalAlphas[i], 0f, t);
                renderers[i].material.color = c;
            }

            yield return null;
        }
    }

    // Visualize radius in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}