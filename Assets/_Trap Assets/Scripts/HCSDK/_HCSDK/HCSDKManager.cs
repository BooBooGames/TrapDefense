using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using BoomBit.HyperCasual;
using Coredian.Firebase;
using System;
using System.Text;
using Coredian.CrossPromo;
using Coredian;
using Coredian.Attribution;
using Coredian.Advertisements.AppOpenAds;
using Coredian.Unity;

public class HCSDKManager : HostMonoBehaviour
{

    System.Action action;
    System.Action is_action;

    public static HCSDKManager INSTANCE;

    int TimeSeconds;

    int AD_WAIT_TIME = 60;
    public const string IS_LOAD_NAME = "level_ads";
    public const string RV_LOAD_NAME = "rv_video";

    public static bool RV_WATCHED = false;

    //public GameObject loacl_push_notification_obj;

    private void Awake()
    {
        if (INSTANCE == null) INSTANCE = this;
    }

    public GameObject dnd;

    private void Start()
    {
        InitializeHCSDK();
        InvokeRepeating(nameof(UpdateTimes), 1, 1);
    }

    private void OnEnable()
    {
        HCSDK.InterstitialCloseEvent += HCSDK_InterstitialCloseEvent;
        HCSDK.InterstitialDisplayEvent += HCSDK_InterstitialDisplayEvent;
        HCSDK.VideoSuccessEvent += HCSDK_VideoSuccessEvent;
        HCSDK.VideoDisplayEvent += HCSDK_VideoDisplayEvent;
        HCSDK.VideoCancelEvent += HCSDK_VideoCancelEvent;
        ApplicationFocusEvent += ApplicationFocus;
    }


    private void OnDisable()
    {
        HCSDK.InterstitialCloseEvent -= HCSDK_InterstitialCloseEvent;
        HCSDK.InterstitialDisplayEvent -= HCSDK_InterstitialDisplayEvent;
        HCSDK.VideoSuccessEvent -= HCSDK_VideoSuccessEvent;
        HCSDK.VideoDisplayEvent -= HCSDK_VideoDisplayEvent;
        HCSDK.VideoCancelEvent -= HCSDK_VideoCancelEvent;
        ApplicationFocusEvent -= ApplicationFocus;
    }

    void UpdateTimes()
    {
        TimeSeconds++;
        //        Debug.Log("TimeSeconds: " + TimeSeconds);
    }
    public static bool rewardedAdRunning = false;
    #region disableBGMusic
    private void HCSDK_VideoDisplayEvent()
    {
        EnableDisabeBGSound(false);
        rewardedAdRunning = true;

    }

    private void HCSDK_InterstitialDisplayEvent()
    {
        EnableDisabeBGSound(false);
    }

    void EnableDisabeBGSound(bool isEnable)
    {
        if (!isEnable)
        {
            //TODO : Sound Stop
        }
        else
        {
            //TODO : Sound Active
        }

    }
    #endregion disableBGMusic

    #region Ads
    private void HCSDK_VideoSuccessEvent()
    {
        TimeSeconds = 0;
        rewardedAdRunning = false;
        EnableDisabeBGSound(true);
        AdWatched();
        Event_AdWatched("rv_watched");
        RV_WATCHED = true;
        action.Invoke();
        if (!string.IsNullOrEmpty(_placeMentRV))
            LogEvent("rv_" + _placeMentRV);
        _placeMentRV = "";

    }

    private void HCSDK_VideoCancelEvent()
    {
        TimeSeconds = 0;
        rewardedAdRunning = false;
        EnableDisabeBGSound(true);
        RV_WATCHED = true;
    }

    private void HCSDK_InterstitialCloseEvent()
    {
        TimeSeconds = 0;
        EnableDisabeBGSound(true);
        AdWatched();
        Event_AdWatched("is_watched");
        is_action?.Invoke();
        if (!string.IsNullOrEmpty(_placeMentInter))
            LogEvent("interstitial_" + _placeMentInter);
        _placeMentInter = "";
    }

    string _placeMentRV;
    string _placeMentInter;

    public string AppInstanceId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public bool IsInitialized => throw new System.NotImplementedException();

