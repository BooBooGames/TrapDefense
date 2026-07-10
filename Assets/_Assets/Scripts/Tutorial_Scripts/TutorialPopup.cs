using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class TutorialPopup : MonoBehaviour
{
    [SerializeField] private GameObject _arrowObject;
    [SerializeField] private GameObject _maskedObject;

    [SerializeField] private List<GameObject> _allArrows;
    [SerializeField] private List<GameObject> _allMaskObjects;

    private void Awake()
    {
        if (_arrowObject != null)
        {
            _allArrows.Add(_arrowObject);
        }

        if (_maskedObject != null)
        {
            _allMaskObjects.Add(_maskedObject);
        }
    }

    private void OnEnable()
    {
        ToggleArrowsAndMasks(true);   
    }

    private void ToggleArrowsAndMasks(bool pActivate)
    {
        foreach (GameObject arrow in _allArrows)
        {
            arrow.SetActive(pActivate);
        }

        foreach(GameObject mask in _allMaskObjects)
        {
            mask.SetActive(pActivate);
        }
    }

    private void OnDisable()
    {
        ToggleArrowsAndMasks(false);
    }
}