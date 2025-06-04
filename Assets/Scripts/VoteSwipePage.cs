using System;
using TMPro;
using UnityEngine;

public enum SwipePageState
{
    Initial,
    Prediction,
    Result
}

public class VoteSwipePage : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;

    [Header("Pages")] 
    [SerializeField] private GameObject resultPage;

    public SwipePageState CurrentState { get; private set; } = SwipePageState.Initial;

    public void SwitchToPredictionVote()
    {
        CurrentState = SwipePageState.Prediction;
        titleText.text = "현재 우세한 항목은?";
        descriptionText.text = "현재 득표 수가 더 많다고 생각되는 항목을 선택해주세요";
    }

    public void SwitchToResult()
    {
        CurrentState = SwipePageState.Result;
        resultPage.SetActive(true);
    }

    public void SetOptionText(string optionA, string optionB)
    {
        optionAText.text = optionA;
        optionBText.text = optionB;
    }

}
