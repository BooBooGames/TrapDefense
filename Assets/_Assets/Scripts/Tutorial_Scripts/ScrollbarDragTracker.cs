using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarDragTracker : MonoBehaviour,
    IBeginDragHandler,
    IEndDragHandler
{
    [SerializeField] private Scrollbar scrollbar;

    public event Action OnStartedDragging;
    public event Action OnStoppedDragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnStartedDragging?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnStoppedDragging?.Invoke();
    }
}