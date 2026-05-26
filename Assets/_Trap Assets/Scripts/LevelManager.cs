using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public LevelHandler[] Levels;

    public LevelHandler activeLevelInstance;
    private bool activeLevelIsSpawnedPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentLevel = GetSavedLevel();
            LoadLevel(CurrentLevel);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextLevel()
    {
        CurrentLevel = CurrentLevel >= Levels.Length ? 1 : CurrentLevel + 1;
        LoadLevel(CurrentLevel);
        SaveCurrentLevel();
        HomeViewScreen.ResetChestRewardsForLevel(CurrentLevel);
        Debug.Log("Loading Level: " + CurrentLevel);
    }

    private int GetSavedLevel()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        int maxLevel = Mathf.Max(1, Levels.Length);
        int savedLevel = saveData.currentLevel <= 0 ? 1 : saveData.currentLevel;
        return Mathf.Clamp(savedLevel, 1, maxLevel);
    }

    private void SaveCurrentLevel()
    {
        SaveGameData saveData = GameSaveSystem.Load();
        saveData.currentLevel = CurrentLevel;
        GameSaveSystem.Save(saveData);
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
            activeLevelInstance = levelPrefab;
            activeLevelIsSpawnedPrefab = false;
            return;
        }

        SetSceneLevelObjectsActive(-1);
        DestroyActiveLevelInstance();
        activeLevelInstance = Instantiate(levelPrefab);
        activeLevelIsSpawnedPrefab = true;
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
        if (activeLevelInstance != null && activeLevelIsSpawnedPrefab)
        {
            Destroy(activeLevelInstance.gameObject);
            activeLevelInstance = null;
            activeLevelIsSpawnedPrefab = false;
        }
    }
}
