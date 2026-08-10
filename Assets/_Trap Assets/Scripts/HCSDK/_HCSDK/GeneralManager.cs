using UnityEngine;

public class GeneralManager : MonoBehaviour
{
    public static bool CheckInternetConnection()
    {

        bool isConnectedToInternet = false;

#if UNITY_IPHONE
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
            Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            isConnectedToInternet = true;
        }
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            isConnectedToInternet = true;
        }
#endif

#if UNITY_ANDROID
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            isConnectedToInternet = true;
        }
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            isConnectedToInternet = true;
        }
#endif
        return isConnectedToInternet;
    }
}
