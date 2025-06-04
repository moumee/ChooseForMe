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
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;

    public SwipePageState CurrentState { get; set; } = SwipePageState.Initial;

    public void SwitchHeaderText()
    {
        titleText.text = "현재 우세한 항목은?";
        descriptionText.text = "현재 득표 수가 더 많다고 생각되는 항목을 선택해주세요";
    }

    public void SetOptionText(string optionA, string optionB)
    {
        optionAText.text = optionA;
        optionBText.text = optionB;
    }

}
