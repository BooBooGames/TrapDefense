using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RvUI : MonoBehaviour
{
    [SerializeField] private Button _useButton;
    [SerializeField] private ModularProgressBar _durationBar;
    [SerializeField] private Image _rvIcon;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _rvText;
    [SerializeField] private TextMeshProUGUI _rvEffectDescriptionText;
    [SerializeField] private GameObject _rvEffectNotActiveObject;

    private RvRequestSO _rvRequest;
    private float _timer;
    private Coroutine _timerCoroutine;

    private bool _isAvailable = true;
    private int _instantGearsReward = 0;

    private Action<RvUI, RvRequestSO> OnComplete;
    private Action<RvUI, RvRequestSO> OnExpire;

    private Action<RvUI, RvRequestSO> OnStartEffect;

    private void OnEnable()
    {
        _durationBar.gameObject.SetActive(false);
    }

    private void Start()
    {
        _useButton.onClick.AddListener(HandleOnUse);
    }

    private void OnToggleEffect(bool pActive)
    {
        _rvEffectNotActiveObject.SetActive(!pActive);
        _durationBar.gameObject.SetActive(pActive);

        _useButton.interactable = !pActive;
    }

    private IEnumerator TimerRoutine(float pTotalDuration, Action pOnDurationComplete)
    {
        _timer = pTotalDuration;

        while (_timer > 0)
        {
            _durationBar.SetProgress(_timer / pTotalDuration);
            _timer -= Time.deltaTime;
            yield return null;
        }

        pOnDurationComplete?.Invoke();
    }

    private void HandleOnUse()
    {
        string eventName = _rvRequest._RvEventName;

        if (HCSDKManager.INSTANCE == null)
        {
            OnAdFinished();
            return;
        }

        HCSDKManager.INSTANCE.DisplayRV(eventName, () =>
        {
            AnalyticsManager.ShowRVEvent(eventName);
            OnAdFinished();
        });
    }

    private void OnAdFinished()
    {
        OnStartEffect?.Invoke(this, _rvRequest);

        OnToggleEffect(true);

        if(_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }

        if (_rvRequest._HasTimedEffect)
        {
            _timerCoroutine = StartCoroutine(TimerRoutine(_rvRequest._EffectDuration, OnRvEffectComplete));
        }
        else
        {
            Hide();
        }
    }

    private void OnRvEffectComplete()
    {
        OnComplete?.Invoke(this, _rvRequest);
        Hide();
    }

    private void Hide()
    {
        _isAvailable = true;
        OnExpire?.Invoke(this, _rvRequest);

        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    public void ForceExpire()
    {
        Hide();
    }

    public void Show(Action<RvUI, RvRequestSO> pOnStartEffect, RvRequestSO pRvRequest, Action<RvUI, RvRequestSO> pOnComplete, Action<RvUI, RvRequestSO> pOnExpire)
    {
        OnStartEffect = pOnStartEffect;
        this._rvRequest = pRvRequest;
        this.OnComplete = pOnComplete;
        this.OnExpire = pOnExpire;

        _timer = pRvRequest._DisplayDuration;

        _durationBar.SetProgress(1f);
        _rvIcon.sprite = pRvRequest._RvIcon;
        _rvText.text = pRvRequest._RvDisplayName;

        OnToggleEffect(false);
        gameObject.SetActive(true);

        _isAvailable = false;

        float multiplier = (pRvRequest._EffectMultiplier - 1f) * 100f;

        bool shouldShowDescription = (pRvRequest._RvEffectDescriptionText != string.Empty);

        _rvEffectDescriptionText.gameObject.SetActive(shouldShowDescription);
        if (shouldShowDescription)
        {
            if (pRvRequest._RvType == RvType.InstantGears)
            {
                int minGears = 7;
                int maxGears = 12;

                _instantGearsReward = Random.Range(minGears, maxGears);
                _rvEffectDescriptionText.text = string.Format(pRvRequest._RvEffectDescriptionText, _instantGearsReward);
            }
            else
            {
                _rvEffectDescriptionText.text = string.Format(pRvRequest._RvEffectDescriptionText, multiplier);
            }
        }
    }

    public int GetInstantGearsReward() => _instantGearsReward;

    public bool IsAvailable() => _isAvailable;
}
