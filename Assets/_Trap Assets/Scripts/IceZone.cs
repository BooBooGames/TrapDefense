using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Freezes every zombie that enters the trigger (stops movement + pauses animator).
/// Thaws them all when the zone expires.
/// Requires a SphereCollider (Is Trigger = true) on the same GameObject.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class IceZone : MonoBehaviour
{
    private static readonly int PropColor = Shader.PropertyToID("_Color");
    private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");

    private float iceDuration = 4f;
    private float fadeDuration = 1f;

    private Renderer[] renderers;
    private ParticleSystem[] particles;

    private readonly List<ZombieRuntime> frozenZombies = new List<ZombieRuntime>();

    public void Init(float radius, float dur, float fade)
    {
        iceDuration = dur;
        fadeDuration = fade;

        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
    }

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        particles = GetComponentsInChildren<ParticleSystem>();
        StartCoroutine(LifetimeRoutine());
    }

    // ── Trigger events ────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && !frozenZombies.Contains(zombie))
        {
            zombie.Freeze();
            frozenZombies.Add(zombie);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Zombie walked/was pushed out — thaw it and stop tracking it
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && frozenZombies.Contains(zombie))
        {
            zombie.Thaw();
            frozenZombies.Remove(zombie);
        }
    }

    // ── Lifetime ──────────────────────────────────────────────────────────────

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(iceDuration - fadeDuration);
        ThawAll();
        StartCoroutine(FadeOut());
        yield return new WaitForSeconds(fadeDuration);
        Destroy(gameObject);
    }

    private void ThawAll()
    {
        for (int i = frozenZombies.Count - 1; i >= 0; i--)
        {
            if (frozenZombies[i] != null) frozenZombies[i].Thaw();
        }
        frozenZombies.Clear();
    }

    private void OnDestroy() => ThawAll();

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, col != null ? col.radius : 1f);
    }
}