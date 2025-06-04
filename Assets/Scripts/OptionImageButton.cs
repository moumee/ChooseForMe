using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionImageButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float DragThreshold = 10.0f;
    public UnityEvent onClick;

    private Vector2 _pointerDownPosition;
    private bool _isDragging = false;
    
    private ScrollRect _parentScrollRect;
    private VoteSwipePage _swipePage;

    private void Awake()
    {
        _parentScrollRect = GetComponentInParent<ScrollRect>();
        _swipePage = GetComponentInParent<VoteSwipePage>();
        onClick.AddListener(OnImageButtonClick);
    }

    private void OnImageButtonClick()
    {
        if (_swipePage.CurrentState == SwipePageState.Initial)
        {
            // Go to prediction vote using function in VoteSwipePage
        }
        else if (_swipePage.CurrentState == SwipePageState.Prediction)
        {
            // Go to result using function in VoteSwipePage
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = false;
        _pointerDownPosition = eventData.position;

        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            float distance = Vector2.Distance(eventData.position, _pointerDownPosition);

            if (distance > DragThreshold) _isDragging = true;
        }

        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            Debug.Log("onClick Invoked!");
            onClick?.Invoke();
        }

        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }

    private void OnDestroy()
    {
        onClick.RemoveAllListeners();
    }
}
