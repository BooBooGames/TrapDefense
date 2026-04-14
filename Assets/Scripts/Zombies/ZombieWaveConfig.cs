using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ZombieLevelConfig", menuName = "TrapDefense/Zombies/Level Wave Config")]
public class ZombieWaveConfig : ScriptableObject
{
    [SerializeField] [Min(1)] private int totalWaves = 9;
    [SerializeField] private WaveDefinition[] waves = new WaveDefinition[9];

    public int TotalWaves => totalWaves;
    public WaveDefinition[] Waves => waves;

    private void OnValidate()
    {
        totalWaves = Mathf.Max(1, totalWaves);

        if (waves == null)
        {
            waves = new WaveDefinition[totalWaves];
            return;
        }

        if (waves.Length != totalWaves)
        {
            Array.Resize(ref waves, totalWaves);
        }
    }
}

[Serializable]
public class WaveDefinition
{
    [Min(0.5f)] public float roadWidth = 4f;
    [Min(0.05f)] public float spawnInterval = 0.45f;
    [Min(0f)] public float delayBeforeNextWave = 2f;
    public ZombieWaveEntry[] zombieEntries;

    public int GetTotalZombieCount()
    {
        if (zombieEntries == null)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < zombieEntries.Length; i++)
        {
            total += Mathf.Max(0, zombieEntries[i].count);
        }

        return total;
    }
}

[Serializable]
public class ZombieWaveEntry
{
    public string zombieTypeName;
    public GameObject prefab;
    [Min(1)] public int count = 1;
    [Min(1f)] public float health = 10f;
    [Range(0.1f, 1f)] public float roadWidthUsage = 1f;
}
