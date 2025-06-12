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
    [SerializeField] private float snapSpeed = 15f; // 스냅 속도를 조금 올려 더 반응성 좋게 만듭니다.
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
        // screenObjects[0]는 항상 중앙, [1]은 위, [2]는 아래
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
        
        // 1: 위로 이동 (다음), -1: 아래로 이동 (이전), 0: 제자리
        int direction = 0; 

        // [방향 로직 최종 수정]
        // 사용자가 손가락을 위로 올리면(y값 증가) -> Content가 위로 움직여야 함 (direction = 1) -> 다음 아이템
        if (velocityY > minSwipeVelocity) direction = 1;
        else if (velocityY < -minSwipeVelocity) direction = -1;
        else
        {
            if (currentY > _itemHeight * 0.5f) direction = 1;
            else if (currentY < -_itemHeight * 0.5f) direction = -1;
        }
        
        _snapCoroutine = StartCoroutine(SnapAndReposition(direction));
    }
    
    private IEnumerator SnapAndReposition(int direction)
    {
        // 1. 목표 위치까지 부드럽게 애니메이션
        float targetY = direction * _itemHeight;
        while (Mathf.Abs(contentRect.anchoredPosition.y - targetY) > 1f)
        {
            if (_isDragging) yield break;
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, new Vector2(0, targetY), Time.deltaTime * snapSpeed);
            yield return null;
        }

        if (_isDragging) yield break;
        contentRect.anchoredPosition = new Vector2(0, targetY);

        // 2. 스와이프가 일어났을 경우, 데이터 업데이트 및 '하나만' 재배치
        if (direction != 0)
        {
            if (direction == 1) // Content가 위로 이동 (다음 아이템을 봄)
            {
                _currentDataIndex = GetNextDataIndex(_currentDataIndex);
                // 맨 아래 있던 아이템(y값이 가장 작음)을 맨 위로 재활용
                RectTransform bottomObject = FindObjectWithLowestY();
                bottomObject.anchoredPosition += new Vector2(0, _itemHeight * 3);
                UpdateScreenContent(bottomObject, GetNextDataIndex(_currentDataIndex));
            }
            else // Content가 아래로 이동 (이전 아이템을 봄)
            {
                _currentDataIndex = GetPreviousDataIndex(_currentDataIndex);
                // 맨 위에 있던 아이템(y값이 가장 큼)을 맨 아래로 재활용
                RectTransform topObject = FindObjectWithHighestY();
                topObject.anchoredPosition -= new Vector2(0, _itemHeight * 3);
                UpdateScreenContent(topObject, GetPreviousDataIndex(_currentDataIndex));
            }
        }

        // 3. Content 패널의 위치를 순간이동시켜 화면이 제자리에 있는 것처럼 보이게 함
        // 이 작업은 아이템 재배치와 동기화되어 점프 현상을 없앱니다.
        contentRect.anchoredPosition -= new Vector2(0, targetY);
        
        _snapCoroutine = null;
    }
    
    private RectTransform FindObjectWithHighestY()
    {
        RectTransform highest = screenObjects[0];
        for (int i = 1; i < screenObjects.Length; i++)
        {
            if (screenObjects[i].anchoredPosition.y > highest.anchoredPosition.y)
                highest = screenObjects[i];
        }
        return highest;
    }

    private RectTransform FindObjectWithLowestY()
    {
        RectTransform lowest = screenObjects[0];
        for (int i = 1; i < screenObjects.Length; i++)
        {
            if (screenObjects[i].anchoredPosition.y < lowest.anchoredPosition.y)
                lowest = screenObjects[i];
        }
        return lowest;
    }
    
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