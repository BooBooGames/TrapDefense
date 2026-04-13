using System.Collections;
using UnityEngine;

public class ZombieCrowdSpawner : MonoBehaviour
{
    private static Material fallbackMaterial;

    [SerializeField] private ZombiePath sharedPath;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField][Min(1)] private int initialZombieCount = 18;
    [SerializeField][Min(0.05f)] private float spawnInterval = 0.45f;
    [SerializeField][Min(0f)] private float initialSpacing = 1.35f;
    [SerializeField] private Vector3 fallbackZombieScale = new Vector3(0.6f, 1.15f, 0.6f);
    [SerializeField] private Color fallbackZombieColor = new Color(0.3f, 0.75f, 0.36f, 1f);
    [SerializeField] private bool autoSpawnOnStart = true;

    private int spawnedCount;

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < initialZombieCount; i++)
        {
            TrySpawnZombie(-(i * initialSpacing));
            yield return new WaitForSeconds(spawnInterval);
        }

        while (true)
        {
            TrySpawnZombie(-initialSpacing);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawnZombie(float initialDistance)
    {
        if (sharedPath == null || !sharedPath.HasValidPath)
        {
            return;
        }

        GameObject zombie = zombiePrefab != null
            ? Instantiate(zombiePrefab, transform.position, Quaternion.identity)
            : CreateFallbackZombie();

        zombie.name = $"Zombie_{spawnedCount:000}";

        ZombiePathFollower follower = zombie.GetComponent<ZombiePathFollower>();
        if (follower == null)
        {
            follower = zombie.AddComponent<ZombiePathFollower>();
        }

        follower.Initialize(sharedPath, initialDistance, spawnedCount + 1);
        spawnedCount++;
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
}
