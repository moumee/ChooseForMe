using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 스와이프 페이지의 상태를 정의합니다.
/// </summary>
public enum SwipePageState
{
    Initial,    // 초기 투표 상태
    Prediction, // 예측 진행 상태
    Result      // 결과 확인 상태
}

/// <summary>
/// 각 스와이프 페이지의 모든 상태 정보를 담는 데이터 클래스입니다.
/// 이 클래스는 컨트롤러(InfiniteSwipe)가 중앙에서 관리합니다.
/// </summary>
[System.Serializable]
public class VotePageData
{
    public int contentID;
    public string title;
    public string description;
    public string optionA;
    public string optionB;
    public SwipePageState currentState = SwipePageState.Initial;
    // 결과 데이터 등 필요한 모든 정보를 여기에 추가할 수 있습니다.
}

/// <summary>
/// UI 프리팹에 부착되어 하나의 페이지 뷰(View)를 책임지는 스크립트입니다.
/// 스스로 상태를 가지지 않으며, 외부에서 주입된 VotePageData에 따라 화면을 그립니다.
/// </summary>
public class VoteSwipePage : MonoBehaviour
{
    [Header("UI 요소 (Text Mesh Pro)")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;

    [Header("활성화/비활성화 될 페이지")]
    [SerializeField] private GameObject resultPage;

    [Header("하위 컨트롤 (연결 필요)")]
    [SerializeField] private OptionImageButton optionAButton; // 옵션 A 이미지 버튼
    [SerializeField] private OptionImageButton optionBButton; // 옵션 B 이미지 버튼

    // --- 내부 변수 ---
    private VotePageData _data; // 현재 이 페이지가 표시해야 할 데이터
    private Action<VotePageData> _onStateChangedCallback; // 상태 변경 시 컨트롤러에 알릴 콜백

    /// <summary>
    /// InfiniteSwipe 컨트롤러가 호출하여 데이터를 주입하고 리스너를 설정하는 함수.
    /// 이 페이지의 생명주기는 이 함수 호출로부터 시작됩니다.
    /// </summary>
    public void Setup(VotePageData data, Action<VotePageData> onStateChangedCallback)
    {
        _data = data;
        _onStateChangedCallback = onStateChangedCallback;

        // 버튼 리스너 초기화 및 재설정
        // 재활용되는 오브젝트의 리스너가 중복으로 쌓이는 것을 방지합니다.
        if (optionAButton != null)
        {
            optionAButton.onClick.RemoveAllListeners();
            optionAButton.onClick.AddListener(HandleOptionClick);
        }
        if (optionBButton != null)
        {
            optionBButton.onClick.RemoveAllListeners();
            optionBButton.onClick.AddListener(HandleOptionClick);
        }

        // 전달받은 데이터 기준으로 화면 전체를 갱신
        UpdateVisuals();
    }
    
    /// <summary>
    /// 옵션 버튼 중 하나가 클릭되었을 때 호출될 단일 핸들러 메서드.
    /// </summary>
    private void HandleOptionClick()
    {
        // 페이지의 현재 상태는 _data 객체를 통해 확인합니다.
        if (_data.currentState == SwipePageState.Initial)
        {
            SwitchToPredictionVote();
        }
        else if (_data.currentState == SwipePageState.Prediction)
        {
            SwitchToResult();
        }
    }

    /// <summary>
    /// 현재 데이터(_data)에 맞춰 모든 UI 요소를 갱신합니다.
    /// </summary>
    private void UpdateVisuals()
    {
        if (_data == null) return;

        // 공통 텍스트는 항상 데이터에 맞게 설정
        optionAText.text = _data.optionA;
        optionBText.text = _data.optionB;

        // 상태에 따라 다른 UI 표시
        switch (_data.currentState)
        {
            case SwipePageState.Initial:
                titleText.text = _data.title;
                descriptionText.text = _data.description;
                resultPage.SetActive(false);
                break;

            case SwipePageState.Prediction:
                titleText.text = "현재 우세한 항목은?";
                descriptionText.text = "현재 득표 수가 더 많다고 생각되는 항목을 선택해주세요.";
                resultPage.SetActive(false);
                break;

            case SwipePageState.Result:
                // 결과 페이지가 활성화될 때 필요한 텍스트 설정 등은 여기서 처리
                titleText.text = "결과 확인"; // 예시
                descriptionText.text = "투표 결과입니다."; // 예시
                resultPage.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 상태를 '예측'으로 변경합니다. 데이터 변경 후 콜백을 호출하고 UI를 갱신합니다.
    /// </summary>
    public void SwitchToPredictionVote()
    {
        if (_data.currentState != SwipePageState.Initial) return;

        // 1. 데이터의 상태를 변경
        _data.currentState = SwipePageState.Prediction;
        
        // 2. 변경된 데이터를 컨트롤러에 알려서 중앙 데이터 리스트를 업데이트하게 함
        _onStateChangedCallback?.Invoke(_data);
        
        // 3. 변경된 데이터를 기반으로 자신의 UI를 갱신
        UpdateVisuals();
    }

    /// <summary>
    /// 상태를 '결과'로 변경합니다. 데이터 변경 후 콜백을 호출하고 UI를 갱신합니다.
    /// </summary>
    public void SwitchToResult()
    {
        if (_data.currentState != SwipePageState.Prediction) return;

        _data.currentState = SwipePageState.Result;
        _onStateChangedCallback?.Invoke(_data);
        UpdateVisuals();
    }
}