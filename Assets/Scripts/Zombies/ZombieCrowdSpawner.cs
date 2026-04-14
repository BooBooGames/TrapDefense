using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieCrowdSpawner : MonoBehaviour
{
    private static Material fallbackMaterial;

    [SerializeField] private ZombiePath sharedPath;
    [SerializeField] private ZombieWaveConfig levelConfig;
    [SerializeField][Min(0.1f)] private float zombieMoveSpeed = 2.5f;
    [SerializeField][Min(0f)] private float initialSpacing = 1.35f;
    [SerializeField] private Vector3 fallbackZombieScale = new Vector3(0.6f, 1.15f, 0.6f);
    [SerializeField] private Color fallbackZombieColor = new Color(0.3f, 0.75f, 0.36f, 1f);
    [SerializeField] private bool autoStartOnPlay = true;

    private int spawnedCount;
    private int aliveZombies;
    private bool wavesRunning;

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        if (wavesRunning || levelConfig == null || sharedPath == null)
        {
            return;
        }

        wavesRunning = true;
        StartCoroutine(RunWaveSequence());
    }

    private IEnumerator RunWaveSequence()
    {
        WaveDefinition[] waves = levelConfig.Waves;
        if (waves == null)
        {
            wavesRunning = false;
            yield break;
        }

        for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            WaveDefinition wave = waves[waveIndex];
            if (wave == null)
            {
                continue;
            }

            if (sharedPath != null)
            {
                sharedPath.SetRoadWidth(wave.roadWidth);
            }

            yield return StartCoroutine(SpawnWave(wave));

            while (aliveZombies > 0)
            {
                yield return null;
            }

            if (waveIndex < waves.Length - 1 && wave.delayBeforeNextWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayBeforeNextWave);
            }
        }

        wavesRunning = false;
    }

    private IEnumerator SpawnWave(WaveDefinition wave)
    {
        if (wave.zombieEntries == null)
        {
            yield break;
        }

        List<ZombieWaveEntry> spawnQueue = BuildSpawnQueue(wave);
        if (spawnQueue.Count == 0)
        {
            yield break;
        }

        int spawnedInWave = 0;
        for (int entryIndex = 0; entryIndex < spawnQueue.Count; entryIndex++)
        {
            ZombieWaveEntry entry = spawnQueue[entryIndex];
            float initialDistance = -(spawnedInWave * initialSpacing);
            SpawnZombie(entry, initialDistance);
            spawnedInWave++;

            if (wave.spawnInterval > 0f)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    private void SpawnZombie(ZombieWaveEntry entry, float initialDistance)
    {
        if (sharedPath == null || !sharedPath.HasValidPath)
        {
            return;
        }

        GameObject zombie = entry.prefab != null
            ? Instantiate(entry.prefab, transform.position, Quaternion.identity)
            : CreateFallbackZombie();

        string zombieLabel = string.IsNullOrWhiteSpace(entry.zombieTypeName) ? "Zombie" : entry.zombieTypeName;
        zombie.name = $"{zombieLabel}_{spawnedCount:000}";

        ZombieRuntime runtime = zombie.GetComponent<ZombieRuntime>();
        if (runtime == null)
        {
            runtime = zombie.AddComponent<ZombieRuntime>();
        }

        runtime.Configure(entry.health);
        runtime.Despawned += OnZombieDespawned;

        ZombiePathFollower follower = zombie.GetComponent<ZombiePathFollower>();
        if (follower == null)
        {
            follower = zombie.AddComponent<ZombiePathFollower>();
        }

        follower.ConfigureMovement(zombieMoveSpeed, entry.roadWidthUsage);
        follower.Initialize(sharedPath, initialDistance, spawnedCount + 1);

        aliveZombies++;
        spawnedCount++;
    }

    private void OnZombieDespawned(ZombieRuntime zombie)
    {
        zombie.Despawned -= OnZombieDespawned;
        aliveZombies = Mathf.Max(0, aliveZombies - 1);
    }

    private GameObject CreateFallbackZombie()
    {
        GameObject zombie = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        zombie.transform.localScale = fallbackZombieScale;

        Renderer renderer = zombie.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            if (fallbackMaterial == null)
            {
                fallbackMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                fallbackMaterial.color = fallbackZombieColor;
            }

            renderer.sharedMaterial = fallbackMaterial;
        }

        return zombie;
    }

    private List<ZombieWaveEntry> BuildSpawnQueue(WaveDefinition wave)
    {
        List<ZombieWaveEntry> queue = new List<ZombieWaveEntry>();

        for (int i = 0; i < wave.zombieEntries.Length; i++)
        {
            ZombieWaveEntry entry = wave.zombieEntries[i];
            if (entry == null)
            {
                continue;
            }

            int count = Mathf.Max(0, entry.count);
            for (int spawnIndex = 0; spawnIndex < count; spawnIndex++)
            {
                queue.Add(entry);
            }
        }

        // Shuffle the final spawn list so mixed-type waves interleave naturally.
        for (int i = queue.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (queue[i], queue[swapIndex]) = (queue[swapIndex], queue[i]);
        }

        return queue;
    }
}
