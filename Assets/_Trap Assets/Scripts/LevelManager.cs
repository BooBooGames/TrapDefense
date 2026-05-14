using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public LevelHandler[] Levels;

    public LevelHandler activeLevelInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadLevel(CurrentLevel);
    }

    public void LoadNextLevel()
    {
        CurrentLevel = CurrentLevel >= Levels.Length ? 1 : CurrentLevel + 1;
        LoadLevel(CurrentLevel);
        Debug.Log("Loading Level: " + CurrentLevel);
    }

    private void LoadLevel(int levelNumber)
    {
        int levelIndex = Mathf.Clamp(levelNumber - 1, 0, Levels.Length - 1);
        LevelHandler levelPrefab = Levels[levelIndex];

        bool usesSceneLevelObjects = levelPrefab.gameObject.scene.IsValid();
        if (usesSceneLevelObjects)
        {
            SetSceneLevelObjectsActive(levelIndex);
            DestroyActiveLevelInstance();
            return;
        }

        DestroyActiveLevelInstance();
        activeLevelInstance = Instantiate(levelPrefab);
    }

    private void SetSceneLevelObjectsActive(int activeLevelIndex)
    {
        for (int i = 0; i < Levels.Length; i++)
        {
            if (Levels[i].gameObject.scene.IsValid())
            {
                Levels[i].gameObject.SetActive(i == activeLevelIndex);
            }
        }
    }

    private void DestroyActiveLevelInstance()
    {
        if (activeLevelInstance != null)
        {
            Destroy(activeLevelInstance.gameObject);
            activeLevelInstance = null;
        }
    }
}
