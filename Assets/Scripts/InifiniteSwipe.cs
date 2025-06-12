using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class InfiniteSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("필수 연결 항목")]
    [Tooltip("아이템들이 자식으로 생성될 부모 패널")]
    [SerializeField] private RectTransform contentPanel;
    
    [Tooltip("복제해서 사용할 UI 아이템 프리팹")]
    [SerializeField] private GameObject itemPrefab;

    [Header("기능 설정")]
    [Tooltip("테스트용 가상 데이터 개수. 이 개수만큼 콘텐츠가 있는 것처럼 순환합니다.")]
    [SerializeField] private int testItemCount = 20;

    [Tooltip("스와이프 애니메이션 시간")]
    [SerializeField] private float swipeAnimationTime = 0.3f;

    [Tooltip("스와이프로 인식될 최소 드래그 거리")]
    [SerializeField] private float minSwipeDistance = 100f;

    // --- 내부 변수 (수정 필요 없음) ---
    private const int PoolSize = 3;
    private List<RectTransform> _itemPool = new List<RectTransform>();
    private int _currentIndex = 0;
    private float _itemHeight;
    private bool _isSwiping = false;
    private Vector2 _dragStartPosition;
    private Vector2 _panelDragStartPosition;
    

    void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        // 프리팹이나 패널이 연결되지 않으면 실행 중지
        if (contentPanel == null || itemPrefab == null)
        {
            Debug.LogError("Content Panel 또는 Item Prefab이 연결되지 않았습니다!");
            return;
        }

        // 기존에 생성된 아이템이 있다면 모두 삭제
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        _itemPool.Clear();

        // 레이아웃 그룹 비활성화
        if (contentPanel.GetComponent<UnityEngine.UI.VerticalLayoutGroup>() != null)
        {
            contentPanel.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().enabled = false;
        }

        _itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height;
        if (_itemHeight == 0)
        {
            Debug.LogError("Item Prefab의 Height가 0입니다! RectTransform에서 고정된 높이 값을 설정해주세요.");
            return;
        }

        // 3개의 아이템 풀 생성
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject itemGO = Instantiate(itemPrefab, contentPanel);
            _itemPool.Add(itemGO.GetComponent<RectTransform>());
        }

        _currentIndex = 0;
        UpdateItemsPositionAndName();
    }

    #region 드래그 이벤트
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        _dragStartPosition = eventData.position;
        _panelDragStartPosition = contentPanel.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        float dragDeltaY = eventData.position.y - _dragStartPosition.y;
        contentPanel.anchoredPosition = _panelDragStartPosition + new Vector2(0, dragDeltaY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        float dragDistanceY = eventData.position.y - _dragStartPosition.y;

        if (dragDistanceY < -minSwipeDistance) StartCoroutine(SwipeTo(1));
        else if (dragDistanceY > minSwipeDistance) StartCoroutine(SwipeTo(-1));
        else StartCoroutine(SwipeTo(0));
    }
    #endregion

    #region 핵심 재배치/스와이프 로직
    private int GetCircularIndex(int index)
    {
        if (testItemCount == 0) return 0;
        return (index % testItemCount + testItemCount) % testItemCount;
    }

    private IEnumerator SwipeTo(int direction)
    {
        _isSwiping = true;

        Vector2 startPosition = contentPanel.anchoredPosition;
        Vector2 endPosition = new Vector2(0, -_itemHeight * direction);

        float timer = 0f;
        while (timer < swipeAnimationTime)
        {
            timer += Time.deltaTime;
            contentPanel.anchoredPosition = Vector2.Lerp(startPosition, endPosition, Mathf.SmoothStep(0, 1, timer / swipeAnimationTime));
            yield return null;
        }

        if (direction != 0)
        {
            _currentIndex = GetCircularIndex(_currentIndex + direction);
            contentPanel.anchoredPosition = Vector2.zero;
            UpdateItemsPositionAndName();
        }
        else
        {
            contentPanel.anchoredPosition = endPosition;
        }

        _isSwiping = false;
    }

    private void UpdateItemsPositionAndName()
    {
        // 중앙 아이템 (pool[1])
        UpdateSingleItem(_itemPool[1], _currentIndex);
        _itemPool[1].anchoredPosition = Vector2.zero;

        // 위쪽 아이템 (pool[0])
        UpdateSingleItem(_itemPool[0], _currentIndex - 1);
        _itemPool[0].anchoredPosition = new Vector2(0, _itemHeight);

        // 아래쪽 아이템 (pool[2])
        UpdateSingleItem(_itemPool[2], _currentIndex + 1);
        _itemPool[2].anchoredPosition = new Vector2(0, -_itemHeight);
    }
    
    /// <summary>
    /// 아이템 하나의 이름만 현재 데이터 인덱스에 맞게 바꿔줍니다. (디버깅용)
    /// 데이터 표시는 이 스크립트가 책임지지 않습니다.
    /// </summary>
    private void UpdateSingleItem(RectTransform itemRect, int dataIndex)
    {
        int circularIndex = GetCircularIndex(dataIndex);
        itemRect.gameObject.name = $"Item_ContentID_{circularIndex}";
        
        // 데이터 표시는 여기서 하는 것이 아니라,
        // Item Prefab에 붙어있는 별도의 스크립트가 스스로의 이름(인덱스)을 보고 처리해야 합니다.
    }
    #endregion
}