    private void ShowRewardAds(string placement, System.Action onComplete)
    {

#if UNITY_EDITOR
        onComplete?.Invoke();
        //HCSDK_VideoSuccessEvent();
        return;
#endif


        if (HCSDK.IsRewardedReady(placement))
        {
            Debug.Log("Ad Dispalyed");
            _placeMentRV = placement;
            action = onComplete;
            HCSDK.ShowRewarded(placement);
        }
        else
        {

            if (!GeneralManager.CheckInternetConnection())
            {
                Debug.Log("No Internet Connected");
            }
            else
            {
                Debug.Log("No Ads!");
            }
            //TODO: Display message ad not availed
            // TODO UI             GameManager.Instance?._uiManager.NoAdsTextAnim();
            //LoadPanel.Instance.NotifyMsg("Ads not available");
            //LoadPanel.Instance.NotifyMsg(LocalizationManager.Instance.GetText("General_61"));
        }
    }

    public bool IsInterstitialReady(string placement)
    {
        return HCSDK.IsInterstitialReady(placement);
    }

    public bool IsInterstitialReady()
    {
#if UNITY_EDITOR
        return true;
#endif
        return HCSDK.IsInterstitialReady(IS_LOAD_NAME);
    }

    public bool isRewardedVideoAvailable()
    {

        return HCSDK.IsRewardedReady(RV_LOAD_NAME);
    }

    public void OpenApp()
    {

    }

    bool IsInterstitalAdTimeMatch()
    {
        if (TimeSeconds <= AD_WAIT_TIME)
        {
            return false;
        }
        return true;
    }

    //public void ShowInterstitial(int _levelIndex)
    //{
    //    if (_levelIndex >= interstitialDisplayLevel.Count)
    //    {
    //        Debug.LogError("Ads  => Ad Display :: Above 15 Levels => Current Level " + _levelIndex);
    //        ShowInterstitialAd(IS_LOAD_NAME);
    //        return;
    //    }

    //    Debug.LogError("Ads => Ad Display :: Index Value: " + interstitialDisplayLevel[_levelIndex] + " Index : " + _levelIndex);

    //    if (interstitialDisplayLevel[_levelIndex] == 1)
    //    {
    //        Debug.LogError("Ads => Ad Display :: Levels => Current Level " + _levelIndex);
    //        ShowInterstitialAd(IS_LOAD_NAME);
    //    }
    //}

    public void ShowInterstitialAd(string placement, System.Action onComplete)// == levelComplete
    {
        if (PlayerPrefs.GetInt("NoAds").Equals(1))
        {
            Debug.Log("Remove Ads Activated => Banner");
            return;
        }
#if UNITY_IOS
        if (ios_in_reviewValue)
        {
            Debug.Log("iOS in review: " + ios_in_reviewValue);
            onComplete?.Invoke();
            return;
        }
#endif

        if (rewardedAdRunning)
        {
            Debug.Log("Rewarded Ad already running");
            return;
        }

        //Return if RV Ad Already watched
        if (!IsInterstitalAdTimeMatch())
        {
            Debug.LogError("Ad Wait time not completed : " + TimeSeconds);
            return;
        }

#if UNITY_EDITOR
        onComplete?.Invoke();
        return;
#endif

        if (HCSDK.IsInterstitialReady(placement))
        {
            _placeMentInter = placement;
            is_action = onComplete;
            HCSDK.ShowInterstitial(placement);
        }
        else HCSDK.StartCachingInterstitial();
        Debug.Log("MiniGame Interstitial : " + placement);
    }

    public void ShowBanner(bool showBanner = true)
    {
        //No ADS Purchased
        if (PlayerPrefs.GetInt("NoAds").Equals(1))
        {
            Debug.Log("Remove Ads Activated => Banner");
            return;
        }

#if UNITY_IOS
        if (ios_in_reviewValue)
        {
            Debug.Log("iOS in review: " + ios_in_reviewValue);
            return;
        }
#endif
        if (showBanner)
        {
            HCSDK.ShowBanner();
            //Show Background
        }
        else
        {
            HCSDK.HideBanner();
            //Disable Background
        }
    }

    public void HideBanner()
    {
        HCSDK.HideBanner();
    }

    //int currentLevel = 0;
    //public void DisplayRV()
    //{
    //    ShowRewardAds("rv_replay", () =>
    //    {
    //        Debug.Log("On RV Complete: " + " rv_replay");
    //    });
    //}

    public void DisplayRV(string placement, Action onComplete)
    {
        Debug.Log("RV Placement : " + placement);
        ShowRewardAds(placement, onComplete);
    }
    #endregion

