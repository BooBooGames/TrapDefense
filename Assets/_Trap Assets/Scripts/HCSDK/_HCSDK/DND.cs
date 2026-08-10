using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DND : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Input.touchCount > 0)
    //    {
    //        if (Input.touchCount.Equals(3)) //Enable UI
    //        {
    //            Debug.Log("Enable UI");
    //            UIEnable(true);
    //        }

    //        if (Input.touchCount.Equals(4)) // Disable UI
    //        {
    //            Debug.Log("Disable UI");
    //            UIEnable(false);
    //            //if (GameObject.Find("Canvas_UI"))
    //            //{
    //            //    GameObject.Find("Canvas_UI").gameObject.SetActive(false);
    //            //}

    //            //if (GameObject.Find("Canvas_forSpeechBubble"))
    //            //{
    //            //    GameObject.Find("Canvas_forSpeechBubble").gameObject.SetActive(false);
    //            //}
    //        }
    //    }
    //}

    //void UIEnable(bool isEnable)
    //{
    //    //if (GameObject.Find("Canvas_UI"))
    //    //{
    //    //    GameObject.Find("Canvas_UI").gameObject.SetActive(isEnable);
    //    //}

    //    //if (GameObject.Find("Canvas_forSpeechBubble"))
    //    //{
    //    //    GameObject.Find("Canvas_forSpeechBubble").gameObject.SetActive(isEnable);
    //    //}

    //    if (GameObject.FindObjectOfType<DebugScript>())
    //    {
    //        DebugScript debugScript = GameObject.FindObjectOfType<DebugScript>();
    //        for (int i = 0; i < debugScript.objects.Length; i++)
    //        {
    //            debugScript.objects[i].SetActive(isEnable);
    //        }
    //    }
    //}

    //[ButtonInspector]
    void DeleteAllPlayerPrefs()
    {
        Debug.Log("All Player Prefs deleted");
        PlayerPrefs.DeleteAll();
    }
}
