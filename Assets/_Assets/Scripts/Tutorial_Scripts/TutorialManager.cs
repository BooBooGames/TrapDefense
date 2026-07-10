using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private TutorialData[] _tutorialDataList;

    public static event Action<TutorialType> OnTutorialCompleted;
    public static event Action<TutorialEventType> OnEventOccurred;

    public event Action<bool> OnInputStatusChanged;
    public event Action<bool> OnGamePauseStatusChanged;

    private TutorialStepData _currentTutorialStep;
    private Queue<TutorialData> _pendingTutorialsQueue;

    private TutorialData _ongoingTutorial;
    private bool _areTutorialsRegistered;

    private void Awake()
    {
        Instance = this;
        _pendingTutorialsQueue = new Queue<TutorialData>();
    }

    private void Start()
    {
        if (PlayerPrefsExtension.GetBool(PlayerPrefsKeys.IS_WELCOMED))
        {
            RegisterAllTutorials();
        }

        WelcomeScreen.OnContinue += RegisterAllTutorials;

        //UserInput.OnClicked += UserInput_OnClicked;

        RegisterAllTutorials();
    }

    private void UserInput_OnClicked()
    {
        if(_currentTutorialStep != null)
        {
            foreach(var endConstraint in _currentTutorialStep.EndConstraints)
            {
                if(endConstraint is ClickConstraint timerConstraint)
                {
                    timerConstraint.OnEnd();
                }
            }
        }
    }

    private void RegisterAllTutorials()
    {
        if (_areTutorialsRegistered) return;

        _areTutorialsRegistered = true;
        foreach (TutorialData tutorialData in _tutorialDataList)
        {
            RegisterTutorial(tutorialData);
        }
    }

    private void RegisterTutorial(TutorialData pTutorial)
    {
        pTutorial.StartTrigger.Initialize(() =>
        {
            StartCoroutine(RunTutorial(pTutorial));
        });
    }

    private IEnumerator RunTutorial(TutorialData pTutorial)
    {
        //Debug.Log($"Start Tutorial {tutorial._TutorialType}");

        if (IsTutorialDone(pTutorial._TutorialType))
        {
            OnTutorialCompleted?.Invoke(pTutorial._TutorialType);
            yield break;
        }

        if(_ongoingTutorial != null)
        {
            _pendingTutorialsQueue.Enqueue(pTutorial);
            yield break;
        }

        _ongoingTutorial = pTutorial;
        foreach (var step in pTutorial.TutorialSteps)
        {
            yield return RunStep(step);

            if (step.WaitTimeAfterStepEnd > 0f)
            {
                yield return new WaitForSeconds(step.WaitTimeAfterStepEnd);
            }
        }

        //Debug.Log($"Tutorial {tutorial._TutorialType} Done");
        SetTutorialDone(pTutorial._TutorialType);
        _ongoingTutorial = null;

        if(_pendingTutorialsQueue.Count > 0)
        {
            TutorialData pendingTutorial = _pendingTutorialsQueue.Dequeue();

            StartCoroutine(RunTutorial(pendingTutorial));
        }

        OnTutorialCompleted?.Invoke(pTutorial._TutorialType);
    }

    private void HandleSpecialActions(TutorialStepStartAction pTutorialStepStartAction)
    {
        if(pTutorialStepStartAction == TutorialStepStartAction.TurnInputOn)
        {
            OnInputStatusChanged?.Invoke(true);
        }
        else if (pTutorialStepStartAction == TutorialStepStartAction.TurnInputOff)
        {
            OnInputStatusChanged?.Invoke(false);
        }
        else if (pTutorialStepStartAction == TutorialStepStartAction.UnpauseGameplay)
        {
            OnGamePauseStatusChanged?.Invoke(false);
        }
        else if (pTutorialStepStartAction == TutorialStepStartAction.PauseGameplay)
        {
            OnGamePauseStatusChanged?.Invoke(true);
        }
    }

    private void HandleSpecialEvent(TutorialEventType pTutorialEventType)
    {
        _currentTutorialStep?.OnEventOccured(pTutorialEventType);

        OnEventOccurred?.Invoke(pTutorialEventType);
    }

    private IEnumerator RunStep(TutorialStepData pTutorialStep)
    {
        bool completed = false;

        foreach(var startAction in pTutorialStep.OnStepStartActions)
        {
            startAction.Execute();

            if(startAction is SpecialTutorialAction specialTutorialAction)
            {
                HandleSpecialActions(specialTutorialAction._TutorialStepStartAction);
            }
        }

        foreach(TutorialConstraint endConstraint in pTutorialStep.EndConstraints)
        {
            endConstraint.Begin(onComplete: () =>
            {
                completed = true;
            });
        }
        
        _currentTutorialStep = pTutorialStep;
        
        yield return new WaitUntil(() => completed);

        _currentTutorialStep = null;

        foreach (TutorialConstraint endConstraint in pTutorialStep.EndConstraints)
        {
            endConstraint.End();
        }

        foreach (var endAction in pTutorialStep.OnStepEndActions)
        {
            endAction.Execute();

            if (endAction is SpecialTutorialEndAction specialTutorialAction)
            {
                HandleSpecialActions(specialTutorialAction._TutorialStepStartAction);
            }
        }
    }

    private void OnDestroy()
    {
        WelcomeScreen.OnContinue -= RegisterAllTutorials;

        //UserInput.OnClicked -= UserInput_OnClicked;

        OnTutorialCompleted = null;
        OnEventOccurred = null;
    }

    public bool IsTutorialDone(TutorialType tutorialType)
    {
        return PlayerPrefsExtension.GetBool(PlayerPrefsKeys.GetTutorialDoneKey(tutorialType.ToString()));
    }

    public void SetTutorialDone(TutorialType tutorialType)
    {
        PlayerPrefsExtension.SetBool(PlayerPrefsKeys.GetTutorialDoneKey(tutorialType.ToString()), true);
    }

    private void OnTutorialEventOccurred(TutorialEventType tutorialEventType)
    {
        OnEventOccurred?.Invoke(tutorialEventType);
    }

    public void OnPlayingForFirstTime()
    {
        OnTutorialEventOccurred(TutorialEventType.StartingGameForFirstTime);
    }

    public void OnDraggedWeaponForSomeTime()
    {
        HandleSpecialEvent(TutorialEventType.DraggedWeapon);
    }

    public bool IsAnyTutorialRunning() => _ongoingTutorial;

    public static bool IsThisTutorialRunning(TutorialType pTutorialType)
    {
        return Instance._ongoingTutorial != null ? Instance._ongoingTutorial._TutorialType == pTutorialType : false;
    }
}