    #region Remote Config Setup
    private bool isFirebaseInitialized = false;

    //const string InterstitialDisplayLevelKey = "interstitial_display_level_complete";
    //public List<int> InterstitialDisplayLevel_Default;
    //public List<int> interstitialDisplayLevel;
    //public string levelData_Default;

    const string Is_Is_Rv_is_delay_secsKey = "is_is_rv_is_delay_secs";
    const int Is_Is_Rv_is_delay_secs_Default = 60;
    public int is_is_rv_is_delay_secsValue;

    const string local_push_notification_activeKey = "local_push_notification_active";
    const bool local_push_notification_active_Default = true;
    public bool local_push_notification_activeValue;

    const string ios_in_review_Key = "ios_in_review";
    const bool ios_in_review_Default = false;
    public bool ios_in_reviewValue;

    const string interstitialAd_timer_key = "interstitial_frequency";
    const int interstitialAd_timer_Default = 600;
    public int interstitialAd_timer;

    const string multiple_Interstitial_frequency_key = "multiple_Interstitial_frequency";
    public List<int> multiple_Interstitial_frequency_Default;
    public List<int> multiple_Interstitial_frequency;
    string multiple_Interstitial_frequency_Default_data;

    const string LocalNotificationIntervalTimeHoursKey = "local_notification_interval_time_hours"; //"vip_offer_display_level_interval"
    const int LocalNotificationIntervalTimeHoursValue_Default = 16;
    public int localNotificationIntervalTimeHoursValue;
    public IReadOnlyDictionary<string, string> configDictionary;

    public void InitializeHCSDK()
    {
        //if (Core.IsInitialized)
        //{
        //    Core.GetService<IFirebaseRemoteConfigService>().FetchRemoteConfig(OnFetchSuccess, OnFetchFail);
        //}
        //else
        //{
        //    Core.InitializationSucceeded += () =>
        //        Core.GetService<IFirebaseRemoteConfigService>().FetchRemoteConfig(OnFetchSuccess, OnFetchFail);
        //}
        HCSDK.StartHCSDK();
    }
    public void DisplayBannerAd()
    {
        ShowBanner();
    }
    //void OnMessageReceived(IDictionary<string, string> message)
    //{
    //    Debug.LogError("Message Received: " + message);
    //}

    //void OnTokenReceived(string token)
    //{

    //    string pushToken = Core.GetService<IFirebaseMessagingService>().PushToken;

    //    string first100Characters = pushToken.Substring(0, 100);
    //    string remainingCharacters = pushToken.Substring(100);

    //    //Debug.LogError("Push Token: " + pushToken);
    //    //Debug.LogError("First 100: " + first100Characters);
    //    //Debug.LogError("Rest: " + remainingCharacters);
    //    //Debug.LogError("Token: " + token);

    //    LogEvent("push_token", "push_token_0", first100Characters, "push_token_1", remainingCharacters);

    //}

    private void OnFetchFail()
    {
        Debug.Log("RC Fetch Failed...!!!");
        SetRemoteOrLocalValues(true);
    }

    private void OnFetchSuccess(IReadOnlyDictionary<string, string> config)
    {
        configDictionary = config;
        Debug.Log("RC Fetch Sucessfully");
        isFirebaseInitialized = true;
        SetRemoteOrLocalValues(false);
    }

    void SetRemoteOrLocalValues(bool isLocal)
    {
        Debug.Log("RC Fetch isLocal: " + isLocal);
        GetUniversalData(isLocal);
    }

    void GetUniversalData(bool isLocal)
    {
        is_is_rv_is_delay_secsValue = GetIntData(Is_Is_Rv_is_delay_secsKey, Is_Is_Rv_is_delay_secs_Default);
        Debug.Log("is_is_rv_is_delay_secs: " + is_is_rv_is_delay_secsValue);

        AD_WAIT_TIME = is_is_rv_is_delay_secsValue;
        Debug.Log("is_is_rv_is_delay_secs: => AD_WAIT_TIME: " + AD_WAIT_TIME);

        ios_in_reviewValue = GetBoolData(ios_in_review_Key, ios_in_review_Default);
        Debug.Log("ios_in_review: " + ios_in_reviewValue);

        //localNotificationIntervalTimeHoursValue = GetIntData(LocalNotificationIntervalTimeHoursKey, LocalNotificationIntervalTimeHoursValue_Default);
        //Debug.Log("Loading Scene with Local Values : " + isLocal);

        interstitialAd_timer = GetIntData(interstitialAd_timer_key, interstitialAd_timer_Default);
        Debug.Log("Interstital Ad Timer :" + interstitialAd_timer);

        //multiple_Interstitial_frequency_Default_data = ConvertStringListToString(multiple_Interstitial_frequency_Default);
        //string levelData = GetStringData(multiple_Interstitial_frequency_key, multiple_Interstitial_frequency_Default_data);
        //Debug.Log("Level Data : " + levelData);
        //GetUtilitiesValues(levelData);

    }

