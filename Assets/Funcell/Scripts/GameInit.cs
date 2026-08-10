using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInit : MonoBehaviour
{
    public static GameInit instance;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return 0;
        instance = this;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;

        yield return new WaitForSeconds(1);

        InitGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    AsyncOperation asyncOperation;
    public Image progressSlider;

    void InitGame()
    {
        GameStart();
    }

    void GameStart()
    {
#if UNITY_EDITOR
        VersionText();
#endif
        int sceneId = 1;
        asyncOperation = SceneManager.LoadSceneAsync(sceneId);
    }

    void OnGUI()
    {
        if (progressSlider == null)
            return;

        if (asyncOperation == null)
        {
            float progress = Time.unscaledDeltaTime * 0.001f + progressSlider.fillAmount;
            progressSlider.fillAmount = Mathf.Min(0.4f, progress);
            return;
        }

        if (progressSlider != null)
        {
            progressSlider.fillAmount =
                Mathf.Clamp(Mathf.Max(asyncOperation.progress, progressSlider.fillAmount), 0, 0.8f);
        }
    }

    void VersionText()
    {
        Debug.Log("Funcell SDK Verison : " + Funcell.Games.FuncellSDKVersion.Version);
    }

    void ClearAll()
    {
        PlayerPrefs.DeleteAll();
    }
}
