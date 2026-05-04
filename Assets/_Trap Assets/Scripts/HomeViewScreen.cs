using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeViewScreen : MonoBehaviour
{
    public Button playButton;
    public List<ChestInfo> chestInfos;

    void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(() =>
            {
                UIManager.Instance.StartGame();
            });
        }
    }

}