    string ConvertStringArrayToString(string[] array)
    {
        StringBuilder strinbuilder = new StringBuilder();
        foreach (string value in array)
        {
            strinbuilder.Append(value);
            strinbuilder.Append(',');
        }
        return strinbuilder.ToString();
    }


    string ConvertStringListToString(List<int> array)
    {
        //
        // Concatenate all the elements into a StringBuilder.
        //
        StringBuilder strinbuilder = new StringBuilder();
        for (int i = 0; i < array.Count; i++)
        {
            strinbuilder.Append(array[i]);
            if (i != (array.Count - 1))
                strinbuilder.Append(',');
        }
        return strinbuilder.ToString();
    }

    int GetIntData(string key, int defaultValue)
    {
        if (string.IsNullOrEmpty(GetValueForKey(key)))
        {
            return defaultValue;
        }
        else
        {
            try
            {
                return int.Parse(GetValueForKey(key));
            }
            catch (Exception e)
            {
                Console.WriteLine("Parse Error: " + e.Message);
                return defaultValue;
            }
        }
    }

    bool GetBoolData(string key, bool defaultValue)
    {
        if (string.IsNullOrEmpty(GetValueForKey(key)))
        {
            return defaultValue;
        }
        else
        {
            try
            {
                return bool.Parse(GetValueForKey(key));
            }
            catch (Exception e)
            {
                Console.WriteLine("Parse Error: " + e.Message);
                return defaultValue;
            }
        }
    }

    string GetStringData(string key, string defaultValue)
    {
        if (string.IsNullOrEmpty(GetValueForKey(key)))
        {
            return defaultValue;
        }
        else
        {
            try
            {
                return GetValueForKey(key);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parse Error: " + e.Message);
                return defaultValue;
            }
        }
    }

    string GetValueForKey(string key)
    {
        if (!isFirebaseInitialized) return "";

        configDictionary.TryGetValue(key, out var value);
        return value;
    }

    //List<int> GetUtilitiesValues(string _rcValue)
    //{
    //    //List<int> utilitiesValues = new List<int>();
    //    interstitialDisplayLevel= new List<int>();
    //    string[] costsArray = _rcValue.Split(char.Parse(","));

    //    foreach (var t in costsArray)
    //    {
    //        utilitiesValues.Add(Int32.Parse(t));
    //    }
    //    return utilitiesValues;
    //}

    List<int> GetUtilitiesValues(string _rcValue)
    {
        //List<int> utilitiesValues = new List<int>();
        multiple_Interstitial_frequency = new List<int>();
        string[] costsArray = _rcValue.Split(char.Parse(","));

        foreach (var t in costsArray)
        {
            multiple_Interstitial_frequency.Add(Int32.Parse(t));
        }
        return multiple_Interstitial_frequency;
    }

    #endregion

    #region Offer Panel
    /*
    public string vipOfferPlacement = "";
    public int offerPopupCounter = 0;

    //[ButtonInspector]
    void PurchaseTailorOffer()
    {
        OnTailorsOfferPurchasedSuccessful(false);
    }

    [ButtonInspector]
    public void OpenOffer()
    {
        Debug.Log("Counter : " + offerPopupCounter + " vip_offer_display_level_interval -- " + vipOfferDisplayLevelIntervalValue);
        if (offerPopupCounter % vipOfferDisplayLevelIntervalValue == 0)
            Invoke("OpenOfferEverytime", 0.2f);
        offerPopupCounter++;
    }

    public void OpenOffer(int _levelCount)
    {
        Debug.Log("Open Offer Level Completed : " + _levelCount);
        if (IS_TAILOR_OFFER == 1)
        {
            Debug.Log("Offer Purchased");
            return;
        }
        if (_levelCount % 7 == 0)
        {
            Invoke("OpenOfferEverytime", 0.2f);
        }
    }

    void OpenOfferEverytime()
    {
        LoadPanel.Instance.LoadPanelByName(Generic.Constants.Panel_offerPanel, LoadPanel.panelParentType.top);
    }
    */

