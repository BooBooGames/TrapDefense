/*using System.Collections;
using UnityEngine;

/// <summary>
/// Damages every zombie inside the radius every tickInterval seconds.
/// All values are passed in via Init() — nothing relies on serialized fields.
/// </summary>
public class PoisonZone : MonoBehaviour
{
    private static readonly int PropColor = Shader.PropertyToID("_Color");
    private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");

    private float radius = 5f;
    private float duration = 5f;
    private float fadeDuration = 1.5f;
    private float damagePerTick = 2f;
    private float tickInterval = 0.5f;

    private float timer;
    private float damageTimer;
    private bool isFading;

    private Renderer[] renderers;
    private ParticleSystem[] particles;

    /// <summary>Called by SpellPlacer immediately after AddComponent.</summary>
    public void Init(float r, float dur, float fade, float dmg, float interval)
    {
        radius = r;
        duration = dur;
        fadeDuration = fade;
        damagePerTick = dmg;
        tickInterval = interval;
    }

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        damageTimer += Time.deltaTime;

        if (damageTimer >= tickInterval)
        {
            damageTimer = 0f;
            DamageTick();
        }

        if (!isFading && timer >= duration - fadeDuration)
        {
            isFading = true;
            StartCoroutine(FadeOut());
        }

        if (timer >= duration)
            Destroy(gameObject);
    }

    private void DamageTick()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider col in hits)
        {
            ZombieRuntime zombie = col.GetComponent<ZombieRuntime>();
            if (zombie != null)
                zombie.ApplyDamage(damagePerTick);
        }
    }

    private IEnumerator FadeOut()
    {
        foreach (ParticleSystem ps in particles)
        {
            var e = ps.emission;
            e.enabled = false;
        }

        float[] startAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startAlphas[i] = GetAlpha(renderers[i].material);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    SetAlpha(renderers[i].material, Mathf.Lerp(startAlphas[i], 0f, t));
            }
            yield return null;
        }
    }

    private float GetAlpha(Material mat)
    {
        if (mat.HasProperty(PropColor)) return mat.GetColor(PropColor).a;
        if (mat.HasProperty(PropTintColor)) return mat.GetColor(PropTintColor).a;
        return 1f;
    }

    private void SetAlpha(Material mat, float a)
    {
        if (mat.HasProperty(PropColor))
        {
            Color c = mat.GetColor(PropColor); c.a = a; mat.SetColor(PropColor, c);
        }
        else if (mat.HasProperty(PropTintColor))
        {
            Color c = mat.GetColor(PropTintColor); c.a = a; mat.SetColor(PropTintColor, c);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Damages every zombie inside the trigger every tickInterval seconds.
/// Requires a SphereCollider (Is Trigger = true) on the same GameObject,
/// which SpellPlacer sets to match spellRadius automatically.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class PoisonZone : MonoBehaviour
{
    private static readonly int PropColor = Shader.PropertyToID("_Color");
    private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");

    private float duration = 5f;
    private float fadeDuration = 1.5f;
    private float damagePerTick = 2f;
    private float tickInterval = 0.5f;

    private float timer;
    private float damageTimer;
    private bool isFading;

    private Renderer[] renderers;
    private ParticleSystem[] particles;

    // Zombies currently inside the trigger
    private readonly List<ZombieRuntime> zombiesInZone = new List<ZombieRuntime>();

    public void Init(float radius, float dur, float fade, float dmg, float interval)
    {
        duration = dur;
        fadeDuration = fade;
        damagePerTick = dmg;
        tickInterval = interval;

        // Size the trigger collider to match the spell radius
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        damageTimer += Time.deltaTime;

        if (damageTimer >= tickInterval)
        {
            damageTimer = 0f;
            DamageTick();
        }

        if (!isFading && timer >= duration - fadeDuration)
        {
            isFading = true;
            StartCoroutine(FadeOut());
        }

        if (timer >= duration)
            Destroy(gameObject);
    }


    // ── Trigger events ────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && !zombiesInZone.Contains(zombie))
            zombiesInZone.Add(zombie);
    }

    private void OnTriggerExit(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null)
            zombiesInZone.Remove(zombie);
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    private void DamageTick()
    {
        // Iterate backwards so we can safely remove dead zombies
        for (int i = zombiesInZone.Count - 1; i >= 0; i--)
        {
            ZombieRuntime zombie = zombiesInZone[i];
            if (zombie == null) { zombiesInZone.RemoveAt(i); continue; }
            zombie.ApplyDamage(damagePerTick);
        }
    }

    // ── Fade out ──────────────────────────────────────────────────────────────

    private IEnumerator FadeOut()
    {
        foreach (ParticleSystem ps in particles)
        {
            var e = ps.emission;
            e.enabled = false;
        }

        float[] startAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startAlphas[i] = GetAlpha(renderers[i].material);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    SetAlpha(renderers[i].material, Mathf.Lerp(startAlphas[i], 0f, t));
            }
            yield return null;
        }
    }

    // ── Material helpers ──────────────────────────────────────────────────────

    private float GetAlpha(Material mat)
    {
        if (mat.HasProperty(PropColor)) return mat.GetColor(PropColor).a;
        if (mat.HasProperty(PropTintColor)) return mat.GetColor(PropTintColor).a;
        return 1f;
    }

    private void SetAlpha(Material mat, float a)
    {
        if (mat.HasProperty(PropColor))
        {
            Color c = mat.GetColor(PropColor); c.a = a; mat.SetColor(PropColor, c);
        }
        else if (mat.HasProperty(PropTintColor))
        {
            Color c = mat.GetColor(PropTintColor); c.a = a; mat.SetColor(PropTintColor, c);
        }
    }

    private void OnDrawGizmosSelected()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, col != null ? col.radius : 1f);
    }
}