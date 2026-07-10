using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialActionsHandler : MonoBehaviour
{
    private static TutorialActionsHandler Instance;

    [SerializeField] private SerializedDictionary<TutorialPopupType, GameObject> _tutorialPopupsDictionary;
    [SerializeField] private SerializedDictionary<HudObjectType, GameObject> _hudTutorialObjectsDictionary;

    [SerializeField] private List<GameObject> _hudItemsList;

    private Dictionary<GameObject, bool> _objectActiveStatusDictionary;

    private void Awake()
    {
        Instance = this;

        _objectActiveStatusDictionary = new();
    }

    private void Start()
    {
        foreach(var go in _hudItemsList)
        {
            _objectActiveStatusDictionary.Add(go, go.activeSelf);
        }
    }

    private void OnDestroy()
    {
    }

    public static void ShowTutorialHudItems(HudObjectType[]  pHudButtonTypeItems)
    {
        foreach (var go in Instance._hudItemsList)
        {
            go.SetActive(false);
        }

        foreach (var hudButtonType in pHudButtonTypeItems)
        {
            if (hudButtonType == HudObjectType.None) continue;

            GameObject targetObject = Instance._hudTutorialObjectsDictionary[hudButtonType];

            if (targetObject == null) continue;

            targetObject.SetActive(true);
        }
    }

    public static void HideTutorialHudItems(HudObjectType[] pHudButtonTypeItems)
    {
        foreach (var go in Instance._hudItemsList)
        {
            go.SetActive(false);
        }

        foreach (var hudButtonType in pHudButtonTypeItems)
        {
            if (hudButtonType == HudObjectType.None) continue;

            GameObject targetObject = Instance._hudTutorialObjectsDictionary[hudButtonType];

            if (targetObject == null) continue;

            targetObject.SetActive(false);
        }
    }

    public static void ShowAllHudItems(HudObjectType pHudButtonType)
    {
        foreach (var pair in Instance._objectActiveStatusDictionary)
        {
            pair.Key.SetActive(true);
        }
    }

    public static void ShowTutorialPopup(TutorialPopupType pTutorialPopupType)
    {
        GameObject popup = Instance._tutorialPopupsDictionary[pTutorialPopupType];

        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    public static void HideTutorialPopup(TutorialPopupType pTutorialPopupType)
    {
        GameObject popup = Instance._tutorialPopupsDictionary[pTutorialPopupType];

        if (popup != null)
        {
            popup.SetActive(false);
        }
    }
}
