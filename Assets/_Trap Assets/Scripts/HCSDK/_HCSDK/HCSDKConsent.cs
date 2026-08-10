using System.Collections;
using System.Collections.Generic;
using Coredian.Privacy;
using UnityEngine;
using Coredian.Privacy.Configuration;

public class HCSDKConsent : MonoBehaviour
{
    public GameObject initMainGameObject;

    private void Awake()
    {

        //if (Core.GetService<ConsentService>().GetConsentStatus(ConsentType.PrivacyPolicy) == ConsentStatus.Granted)
        //{
        //    Debug.Log("Consent Status: " + Core.GetService<ConsentService>().GetConsentStatus(ConsentType.PrivacyPolicy));
        //    StartScene();
        //}
        //else
        //{
        //    Core.GetService<ConsentService>().ConsentStatusChanged += OnConsentStatusChanged;
        //}
        StartScene();
    }

    //private void OnConsentStatusChanged(ConsentType consentType, ConsentStatus consentStatus)
    //{
    //    Debug.Log("Consent =>  Type: " + consentType + " Status : " + consentStatus );
    //    if (consentStatus == ConsentStatus.Granted && consentType == ConsentType.PrivacyPolicy)
    //    {
    //        StartScene();
    //    }
    //}

    public void StartScene()
    {
        initMainGameObject.SetActive(true);
    }
}
