using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private PollDataManager pollDataManager;
    
    [Header("UI State")]
    [SerializeField] private GameObject loadingIndicator;

    private int _itemCount;
    private int _currentDataIndex = 0;
    private float _itemHeight;
    private Coroutine _snapCoroutine;
    private Vector2 _beginDragPointerPos;
    private Vector2 _beginDragContentPos;
    private float _dragStartTime;
    private bool _isInitialized = false;
    private bool _isDragging = false;

    async void Start()
    {
        _isInitialized = false;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        bool isLoaded = await LoadAndConvertPollsAsync();
        
        if (!isLoaded)
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            if(gameObject.activeInHierarchy) gameObject.SetActive(false);
            return;
        }
        
        _itemCount = contentDatas.Count;
        if (_itemCount < 3)
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
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

        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        _isInitialized = true;
    }

    private async Task<bool> LoadAndConvertPollsAsync()
    {
        if (pollDataManager == null) { return false; }
        List<PollData> pollDataList = await pollDataManager.GetAllPollsAsync();
        if (pollDataList == null || pollDataList.Count == 0) { return false; }

        contentDatas.Clear();
        foreach (var poll in pollDataList)
        {
            if (poll.Options == null || poll.Options.Count < 2) continue;
            ContentData content = new ContentData {
                pollId = poll.Id, voteTitle = poll.Question, itemAName = poll.Options[0], itemBName = poll.Options[1],
                itemAImageUrl = (poll.OptionImages != null && poll.OptionImages.ContainsKey("0")) ? poll.OptionImages["0"] : "",
                itemBImageUrl = (poll.OptionImages != null && poll.OptionImages.ContainsKey("1")) ? poll.OptionImages["1"] : "",
                itemAResultPercent = (poll.TotalVoteCount > 0) ? (float)poll.Option1Votes / poll.TotalVoteCount : 0f,
                itemBResultPercent = (poll.TotalVoteCount > 0) ? (float)poll.Option2Votes / poll.TotalVoteCount : 0f,
                comments = new List<string>() 
            };
            contentDatas.Add(content);
        }
        return true;
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
        if (!_isInitialized) return;
        _isDragging = true;
        if (_snapCoroutine != null) StopCoroutine(_snapCoroutine);
        _beginDragPointerPos = eventData.position;
        _beginDragContentPos = contentRect.anchoredPosition;
        _dragStartTime = Time.time;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        Vector2 dragDelta = eventData.position - _beginDragPointerPos;
        contentRect.anchoredPosition = _beginDragContentPos + new Vector2(0, dragDelta.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        _isDragging = false;
        
        float dragDuration = Time.time - _dragStartTime;
        if (dragDuration < 0.01f) dragDuration = 0.01f;
        float velocityY = (eventData.position.y - _beginDragPointerPos.y) / dragDuration;
        float currentY = contentRect.anchoredPosition.y;
        
        // [방향 로직 최종 수정]
        // 1: 위로 스와이프 (다음 아이템), -1: 아래로 스와이프 (이전 아이템)
        int direction = 0; 
        
        if (velocityY > minSwipeVelocity) direction = 1;
        else if (velocityY < -minSwipeVelocity) direction = -1;
        else
        {
            if (currentY > _itemHeight * 0.5f) direction = 1;
            else if (currentY < -_itemHeight * 0.5f) direction = -1;
        }
        
        _snapCoroutine = StartCoroutine(SnapAndReset(direction));
    }
    
    private IEnumerator SnapAndReset(int direction)
    {
        // 목표 위치 계산: 위로 스와이프(direction=1)하면 Content는 위로(+H) 이동
        float targetY = direction * _itemHeight;
        
        // 목표 위치까지 부드럽게 스냅 애니메이션
        while (Mathf.Abs(contentRect.anchoredPosition.y - targetY) > 1f)
        {
            if (_isDragging) yield break;
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, new Vector2(0, targetY), Time.deltaTime * snapSpeed);
            yield return null;
        }
        
        // 애니메이션이 끝나면 정확한 위치로 고정
        contentRect.anchoredPosition = new Vector2(0, targetY);

        // 스와이프가 일어난 경우에만 데이터 및 화면 리셋
        if (direction != 0)
        {
            // 방향에 맞게 데이터 인덱스 업데이트
            if (direction == 1) // 다음 아이템으로
                _currentDataIndex = GetNextDataIndex(_currentDataIndex);
            else // 이전 아이템으로
                _currentDataIndex = GetPreviousDataIndex(_currentDataIndex);
            
            // 화면 전체를 새로운 데이터 인덱스 기준으로 리셋
            InitializeScreens();
        }
        
        // 마지막으로 Content 위치를 (0,0)으로 완벽하게 리셋하여 다음 스와이프 준비
        contentRect.anchoredPosition = Vector2.zero;
        _snapCoroutine = null;
    }
    
    private void UpdateScreenContent(RectTransform screen, int dataIndex)
    {
        if (_itemCount == 0) return;
        ContentData data = contentDatas[dataIndex];
        ScreenContentUI ui = screen.GetComponent<ScreenContentUI>();
        if (ui != null) ui.SetContent(data);
    }

    private int GetNextDataIndex(int index) => (_itemCount == 0) ? 0 : (index + 1) % _itemCount;
    private int GetPreviousDataIndex(int index) => (_itemCount == 0) ? 0 : (index - 1 + _itemCount) % _itemCount;
    
    private void OnCreateVoteButtonClick()
    {
        Debug.Log("투표 생성 버튼 클릭");
    }
}