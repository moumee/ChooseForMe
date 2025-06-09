using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class OptionImageButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("버튼 클릭 시 발생하는 이벤트. 인스펙터에서 연결하거나 코드로 리스너를 추가할 수 있습니다.")]
    public UnityEvent onClick;

    [Header("애니메이션 설정")]
    [SerializeField] private float _scaleDownFactor = 0.85f;
    [SerializeField] private float _animationDuration = 0.12f;

    private Vector3 _originalScale;
    private Color _originalColor;
    private Sequence _pressAnimation;
    private Image _image;
    
    // 드래그와 클릭을 구분하기 위한 변수들
    private const float DragThreshold = 10.0f;
    private Vector2 _pointerDownPosition;
    private bool _isDragging = false;
    
    // 상위 스크롤뷰 이벤트를 전달하기 위한 참조
    private ScrollRect _parentScrollRect;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _parentScrollRect = GetComponentInParent<ScrollRect>();

        _originalScale = transform.localScale;
        _originalColor = _image.color;
    }

    // OnImageButtonClick 메서드는 더 이상 필요 없으므로 삭제합니다.

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressAnimation?.Kill();
        
        Color pressColor = new Color(_originalColor.r * 0.8f, _originalColor.g * 0.8f, _originalColor.b * 0.8f, _originalColor.a);
        
        _pressAnimation = DOTween.Sequence();
        _pressAnimation.Append(transform.DOScale(_originalScale * _scaleDownFactor, _animationDuration))
            .Join(_image.DOColor(pressColor, _animationDuration));
        
        _isDragging = false;
        _pointerDownPosition = eventData.position;

        // 드래그를 위해 상위 스크롤뷰로 이벤트 전달
        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressAnimation?.Kill();
        
        _pressAnimation = DOTween.Sequence();
        _pressAnimation.Append(transform.DOScale(_originalScale, _animationDuration));

        // 색상 복원 애니메이션이 끝나면 onClick 이벤트 호출
        _image.DOColor(_originalColor, _animationDuration).OnComplete(() =>
        {
            // 드래그가 아닐 때만 클릭으로 간주하고 onClick 이벤트 발생
            if (!_isDragging)
            {
                onClick?.Invoke();
            }
        });
        
        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        }
    }

    #region 드래그 이벤트 전달 (변경 없음)
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging && Vector2.Distance(eventData.position, _pointerDownPosition) > DragThreshold)
        {
            _isDragging = true;
        }

        if (_parentScrollRect)
        {
            ExecuteEvents.Execute(_parentScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
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
    #endregion

    private void OnDestroy()
    {
        // 이 오브젝트가 파괴될 때 모든 리스너를 깔끔하게 제거
        onClick.RemoveAllListeners();
    }
}