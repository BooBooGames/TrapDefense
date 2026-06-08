using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ZombieCrowdSpawner : MonoBehaviour
{
    private static Material fallbackMaterial;

    [SerializeField] private ZombiePath sharedPath;
    [SerializeField] private ZombieWaveConfig levelConfig;
    [SerializeField] private bool autoStartOnPlay = false;

    private int spawnedCount;
    private int aliveZombies;
    private int totalPlannedZombies;
    private int completedZombies;
    private int completedWaves;
    private int currentWavePlannedZombies;
    private int currentWaveCompletedZombies;
    private int currentWaveNumber;
    private bool wavesRunning;
    private GameViewScreen gameViewScreen;

    public event Action<int, int, float> ProgressChanged;

    public int CurrentWaveNumber => currentWaveNumber;
    public int TotalWaves => levelConfig != null ? levelConfig.TotalWaves : 0;
    public float OverallProgress
    {
        get
        {
            if (TotalWaves <= 0)
            {
                return 0f;
            }

            float completedWaveProgress = completedWaves;
            float currentWaveProgress = currentWavePlannedZombies > 0
                ? currentWaveCompletedZombies / (float)currentWavePlannedZombies
                : 0f;

            return Mathf.Clamp01((completedWaveProgress + currentWaveProgress) / TotalWaves);
        }
    }

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

        totalPlannedZombies = CalculateTotalZombieCount();
        completedZombies = 0;
        completedWaves = 0;
        currentWavePlannedZombies = 0;
        currentWaveCompletedZombies = 0;
        currentWaveNumber = 0;
        wavesRunning = true;
        if (gameViewScreen == null)
        {
            gameViewScreen = GameViewScreen.Instance;
        }
        if (gameViewScreen != null)
        {
            gameViewScreen.InitializePlayerHealth();
        }
        NotifyProgressChanged();
        StartCoroutine(RunWaveSequence());
    }

    public void StopWaves()
    {
        StopAllCoroutines();
        wavesRunning = false;

        ZombieRuntime[] zombies = FindObjectsByType<ZombieRuntime>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] != null)
            {
                Destroy(zombies[i].gameObject);
            }
        }

        aliveZombies = 0;
        currentWavePlannedZombies = 0;
        currentWaveCompletedZombies = 0;
        currentWaveNumber = 0;
        NotifyProgressChanged();
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

            currentWaveNumber = waveIndex + 1;
            currentWavePlannedZombies = wave.GetTotalZombieCount();
            currentWaveCompletedZombies = 0;
            NotifyProgressChanged();

            if (sharedPath != null)
            {
                sharedPath.SetRoadWidth(wave.roadWidth);
            }

            yield return StartCoroutine(SpawnWave(wave));

            while (aliveZombies > 0)
            {
                yield return null;
            }

            completedWaves = Mathf.Min(TotalWaves, completedWaves + 1);
            currentWavePlannedZombies = 0;
            currentWaveCompletedZombies = 0;
            NotifyProgressChanged();

            Debug.Log($"Wave {currentWaveNumber} completed. Total completed zombies: {completedZombies}/{totalPlannedZombies}");
            HomeViewScreen.AwardChestForCompletedWave(currentWaveNumber);
            gameViewScreen.ShowChestTriggerImage(currentWaveNumber);
            PlayerXpSystem.Instance?.AwardWaveCompletionBonus();

            if (waveIndex < waves.Length - 1 && wave.delayBeforeNextWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayBeforeNextWave);
            }
        }

        wavesRunning = false;
        gameViewScreen?.HandleAllWavesCompleted();
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

        for (int entryIndex = 0; entryIndex < spawnQueue.Count; entryIndex++)
        {
            ZombieWaveEntry entry = spawnQueue[entryIndex];
            SpawnZombie(entry, 0f);

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

        GameObject zombie = Instantiate(entry.prefab, transform.position, Quaternion.identity);

        string zombieLabel = string.IsNullOrWhiteSpace(entry.zombieTypeName) ? "Zombie" : entry.zombieTypeName;
        zombie.name = $"{zombieLabel}_{spawnedCount:000}";

        ZombieRuntime runtime = zombie.GetComponent<ZombieRuntime>();
        if (runtime == null)
        {
            runtime = zombie.AddComponent<ZombieRuntime>();
        }

        runtime.Configure(entry.health);
        runtime.ConfigureRewards(entry.coinReward, entry.gemReward);
        runtime.ConfigureKillEffect(entry.killEffectPrefab);
        runtime.Despawned += OnZombieDespawned;
        runtime.Killed += OnZombieKilled;

        ZombiePathFollower follower = zombie.GetComponent<ZombiePathFollower>();
        if (follower == null)
        {
            follower = zombie.AddComponent<ZombiePathFollower>();
        }

        /*  ZombieWeaponCollision weaponCollision = zombie.GetComponent<ZombieWeaponCollision>();
         if (weaponCollision == null)
         {
             weaponCollision = zombie.AddComponent<ZombieWeaponCollision>();
         } */

        follower.ConfigureMovement(entry.moveSpeed, entry.roadWidthUsage);
        follower.Initialize(sharedPath, initialDistance, spawnedCount + 1, _ => HandleZombieReachedEnd(runtime));

        aliveZombies++;
        spawnedCount++;
    }

    private void OnZombieDespawned(ZombieRuntime zombie)
    {
        zombie.Despawned -= OnZombieDespawned;
        zombie.Killed -= OnZombieKilled;
        aliveZombies = Mathf.Max(0, aliveZombies - 1);
        completedZombies = Mathf.Min(totalPlannedZombies, completedZombies + 1);
        currentWaveCompletedZombies = Mathf.Min(currentWavePlannedZombies, currentWaveCompletedZombies + 1);
        NotifyProgressChanged();
    }

    private void OnZombieKilled(ZombieRuntime zombie, Vector3 deathPosition)
    {
        SpawnKillEffect(zombie, deathPosition);
        SpawnCoinBillboard(zombie);

        gameViewScreen?.AddInGameCoins(zombie.CoinReward);
        PlayerCurrencySystem.AddGems(zombie.GemReward);

        if (PlayerXpSystem.Instance != null)
        {
            PlayerXpSystem.Instance.AddXp(1);

            if (PlayerXpSystem.Instance.TryRollScrapCollectorGearReward())
            {
                gameViewScreen?.AddGears(1);
            }
        }
    }


    private void SpawnKillEffect(ZombieRuntime zombie, Vector3 deathPosition)
    {
        ParticleSystem effectPrefab = zombie.KillEffectPrefab;

        ParticleSystem effectInstance = Instantiate(effectPrefab, deathPosition, Quaternion.identity);
        effectInstance.Play();
    }

    private void SpawnCoinBillboard(ZombieRuntime zombie)
    {
        CoinBillboard billboard = Instantiate(levelConfig.coinBillboardPrefab, zombie.GetHeadBillboardPosition(), Quaternion.identity);
        billboard.Show(zombie.CoinReward);
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
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            (queue[i], queue[swapIndex]) = (queue[swapIndex], queue[i]);
        }

        return queue;
    }

    private int CalculateTotalZombieCount()
    {
        if (levelConfig == null || levelConfig.Waves == null)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < levelConfig.Waves.Length; i++)
        {
            WaveDefinition wave = levelConfig.Waves[i];
            if (wave != null)
            {
                total += wave.GetTotalZombieCount();
            }
        }

        return total;
    }

    private void NotifyProgressChanged()
    {
        ProgressChanged?.Invoke(currentWaveNumber, TotalWaves, OverallProgress);
    }

    private void HandleZombieReachedEnd(ZombieRuntime zombieRuntime)
    {
        if (zombieRuntime != null)
        {
            zombieRuntime.MarkEscaped();
        }
    }

    public void SetGameViewScreen(GameViewScreen screen)
    {
        gameViewScreen = screen;
    }
}
