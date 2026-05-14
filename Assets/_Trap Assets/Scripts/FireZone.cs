using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Damages and plays a burn animation on every zombie inside the trigger.
/// Stops the burn animation when a zombie exits or the zone expires.
/// Requires a SphereCollider (Is Trigger = true) on the same GameObject.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class FireZone : MonoBehaviour
{
    private static readonly int PropColor = Shader.PropertyToID("_Color");
    private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");

    private float duration = 4f;
    private float fadeDuration = 1f;
    private float damagePerTick = 3f;
    private float tickInterval = 0.4f;

    private float timer;
    private float damageTimer;
    private bool isFading;

    private Renderer[] renderers;
    private ParticleSystem[] particles;

    private readonly List<ZombieRuntime> burningZombies = new List<ZombieRuntime>();

    public void Init(float radius, float dur, float fade, float dmg, float interval)
    {
        duration = dur;
        fadeDuration = fade;
        damagePerTick = dmg;
        tickInterval = interval;

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
            FireTick();
        }

        if (!isFading && timer >= duration - fadeDuration)
        {
            isFading = true;
            StopBurnAll();
            StartCoroutine(FadeOut());
        }

        if (timer >= duration)
            Destroy(gameObject);
    }

    // ── Trigger events ────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && !burningZombies.Contains(zombie))
        {
            burningZombies.Add(zombie);
            zombie.PlayBurnAnimation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ZombieRuntime zombie = other.GetComponent<ZombieRuntime>();
        if (zombie != null && burningZombies.Contains(zombie))
        {
            zombie.StopBurnAnimation();
            burningZombies.Remove(zombie);
        }
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    private void FireTick()
    {
        for (int i = burningZombies.Count - 1; i >= 0; i--)
        {
            ZombieRuntime zombie = burningZombies[i];
            if (zombie == null) { burningZombies.RemoveAt(i); continue; }
            zombie.ApplyDamage(damagePerTick);
        }
    }

    private void StopBurnAll()
    {
        for (int i = burningZombies.Count - 1; i >= 0; i--)
        {
            if (burningZombies[i] != null) burningZombies[i].StopBurnAnimation();
        }
        burningZombies.Clear();
    }

    private void OnDestroy() => StopBurnAll();

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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, col != null ? col.radius : 1f);
    }
}