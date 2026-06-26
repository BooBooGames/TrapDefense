using System;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;

public static class GameAnalyticsController
{
    public static bool IsInitialized { get; private set; }
    public static AppStateType AppState { get; private set; }


    public static void Setup()
    {

    }

    static GameAnalyticsController()
    {
        if (!IsInitialized)
        {
            GameAnalyticsSDK.GameAnalytics.Initialize();
            IsInitialized = true;
            CaptureAppState();
        }
    }

    static void CaptureAppState()
    {
        //Unique key;
        string key = $"{Application.identifier}_version_0987654321";

        if (!PlayerPrefs.HasKey(key))
        {
            AppState = AppStateType.First_Open;
            //PlayerPrefs.SetString("AnalyticsData_0987654321", JsonUtility.ToJson(new AnalyticsData() { firstOpenTime = DateTime.Now, gameOpenCounter = 1 }));
        }
        else
        {
            var lastVersion = PlayerPrefs.GetString(key);
            AppState = string.Equals(lastVersion, Application.version, System.StringComparison.InvariantCultureIgnoreCase) ? AppStateType.Normal_Open : AppStateType.Version_Update;
        }
        PlayerPrefs.SetString(key, Application.version);
    }

    public static class StageLevelBasedProgressionRelated
    {
        public static void LogStageLevelStartEvent(int stage, int level)
        {
            CheckGAInit();
            if (level < 1 || stage < 1) if (level < 1)
                    throw new System.Exception("Level/stage num should start from 1");
            var lvl = "level_" + level.ToString("000");
            var world = "stage_" + stage.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, world, lvl);
        }