    //    public void OpenRatePopup(int _levelCount)
    //    {
    //        Debug.Log("Rate Popup Level Completed : " + _levelCount);
    //        //if (!PlayerPrefs.HasKey("RateClick") && _levelCount % 9 == 0)
    //        int level = 16;
    //#if UNITY_EDITOR
    //level = 16;
    //#endif

    //        if (PlayerPrefs.GetInt("RateClick", 0) == 0 && _levelCount % level == 0)
    //            Invoke(nameof(ShowRatePopup), 3);
    //        //ThreadSortingManager.Instance.loadPanel.LoadPanelByName(Generic.Constants.Panel_RateUs, LoadPanel.panelParentType.top);
    //    }

    //    void ShowRatePopup()
    //    {
    //        ThreadSortingManager.Instance.loadPanel.LoadPanelByName(Generic.Constants.Panel_RateUs, LoadPanel.panelParentType.top);
    //    }

    //[ButtonInspector]
    //public void CancleOfferCountDown()
    //{
    //    CancelInvoke(nameof(UpdateOfferTimes));
    //}

    //[ButtonInspector]
    //public void StartOfferCountDown()
    //{
    //    offerDisplayTime = tailor_offer_interval_secsValue;
    //    if (IS_TAILOR_OFFER.Equals(1))
    //    {
    //        Debug.Log("Tailor Offer already purchased");
    //        return;
    //    }

    //    if (PlayerPrefs.GetInt(Generic.Constants.REMOVE_AD).Equals(1))
    //        InvokeRepeating(nameof(UpdateOfferTimes), 1, 1);
    //}

    public int offerDisplayTime = 0;

    //void UpdateOfferTimes()
    //{
    //    if (PlayerPrefs.GetInt(Generic.Constants.REMOVE_AD).Equals(0))
    //    {
    //        Debug.Log("Remove Ad is not purchased");
    //        return;
    //    }
    //    offerDisplayTime--;
    //    Debug.Log("offerDisplayTime: " + offerDisplayTime);
    //    if (offerDisplayTime <= 0)
    //    {
    //        offerDisplayTime = tailor_offer_interval_secsValue;
    //        OpenOfferEverytime();
    //    }
    //}
    #endregion

    #region Events
    public void LogEvent(string eventName)
    {
        //        Debug.LogError("EventName: " + eventName);

        Core.GetService<Coredian.Firebase.IFirebaseService>().LogEvent(eventName);
        //Core.GetService<Coredian.Facebook.IFacebookService>().LogEvent(eventName);

        GameAnalytics.NewDesignEvent(eventName);

        //FB.LogAppEvent(eventName);
    }

    public void LogEvent_Custom(string eventName)
    {
        Core.GetService<Coredian.Firebase.IFirebaseService>().LogEvent(eventName);
        Core.GetService<IAttributionService>().SendEvent(eventName);
        // Core.GetService<Coredian.Facebook.IFacebookService>().LogEvent(eventName);
        //GameAnalytics.NewDesignEvent(eventName);
    }

    //public void LogEvent(string eventName, string eventParameter1, string eventValue1)
    //{
    //    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
    //    keyValuePairs.Add(eventParameter1, eventValue1);
    //    Core.GetService<Coredian.Firebase.IFirebaseService>().LogEvent(eventName, keyValuePairs);
    //    Core.GetService<Coredian.Facebook.IFacebookService>().LogEvent(eventName,0,keyValuePairs);

    //    //FB.LogAppEvent(eventName, null, keyValuePairs);

    //    GameAnalytics.NewDesignEvent(eventName, keyValuePairs);
    //}

    //public void LogEvent(string eventName, string eventParameter1, string eventValue1, string eventParameter2, string eventValue2)
    //{
    //    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
    //    keyValuePairs.Add(eventParameter1, eventValue1);
    //    keyValuePairs.Add(eventParameter2, eventValue2);
    //    Core.GetService<Coredian.Firebase.IFirebaseService>().LogEvent(eventName, keyValuePairs);
    //    Core.GetService<Coredian.Facebook.IFacebookService>().LogEvent(eventName, 0, keyValuePairs);

