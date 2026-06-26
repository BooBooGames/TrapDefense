using System;
using Balaso;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example MonoBehaviour class requesting iOS Tracking Authorization
/// </summary>
public class AppTrackingTransparencyManager : MonoBehaviour
{
    protected static AppTrackingTransparencyManager Instance;
#if UNITY_IOS
    public static ATTResult result { get; private set; }
#endif

    public UnityEvent onGameReadyToInit;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void Start()
    {

#if UNITY_IOS
        var currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;
        result = new ATTResult()
        {
            authorizationStatus = AppTrackingTransparency.TrackingAuthorizationStatus,
            isUpdated = false
        };

        AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
        Debug.Log("Request AppTrackingTransparency");



        AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
        Debug.Log(string.Format("Current authorization status: {0}", currentStatus.ToString()));

        if (currentStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
        {
            Debug.Log("Requesting authorization...");
            AppTrackingTransparency.RequestTrackingAuthorization();
            result.isUpdated = false;
        }
        else
        {
            result.isUpdated = true;
            result.authorizationStatus = currentStatus;
            MoveToGameInit();
        }
#else
        MoveToGameInit();
#endif

    }

#if UNITY_IOS

    /// <summary>
    /// Callback invoked with the user's decision
    /// </summary>
    /// <param name="status"></param>
    private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status)
    {

        result.isUpdated = true;
        result.authorizationStatus = status;

        switch (status)
        {
            case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                Debug.Log("AuthorizationStatus: NOT_DETERMINED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                Debug.Log("AuthorizationStatus: RESTRICTED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.DENIED:
                Debug.Log("AuthorizationStatus: DENIED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                Debug.Log("AuthorizationStatus: AUTHORIZED");
                break;
        }

        // Obtain IDFA
        Debug.Log(string.Format("IDFA: {0}", AppTrackingTransparency.IdentifierForAdvertising()));

        // START THE GAME FROM HERE
        MoveToGameInit();
    }
#endif

    void MoveToGameInit()
    {
        if (onGameReadyToInit != null)
            functionMustCallFromMainThread = new Action(onGameReadyToInit.Invoke);
    }

    /// <summary>
    /// Issue : Game fail to run when we call Unity function from other thread, Ex : Scene loading, Audio play etc.
    /// </summary>
    Action functionMustCallFromMainThread = null;

    void Update()
    {
        if (functionMustCallFromMainThread != null)
        {
            functionMustCallFromMainThread.Invoke();
            functionMustCallFromMainThread = null;
        }
    }
}
