using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class HomePanel : MonoBehaviour
{
    [SerializeField] private Button createVoteButton;
    
    [Header("Swipe UI")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private float swipeTime = 0.2f;
    [SerializeField] private float swipeDistance = 50.0f;
    
    private float[] _scrollPageValues;
    private float _valueDistance;
    private int _currentPage = 0;
    private int _maxPage;
    private float _startTouchY;
    private float _endTouchY;
    private bool _isSwipeMode = false;
    
    private void Awake()
    {
        createVoteButton.onClick.AddListener(OnCreateVoteButtonClick);
        
        _scrollPageValues = new float[scrollRect.content.childCount];
    
        _valueDistance = 1f / (_scrollPageValues.Length - 1);
        
        for (int i = 0; i < _scrollPageValues.Length; i++)
        {
            _scrollPageValues[i] = 1f - _valueDistance * i;
        }
    
        _maxPage = scrollRect.content.childCount;
    
    }
    
    private void Start()
    {
        SetScrollbarValue(0);
    }
    
    public void SetScrollbarValue(int index)
    {
        _currentPage = index;
        scrollbar.value = _scrollPageValues[index];
    }
    
    private void Update()
    {
        UpdateInput();
    }
    
    private void UpdateInput()
    {
        if (_isSwipeMode) return;
    
        if (Input.GetMouseButtonDown(0) && !Comments.PointerOnComment)
        {
            _startTouchY = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_startTouchY != 0)
            {
                _endTouchY = Input.mousePosition.y;
                UpdateSwipe();
                _startTouchY = 0;
            }
           
        }
    }
    
    private void UpdateSwipe()
    {
        // If swiped less than swipe distance go back to current page
        if (Mathf.Abs(_startTouchY - _endTouchY) < swipeDistance)
        {
            StartCoroutine(OnSwipeOneStep(_currentPage));
            return;
        }
    
        bool isUp = _startTouchY < _endTouchY;
    
        if (isUp)
        {
            if (_currentPage == _maxPage - 1) return;
    
            _currentPage++;
        }
        else
        {
            if (_currentPage == 0) return;
    
            _currentPage--;
        }
        
        StartCoroutine(OnSwipeOneStep(_currentPage));
    }
    
    private IEnumerator OnSwipeOneStep(int index)
    {
        float start = scrollbar.value;
        float current = 0;
        float percent = 0;
        
        _isSwipeMode = true;
    
        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;
    
            scrollbar.value = Mathf.Lerp(start, _scrollPageValues[index], percent);
            
            yield return null;
        }
        
        _isSwipeMode = false;
    }
    
    
    private void OnCreateVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.CreateVote);
    }
}
