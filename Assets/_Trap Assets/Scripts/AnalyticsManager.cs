using System;
using UnityEngine;

public enum ProgressionType
{
    Start,
    Finish,
    Fail
}

public static class AnalyticsManager
{
    private static float LastLevelStartTime;
    private static readonly float InterstitialWaitTimer = 180;
    private static readonly int WavesBuffer = 2;

    private static int WavesCompletedFromLastIS = 0;
    private static float LastInterstitialTime;

    public static void ShowRVEvent(string eventName)
    {
        GameAnalyticsController.Miscellaneous.NewDesignEvent($"rv:{eventName}");
    }

    public static void RequestInterstitialAdAfterWaveEnd()
    {
        WavesCompletedFromLastIS += 1;
                
        if (WavesCompletedFromLastIS < WavesBuffer)
        {
            Debug.Log($"Didn't show IS as the waves completed after the last IS is = {WavesCompletedFromLastIS}");

            return;
        }

        if((Time.realtimeSinceStartup - LastInterstitialTime) >=  InterstitialWaitTimer)
        {
            Debug.Log($"Show RV");
            WavesCompletedFromLastIS = 0;
            LastInterstitialTime = Time.realtimeSinceStartup;
        }

        Debug.Log($"Didn't show IS as the time from last IS to current time is = {(Time.realtimeSinceStartup - LastInterstitialTime)}");
    }

    public static void ResetISParameters()
    {
        WavesCompletedFromLastIS = 0;
        LastInterstitialTime = Time.realtimeSinceStartup;
    }

    public static void ShowLevelProgressionEvent(int levelNumber, ProgressionType progressionType)
    {
        if (progressionType == ProgressionType.Start)
        {
            LastLevelStartTime = Time.realtimeSinceStartup;
            GameAnalyticsController.LevelBasedProgressionRelated.LogLevelStartEventWithTime(levelNumber);
        }
        else if (progressionType == ProgressionType.Fail)
        {
            var levelData = new GameAnalyticsController.LevelProgressTimeData(levelNumber, LastLevelStartTime);
            GameAnalyticsController.LevelBasedProgressionRelated.LogLevelFailEventWithTime(levelData);
        }
        else if (progressionType == ProgressionType.Finish)
        {
            var levelData = new GameAnalyticsController.LevelProgressTimeData(levelNumber, LastLevelStartTime);
            GameAnalyticsController.LevelBasedProgressionRelated.LogLevelEndEventWithTime(levelData);
        }
    }
}
