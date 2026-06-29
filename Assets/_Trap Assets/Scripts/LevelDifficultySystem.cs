using UnityEngine;

public static class LevelDifficultySystem
{
    public const float EnemyCountIncreasePerLevel = 0.05f;
    public const float EnemyMoveSpeedIncreasePerLevel = 0.05f;

    public static int CurrentDifficultyLevel
    {
        get
        {
            if (LevelManager.Instance != null)
            {
                return LevelManager.Instance.CurrentDifficultyLevel;
            }

            SaveGameData saveData = GameSaveSystem.Load();
            return Mathf.Max(1, saveData.difficultyLevel > 0 ? saveData.difficultyLevel : saveData.currentLevel);
        }
    }

    public static float EnemyCountMultiplier => GetDifficultyMultiplier(EnemyCountIncreasePerLevel, CurrentDifficultyLevel);
    public static float EnemyMoveSpeedMultiplier => GetDifficultyMultiplier(EnemyMoveSpeedIncreasePerLevel, CurrentDifficultyLevel);

    public static float GetDifficultyMultiplier(float increasePerLevel, int difficultyLevel)
    {
        int normalizedDifficultyLevel = Mathf.Max(1, difficultyLevel);
        return Mathf.Pow(1f + Mathf.Max(0f, increasePerLevel), normalizedDifficultyLevel - 1);
    }

    public static int GetScaledEnemyCount(int baseCount)
    {
        if (baseCount <= 0)
        {
            return 0;
        }

        return Mathf.Max(baseCount, Mathf.RoundToInt(baseCount * EnemyCountMultiplier));
    }

    public static float GetScaledEnemyMoveSpeed(float baseMoveSpeed)
    {
        return Mathf.Max(0f, baseMoveSpeed) * EnemyMoveSpeedMultiplier;
    }
}
