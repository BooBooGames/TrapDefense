using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UserInput : MonoBehaviour
{
    private static UserInput Instance;

    public static event Action<Vector2> OnTouchPressed;
    public static event Action<Vector2> OnTouchHold;
    public static event Action<Vector2> OnTouchReleased;
    public static event Action<Vector2> OnTouchUI;
    public static event Action OnClicked;

    private bool _wasPressedLastFrame = false;
    private bool _isInputOn = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!Application.isFocused)
        {
            _wasPressedLastFrame = false;
            return;
        }

        Vector2 clickPosition;
        bool isPressed;
        bool pressedThisFrame;
        bool releasedThisFrame;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Mouse.current == null)
            return;

        clickPosition = Mouse.current.position.ReadValue();
        isPressed = Mouse.current.leftButton.isPressed;
        pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
        releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;

#else
        if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0)
        {
            return;
        }

        var touch = Touchscreen.current.touches[0];

        clickPosition = touch.position.ReadValue();
        isPressed = touch.press.isPressed;
        pressedThisFrame = touch.press.wasPressedThisFrame;
        releasedThisFrame = touch.press.wasReleasedThisFrame;
#endif

        if (!isPressed && !_wasPressedLastFrame && !pressedThisFrame && !releasedThisFrame)
        {
            return;
        }

        if (pressedThisFrame)
        {
            OnClicked?.Invoke();
        }
    }

    private bool IsTouchOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach(RaycastResult raycastResult in results)
        {
            //Debug.Log($"Clicked on UI Object {raycastResult.gameObject.name}");
        }

        return results.Count > 0;
    }

    private IEnumerator TurnOnInputAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        _isInputOn = true;
    }

    public void ToggleInput(bool pActivate)
    {
        if (pActivate)
        {
            StartCoroutine(TurnOnInputAfterDelay());
        }
        else
        {
            _isInputOn = pActivate;
        }
    }
}
