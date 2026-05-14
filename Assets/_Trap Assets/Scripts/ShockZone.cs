using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stutters every zombie that enters the trigger:
///   freeze (shockFreezeTime) → run (shockRunTime) → freeze → …
/// Stops when the zombie exits or the zone expires.
/// Requires a SphereCollider (Is Trigger = true) on the same GameObject.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class ShockZone : MonoBehaviour
{
    private static readonly int PropColor = Shader.PropertyToID("_Color");
    private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");

    private float duration = 4f;
    private float fadeDuration = 0.8f;
    private float shockFreezeTime = 0.3f;
    private float shockRunTime = 0.4f;

    private float timer;
    private bool isFading;

    private Renderer[] renderers;
    private ParticleSystem[] particles;

    // Per-zombie shock coroutine handle so we can stop it on exit
    private readonly Dictionary<ZombieRuntime, Coroutine> activeShocks
        = new Dictionary<ZombieRuntime, Coroutine>();

    public void Init(float radius, float dur, float fade, float freezeTime, float runTime)
    {
        duration = dur;
        fadeDuration = fade;
        shockFreezeTime = freezeTime;
        shockRunTime = runTime;

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

        if (!isFading && timer >= duration - fadeDuration)
        {
            isFading = true;
            ThawAll();
            StartCoroutine(FadeOut());
        }

        if (timer >= duration)
            Destroy(gameObject);
    }

    // ── Trigger events ────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && !activeShocks.ContainsKey(zombie))
        {
            Coroutine cr = StartCoroutine(ShockLoop(zombie));
            activeShocks[zombie] = cr;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && activeShocks.TryGetValue(zombie, out Coroutine cr))
        {
            if (cr != null) StopCoroutine(cr);
            if (zombie.IsFrozen) zombie.Thaw();
            activeShocks.Remove(zombie);
        }
    }

    // ── Shock loop per zombie ─────────────────────────────────────────────────

    private IEnumerator ShockLoop(ZombieRuntime zombie)
    {
        while (zombie != null && timer < duration)
        {
            zombie.Freeze();
            yield return new WaitForSeconds(shockFreezeTime);

            if (zombie == null) break;

            zombie.Thaw();
            yield return new WaitForSeconds(shockRunTime);
        }

        // Leave zombie running when the loop ends naturally
        if (zombie != null && zombie.IsFrozen) zombie.Thaw();
        activeShocks.Remove(zombie);
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void ThawAll()
    {
        foreach (var kvp in activeShocks)
        {
            if (kvp.Value != null) StopCoroutine(kvp.Value);
            if (kvp.Key != null && kvp.Key.IsFrozen) kvp.Key.Thaw();
        }
        activeShocks.Clear();
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, col != null ? col.radius : 1f);
    }
}