using System;
using UnityEngine;
using UnityEngine.Rendering;

public static class PlayerPrefsKeys
{
    public const string IS_WELCOMED = "Is_Welcomed";

    public const string IS_TUTORIAL_DONE = "Is_Tutorial_{0}_Done";

    public static string GetTutorialDoneKey(string tutorialName)
    {
        return string.Format(IS_TUTORIAL_DONE, tutorialName);
    }
}