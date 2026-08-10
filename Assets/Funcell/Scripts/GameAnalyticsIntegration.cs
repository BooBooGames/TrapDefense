using System;
using UnityEngine;

public class GameAnalyticsIntegration : MonoBehaviour
{
    void Start()
    {
        GameAnalyticsSDK.GameAnalytics.Initialize();
    }
}
