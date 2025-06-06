using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class OptionImageButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    private float _scaleDownFactor = 0.85f;
    private float _animationDuration = 0.12f;

    private Vector3 _originalScale;
    private Color _originalColor;
    private Sequence _pressAnimation;
    private Image _image;
    
    private const float DragThreshold = 10.0f;
    public UnityEvent onClick;

    private Vector2 _pointerDownPosition;
    private bool _isDragging = false;
    
    private ScrollRect _parentScrollRect;
    private VoteSwipePage _swipePage;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _parentScrollRect = GetComponentInParent<ScrollRect>();
        _swipePage = GetComponentInParent<VoteSwipePage>();
        
        // onClick.AddListener(OnImageButtonClick);

        _originalScale = transform.localScale;
        _originalColor = _image.color;
    }

    private void OnImageButtonClick()
    {
        if (_swipePage.CurrentState == SwipePageState.Initial)
        {
            _swipePage.SwitchToPredictionVote();
        }
        else if (_swipePage.CurrentState == SwipePageState.Prediction)
        {
            _swipePage.SwitchToResult();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pressAnimation != null && _pressAnimation.IsActive())
        {
            _pressAnimation.Kill();
        }
        
        Color pressColor = new Color(_originalColor.r * 0.8f, _originalColor.g * 0.8f, _originalColor.b * 0.8f, _originalColor.a);
        
        _pressAnimation = DOTween.Sequence();
        _pressAnimation.Append(transform.DOScale(_originalScale * _scaleDownFactor, _animationDuration))
            .Join(_image.DOColor(pressColor, _animationDuration));
        
        
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
        if (_pressAnimation != null && _pressAnimation.IsActive())
        {
            _pressAnimation.Kill();
        }
        
        _pressAnimation = DOTween.Sequence();
        _pressAnimation.Append(transform.DOScale(_originalScale, _animationDuration));

        _image.DOColor(_originalColor, _animationDuration).OnComplete(OnImageButtonClick);
        
        if (!_isDragging)
        {
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