        public static void LogStageLevelEndEvent(int stage, int level)
        {
            CheckGAInit();
            if (level < 1 || stage < 1)
                throw new System.Exception("Level/stage num should start from 1");
            var lvl = "level_" + level.ToString("000");
            var world = "stage_" + stage.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, world, lvl);
        }

        public static void LogStageLevelFailEvent(int stage, int level)
        {
            CheckGAInit();
            if (level < 1 || stage < 1)
                throw new System.Exception("Level/stage num should start from 1");
            var lvl = "level_" + level.ToString("000");
            var world = "stage_" + stage.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, world, lvl);
        }
    }

    public static class LevelBasedProgressionRelated
    {
        public static LevelProgressTimeData levelProgressTimeData { get; private set; }

        public static void Puzzle_LogLevelStartEvent(string eventname, int level)
        {
            CheckGAInit();
            if (level < 1)
                throw new System.Exception("Level num should start from 1");
            var lvl = "puzzle_level_" + level.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, lvl);
        }

        public static void Puzzle_LogLevelEndEvent(int level)
        {
            CheckGAInit();
            if (level < 1)
                throw new System.Exception("Level num should start from 1");
            var lvl = "puzzle_level_" + level.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, lvl);
        }

        public static void LogLevelFailEvent(int level)
        {
            CheckGAInit();
            if (level < 1)
                throw new System.Exception("Level num should start from 1");
            var lvl = "level_" + level.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, lvl);
        }


        public static LevelProgressTimeData LogLevelStartEventWithTime(int level)
        {
            CheckGAInit();

            if (level < 1)
                throw new System.Exception("Level num should start from 1");

            var lvl = "level_" + level.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, lvl);
            levelProgressTimeData = new LevelProgressTimeData(level, Time.realtimeSinceStartup);
            return levelProgressTimeData;
        }

        public static void LogLevelEndEventWithTime(LevelProgressTimeData data)
        {
            CheckGAInit();

            if (data.level < 1)
                throw new System.Exception("Level num should start from 1");

            var lvl = "level_" + data.level.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, lvl);

            var timeDiff = Time.realtimeSinceStartup - data.levelStartTime;
            GameAnalytics.NewDesignEvent("level_end_time:" + lvl, timeDiff);
        }

        public static void LogLevelFailEventWithTime(LevelProgressTimeData data)
        {
            CheckGAInit();

            if (data.level < 1)
                throw new System.Exception("Level num should start from 1");

            var lvl = "level_" + data.level.ToString("000");
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, lvl);

            var timeDiff = Time.realtimeSinceStartup - data.levelStartTime;
            GameAnalytics.NewDesignEvent("level_fail_time:" + lvl, timeDiff);
        }
    }

    public class InfiniteGameProgressionRelated
    {
        public static InfiniteLevelProgressTimeData infiniteLevelProgressTimeData { get; private set; }
        public static void LogInfiniteGameStartEvent(string levelName, int score)
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, levelName, score);
        }

        public static void LogInfiniteGameEndEvent(string levelName, int score)
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, levelName, score);
        }

        public static InfiniteLevelProgressTimeData LogInfiniteGameStartEventWithTimer(string levelName, int score)
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, levelName, score);
            infiniteLevelProgressTimeData = new InfiniteLevelProgressTimeData(levelName, Time.time);
            return infiniteLevelProgressTimeData;
        }

        public static void LogInfiniteGameEndEventWithTimer(InfiniteLevelProgressTimeData data, int score)
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, data.level, score);
            var timeDiff = Time.time - data.levelStartTime;
            GameAnalytics.NewDesignEvent("level_end_time:" + data.level, timeDiff);
        }
    }

    public static class TutorialRelated
    {
        public static void LogTutorialStartEvent(string tutorialtype)
        {
            CheckGAInit();
            if (string.IsNullOrEmpty(tutorialtype))
                GameAnalytics.NewDesignEvent("GameFlow:Tuto_Start");
            else
                GameAnalytics.NewDesignEvent("GameFlow:Tuto_Start:" + tutorialtype);
        }

        public static void LogTutorialEndEvent(string tutorialtype)
        {
            CheckGAInit();
            if (string.IsNullOrEmpty(tutorialtype))
                GameAnalytics.NewDesignEvent("GameFlow:Tuto_End");
            else
                GameAnalytics.NewDesignEvent("GameFlow:Tuto_End:" + tutorialtype);
        }

        public static void LogTutorialEvent(int tutorialStep)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("GameFlow:Tuto_Step_" + tutorialStep);
        }
    }

    public static class AdvertisementRelated
    {
        public static void LogRewardAdCloseEventWithLevel(string placement, int level)
        {
            var lvl = "level_" + level.ToString("000");
            LogRewardAdCloseEventWithLevel(placement, lvl);
        }

        public static void LogRewardAdButtonClicked(string placement, int level)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Ads_Log:RV_Clicked:" + placement + ":" + Utility.GetLevelNumberInEventFormat(level));
        }



        public static void LogRewardAdCloseEventWithLevel(string placement, string level)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Ads_Log:RV_Close:" + placement + ":" + level);
        }

        public static void LogInterstitialAdOpenEventWithLevel(string placement, string level)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Ads_Log:FS_Open:" + placement + ":" + level);
        }

        public static void LogRewardAdCloseEvent(string placement)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Ads_Log:RV_Close:" + placement);
        }

        public static void LogInterstitialAdOpenEventWithLevel(string placement, int level)
        {
            var lvl = "level_" + level.ToString("000");
            LogInterstitialAdOpenEventWithLevel(placement, lvl);
        }

        public static void LogInterstitialAdCloseEventWithLevel(string placement, int level)
        {
            var lvl = "level_" + level.ToString("000");
            GameAnalytics.NewDesignEvent("Ads_Log:FS_Close:" + placement + ":" + Utility.GetLevelNumberInEventFormat(level));
        }


        public static void LogInterstitialAdOpenEvent(string placement)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Ads_Log:FS_Open:" + placement);
        }

        public static void LogInterstitialAdCloseEvent(string placement)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Ads_Log:FS_Close:" + placement);
        }
    }

    public static class Miscellaneous
    {
        public static void LogAbTestVariantEvent(BuildVariant buildVariant)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("Variant:" + buildVariant);
        }
        public static void LogAppBootEvent(AppStateType appState1)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent("GameFlow:Boot:" + appState1);
        }

        public static void LogGameplayStatusEvent(string levelName, GameplayStateType gameplayStateType)
        {
            CheckGAInit();
            if (string.IsNullOrEmpty(levelName))
                GameAnalytics.NewDesignEvent("GameFlow:state:" + gameplayStateType);
            else
                GameAnalytics.NewDesignEvent("GameFlow:state:" + levelName + ":" + gameplayStateType);
        }

        public static void NewDesignEvent(string eventName)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent(eventName);
        }

        public static void NewDesignEvent(string eventName, float evenValue)
        {
            CheckGAInit();
            GameAnalytics.NewDesignEvent(eventName, evenValue);
        }

        public static void LogLevelTimeOnly(int level, bool isLevelPassed, float timeDiff)
        {
            var lvl = "level_" + level.ToString("000");
            if (isLevelPassed)
                GameAnalytics.NewDesignEvent("level_end_time:" + lvl, timeDiff);
            else
                GameAnalytics.NewDesignEvent("level_fail_time:" + lvl, timeDiff);
        }
    }

    public static class Utility
    {
        public static string GetLevelNumberInEventFormat(int level)
        {
            return "level_" + level.ToString("000");
        }
    }

    public enum GameplayStateType : int
    {
        Default,
        Start,
        End,
        Resume
    }

    public enum AppStateType : int
    {
        Normal_Open,
        First_Open,
        Version_Update
    }

    public enum BuildVariant
    {
        Control,
        Variant1,
        Variant2,
        Variant3,
        Variant4,
        Variant5,
        Variant6,
    }

    public struct LevelProgressTimeData
    {
        public int level { get; private set; }
        public float levelStartTime { get; private set; }

        public LevelProgressTimeData(int lvl, float levelStartTime)
        {
            this.level = lvl;
            this.levelStartTime = levelStartTime;
        }
    }

    public struct InfiniteLevelProgressTimeData
    {
        public string level { get; private set; }
        public float levelStartTime { get; private set; }

        public InfiniteLevelProgressTimeData(string lvl, float levelStartTime)
        {
            this.level = lvl;
            this.levelStartTime = levelStartTime;
        }
    }

    [System.Serializable]
    public class AnalyticsData
    {
        public DateTime firstOpenTime;
        public int gameOpenCounter;
    }

    static void CheckGAInit()
    {
        if (IsInitialized)
            return;
        throw new System.Exception("GameAnalytics is not initialized yet. Call GAEventManager.Init() to init the GA");
    }
}
