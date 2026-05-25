using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class FTUEController : MonoBehaviour
{
    private const string CompletionSaveKey = "FTUECompleted";
    private const int TotalSteps = 5;

    public List<Sprite> allIcons;
    public List<string> allDescriptions;
    public Image iconImage;
    public TextMeshProUGUI descriptionText;
    public Button continueButton;

    private int currentStepIndex;
    private Action completedCallback;

    public static bool IsCompleted => PlayerPrefs.GetInt(CompletionSaveKey, 0) == 1;

    private void Awake()
    {
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(HandleContinueClicked);
    }

    public void Begin(Action onCompleted)
    {
        completedCallback = onCompleted;
        currentStepIndex = 0;
        ShowCurrentStep();
    }

    private void HandleContinueClicked()
    {
        currentStepIndex++;

        if (currentStepIndex >= TotalSteps)
        {
            PlayerPrefs.SetInt(CompletionSaveKey, 1);
            PlayerPrefs.Save();
            completedCallback.Invoke();
            return;
        }

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        iconImage.sprite = allIcons[currentStepIndex];
        descriptionText.text = allDescriptions[currentStepIndex];
    }
}
