using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class Utils
{
    public static bool IsTutorialDone(TutorialType tutorialType)
    {
        return PlayerPrefsExtension.GetBool(PlayerPrefsKeys.GetTutorialDoneKey(tutorialType.ToString()));
    }

    public static void SetTutorialDone(TutorialType tutorialType)
    {
        PlayerPrefsExtension.SetBool(PlayerPrefsKeys.GetTutorialDoneKey(tutorialType.ToString()), true);
    }

    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * t;
    }
}