    //    //FB.LogAppEvent(eventName, null, keyValuePairs);

    //    GameAnalytics.NewDesignEvent(eventName, keyValuePairs);
    //}

    //public void Event_StageLevelStart(string stage, string level)
    //{
    //    string eventName = "stage_start";
    //    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
    //    keyValuePairs.Add("stage", stage);
    //    keyValuePairs.Add("level", level);

    //    Core.GetService<Coredian.Firebase.IFirebaseService>().LogEvent(eventName, keyValuePairs);
    //    Core.GetService<Coredian.Facebook.IFacebookService>().LogEvent(eventName, 0, keyValuePairs);
    //    //FB.LogAppEvent(eventName, null, keyValuePairs);
    //}

    //public void Event_StageLevelEnd(string stage, string level)
    //{

    //    string eventName = "stage_end";
    //    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
    //    keyValuePairs.Add("stage", stage);
    //    keyValuePairs.Add("level", level);

    //    Core.GetService<Coredian.Firebase.IFirebaseService>().LogEvent(eventName, keyValuePairs);
    //    Core.GetService<Coredian.Facebook.IFacebookService>().LogEvent(eventName, 0, keyValuePairs);
    //    //FB.LogAppEvent(eventName, null, keyValuePairs);
    //}

    public void Event_AdWatched(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        Debug.Log("Ad Watched event: " + eventName);
        LogEvent_Custom(eventName);
    }

    int counter = 0;
    void AdWatched()
    {
        string eventName = "ads_watched";
        counter++;
        Debug.Log("Ad Watched Counter: " + counter);
        switch (counter)
        {
            case 10:
                eventName = "ads_watched_10";
                Event_AdWatched(eventName);
                break;
            case 20:
                eventName = "ads_watched_20";
                Event_AdWatched(eventName);
                break;
            case 30:
                eventName = "ads_watched_30";
                Event_AdWatched(eventName);
                break;
            case 50:
                eventName = "ads_watched_50";
                Event_AdWatched(eventName);
                break;

        }

        //EnableDisabeBGSound(true);
    }

    //Firebase and AF
    public void CustomEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;
        Debug.Log("Custom Event: " + eventName);
        LogEvent_Custom(eventName);
    }

    #endregion

    #region More Games Controller
    public void ShowMoreGames()
    {
        Debug.LogError("ShowMoreGames Start");
        var moreGames = Core.GetService<CrossPromoService>().GetAd(AdUnit.MoreGames);
        if (moreGames == null)
            return; // failed to load the ad at this point.
        moreGames.Shown += OnShown;
        moreGames.Hidden += OnHidden;
        moreGames.Show();
        Debug.LogError("ShowMoreGames Shown");
    }

    void OnShown()
    {
        Log.Info("More games shown");
    }

    void OnHidden()
    {
        Log.Info("More games hidden");
    }
    #endregion

    #region AppOpen Ads

    // when the splash screen is no longer visible disable app open ads
    private void OnSplashScreenHidden()
    {
        //No ADS Purchased
        if (PlayerPrefs.GetInt("NoAds").Equals(1))
        {
            Debug.Log("Remove Ads Activated => Banner");
            return;
        }

        //Debug.Log("Focus Splash Hidden");
        //var appOpenAdsService = Core.GetService<IAppOpenAdsService>();
        // appOpenAdsService.DisableAds(false);
        //appOpenAdsService.ShowAdIfConditionsAreMet();
    }

    // void OnGUI()
    // {
    //     if (GUI.Button(new Rect(100, 50, 150, 100), "Open App Ads"))
    //     {
    //         print("You clicked the button!");
    //         var appOpenAdsService = Core.GetService<IAppOpenAdsService>();
    //         appOpenAdsService.DisableAds(false);
    //         appOpenAdsService.ShowAdIfConditionsAreMet();
    //     }
    // }

    // when the app is going into background enable app open ads
    private void ApplicationFocus(bool focus)
    {
        //No ADS Purchased
        if (PlayerPrefs.GetInt("NoAds").Equals(1))
        {
            Debug.Log("Remove Ads Activated => Banner");
            return;
        }

        //Debug.LogError("Focus : " + focus);
        //if (!focus) return;
        //var appOpenAdsService = Core.GetService<IAppOpenAdsService>();
        //appOpenAdsService.DisableAds(false);
        //appOpenAdsService.ShowAdIfConditionsAreMet();
    }

    #endregion
}
