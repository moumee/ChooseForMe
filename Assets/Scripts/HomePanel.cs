using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HomePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    [SerializeField] private Button createVoteButton;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private RectTransform[] screenObjects;

    [Header("Scroll & Snap Settings")]
    [SerializeField] private float snapSpeed = 15f;
    [SerializeField] private float minSwipeVelocity = 200f;

    [Header("Data Source")]
    public List<ContentData> contentDatas = new List<ContentData>();

    private int _itemCount;
    private int _currentDataIndex = 0;
    private float _itemHeight;
    private Coroutine _snapCoroutine;
    private Vector2 _beginDragPointerPos;
    private Vector2 _beginDragContentPos;
    private float _dragStartTime;
    private bool _isDragging = false;

    void Start()
    {
        _itemCount = contentDatas.Count;
        if (_itemCount < 3)
        {
            if(gameObject.activeInHierarchy) gameObject.SetActive(false);
            return;
        }

        if (contentRect.GetComponent<VerticalLayoutGroup>() != null)
            contentRect.GetComponent<VerticalLayoutGroup>().enabled = false;
        if (contentRect.GetComponent<ContentSizeFitter>() != null)
            contentRect.GetComponent<ContentSizeFitter>().enabled = false;
        
        _itemHeight = contentRect.rect.height;
        InitializeScreens();
        createVoteButton.onClick.AddListener(OnCreateVoteButtonClick);
        contentRect.anchoredPosition = Vector2.zero;
    }

    private void InitializeScreens()
    {
        screenObjects[0].anchoredPosition = new Vector2(0, 0);
        screenObjects[1].anchoredPosition = new Vector2(0, _itemHeight);
        screenObjects[2].anchoredPosition = new Vector2(0, -_itemHeight);

        UpdateScreenContent(screenObjects[0], _currentDataIndex);
        UpdateScreenContent(screenObjects[1], GetPreviousDataIndex(_currentDataIndex));
        UpdateScreenContent(screenObjects[2], GetNextDataIndex(_currentDataIndex));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        if (_snapCoroutine != null) StopCoroutine(_snapCoroutine);
        _beginDragPointerPos = eventData.position;
        _beginDragContentPos = contentRect.anchoredPosition;
        _dragStartTime = Time.time;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragDelta = eventData.position - _beginDragPointerPos;
        contentRect.anchoredPosition = _beginDragContentPos + new Vector2(0, dragDelta.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        
        float dragDuration = Time.time - _dragStartTime;
        if (dragDuration < 0.01f) dragDuration = 0.01f;
        float velocityY = (eventData.position.y - _beginDragPointerPos.y) / dragDuration;

        float currentY = contentRect.anchoredPosition.y;
        
        int direction = 0; // 1: 위로 이동 (다음), -1: 아래로 이동 (이전)
        
        // 방향 판단 로직
        if (velocityY > minSwipeVelocity) direction = 1;
        else if (velocityY < -minSwipeVelocity) direction = -1;
        else
        {
            if (currentY > _itemHeight * 0.5f) direction = 1;
            else if (currentY < -_itemHeight * 0.5f) direction = -1;
        }
        
        _snapCoroutine = StartCoroutine(SnapAndReset(direction));
    }
    
    // ==========================================================
    // 업데이트 속도 문제를 해결한 최종 코루틴
    // ==========================================================
    private IEnumerator SnapAndReset(int direction)
    {
        // 1. 스와이프가 일어났다면, '즉시' 데이터와 화면을 다음/이전 상태로 업데이트
        if (direction != 0)
        {
            // 사용자가 드래그로 밀어놓은 현재 content 위치를 저장
            float currentY = contentRect.anchoredPosition.y;

            // 데이터 인덱스를 먼저 변경
            if (direction == 1) // Content가 위로 이동 (다음 아이템을 볼 것)
                _currentDataIndex = GetNextDataIndex(_currentDataIndex);
            else // Content가 아래로 이동 (이전 아이템을 볼 것)
                _currentDataIndex = GetPreviousDataIndex(_currentDataIndex);
            
            // 변경된 데이터 인덱스를 기준으로 화면 전체를 즉시 리셋
            InitializeScreens();

            // 중요: 화면을 리셋한 뒤, Content의 위치를 사용자가 드래그를 멈춘 위치에서 '이어지는 것처럼' 보이도록 설정
            // 이렇게 하면 화면이 깜빡이거나 점프하는 느낌이 사라집니다.
            contentRect.anchoredPosition = new Vector2(0, currentY - (direction * _itemHeight));
        }

        // 2. 이제 화면을 제자리(y=0)로 부드럽게 정렬하는 애니메이션만 보여줌
        while (Mathf.Abs(contentRect.anchoredPosition.y) > 1f)
        {
            if (_isDragging) yield break;
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, Vector2.zero, Time.deltaTime * snapSpeed);
            yield return null;
        }

        // 3. 애니메이션 종료 후 정확한 위치로 고정
        contentRect.anchoredPosition = Vector2.zero;
        _snapCoroutine = null;
    }
    
    // --- 나머지 헬퍼 메서드 (이전과 동일) ---
    private void UpdateScreenContent(RectTransform screen, int dataIndex)
    {
        ContentData data = contentDatas[dataIndex];
        ScreenContentUI ui = screen.GetComponent<ScreenContentUI>();
        if (ui != null) ui.SetContent(data);
    }
    private int GetNextDataIndex(int index) => (index + 1) % _itemCount;
    private int GetPreviousDataIndex(int index) => (index - 1 + _itemCount) % _itemCount;
    private void OnCreateVoteButtonClick()
    {
        Debug.Log("투표 생성 버튼 클릭");
    }
}