using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardScreenView : MonoBehaviour
{
    public Sprite commonCardSprite, rareCardSprite, epicCardSprite, legendaryCardSprite, lockCardSprite, commonLevelBgSprite, rareLevelBgSprite, epicLevelBgSprite, legendaryLevelBgSprite, lockLevelBgSprite, lockIconSprite;

    public Button summonx1Button, summonx10Button;
    [Serializable]
    public class UpgradeCardData
    {
        public Image bgImage, iconImage, levelBgImage;
        public TextMeshProUGUI levelText, countText;
    }
    public List<UpgradeCardData> upgradeCardDatas;
    public Button perksButton, heroButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}