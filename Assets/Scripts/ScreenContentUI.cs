using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ScreenContentUI : MonoBehaviour
{
    private ContentData currentData;

    [Header("Phase Parent Groups")]
    [SerializeField] private GameObject votePhaseGroup;
    [SerializeField] private GameObject resultPhaseGroup;

    [Header("Vote/Prediction Phase UI")]
    [SerializeField] private TMP_Text vote_UserTitleText;
    [SerializeField] private TMP_Text vote_StateQuestionText;
    [SerializeField] private TMP_Text vote_StateDescriptionText;
    [SerializeField] private TMP_Text vote_OptionAText;
    [SerializeField] private TMP_Text vote_OptionBText;
    [SerializeField] private Image vote_ItemAImage;
    [SerializeField] private Image vote_ItemBImage;
    
    [Header("Result Phase UI")]
    [SerializeField] private TMP_Text result_TitleText;
    [SerializeField] private TMP_Text result_OptionAText;
    [SerializeField] private TMP_Text result_OptionBText;
    [SerializeField] private Slider result_SliderA;
    [SerializeField] private TMP_Text result_PercentA;
    [SerializeField] private Slider result_SliderB;
    [SerializeField] private TMP_Text result_PercentB;
    [SerializeField] private TMP_Text result_PredictionResultText;
    [Tooltip("항목 A의 슬라이더 위 라벨 (예: 치킨 60%)")]
    [SerializeField] private TMP_Text result_SliderLabelA;
    [Tooltip("항목 B의 슬라이더 위 라벨 (예: 피자 40%)")]
    [SerializeField] private TMP_Text result_SliderLabelB;
    
    [Header("Comments Section")]
    [SerializeField] private Transform result_CommentsContentArea;
    [SerializeField] private GameObject result_CommentPrefab;

    public void SetContent(ContentData data)
    {
        this.currentData = data;
        if (currentData == null) return;

        bool isResultState = data.currentState == VoteState.Result;

        votePhaseGroup.SetActive(!isResultState);
        resultPhaseGroup.SetActive(isResultState);

        if (isResultState)
        {
            SetupResultUI();
        }
        else
        {
            SetupVotePhaseUI();
        }
    }

    private void SetupVotePhaseUI()
    {
        vote_UserTitleText.text = currentData.voteTitle;
        vote_OptionAText.text = currentData.itemAName;
        vote_OptionBText.text = currentData.itemBName;
        vote_ItemAImage.sprite = currentData.itemAImage;
        vote_ItemBImage.sprite = currentData.itemBImage;

        if (currentData.currentState == VoteState.InitialVote)
        {
            vote_StateQuestionText.text = "자신의 원픽은?";
            vote_StateDescriptionText.text = "자신의 생각대로 투표해주세요.";
        }
        else // VoteState.Prediction
        {
            vote_StateQuestionText.text = "현재 우세한 항목은?";
            vote_StateDescriptionText.text = "현재 득표 수가 더 많다고 생각되는 항목을 선택해주세요.";
        }
    }

    private void SetupResultUI()
    {
        result_TitleText.text = $"[결과] {currentData.voteTitle}";
        result_OptionAText.text = currentData.itemAName;
        result_OptionBText.text = currentData.itemBName;

        result_SliderA.value = currentData.itemAResultPercent;
        result_SliderB.value = currentData.itemBResultPercent;

        result_PercentA.text = currentData.itemAResultPercent.ToString("P0");
        result_PercentB.text = currentData.itemBResultPercent.ToString("P0");
        
        // 슬라이더 위 라벨 텍스트 설정
        if (result_SliderLabelA != null)
        {
            result_SliderLabelA.text = $"{currentData.itemAName} {currentData.itemAResultPercent:P0}";
        }
        if (result_SliderLabelB != null)
        {
            result_SliderLabelB.text = $"{currentData.itemBName} {currentData.itemBResultPercent:P0}";
        }

        bool predictionSuccess = false;
        if (currentData.itemAResultPercent > currentData.itemBResultPercent && currentData.userPredictionChoice == 0)
            predictionSuccess = true;
        else if (currentData.itemBResultPercent > currentData.itemAResultPercent && currentData.userPredictionChoice == 1)
            predictionSuccess = true;
        
        result_PredictionResultText.text = predictionSuccess ? "🎉 예측 성공! 🎉" : "😢 예측 실패 😢";
        
        UpdateComments();
    }

    private void UpdateComments()
    {
        foreach (Transform child in result_CommentsContentArea) Destroy(child.gameObject);
        if (currentData.comments == null) return;
        foreach (string commentText in currentData.comments)
        {
            GameObject commentObject = Instantiate(result_CommentPrefab, result_CommentsContentArea);
            TMP_Text textComponent = commentObject.GetComponentInChildren<TMP_Text>();
            if (textComponent != null) textComponent.text = commentText;
        }
    }
    
    public void OnVoteButtonClicked(int choice)
    {
        if (currentData.currentState != VoteState.InitialVote) return;
        currentData.userVoteChoice = choice;
        currentData.currentState = VoteState.Prediction;
        SetContent(currentData);
    }
    
    public void OnPredictButtonClicked(int choice)
    {
        if (currentData.currentState != VoteState.Prediction) return;
        currentData.userPredictionChoice = choice;
        currentData.currentState = VoteState.Result;
        SetContent(currentData);
    }
}