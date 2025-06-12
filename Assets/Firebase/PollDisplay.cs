using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 필요
using System.Collections.Generic; // List<> 사용을 위해 필요

public class PollDisplay : MonoBehaviour
{
    [Header("표시할 UI 텍스트")]
    [SerializeField] private TextMeshProUGUI _questionText;       // 투표 제목
    [SerializeField] private TextMeshProUGUI _option1Text;          // 옵션 1 텍스트
    [SerializeField] private TextMeshProUGUI _option2Text;          // 옵션 2 텍스트
    [SerializeField] private TextMeshProUGUI _option1VoteCountText; // 옵션 1 득표수
    [SerializeField] private TextMeshProUGUI _option2VoteCountText; // 옵션 2 득표수

    /// <summary>
    /// 외부(예: PollListManager)에서 PollData를 받아와 UI의 각 텍스트 필드를 채웁니다.
    /// </summary>
    /// <param name="data">표시할 투표의 정보가 담긴 객체</param>
    public void SetData(PollData data)
    {
        // 데이터가 없는 예외적인 경우, 이 카드를 비활성화하고 종료
        if (data == null)
        {
            Debug.LogWarning("표시할 투표 데이터가 없습니다.");
            gameObject.SetActive(false);
            return;
        }

        // 1. 투표 제목 설정
        _questionText.text = data.Question;

        // 2. 옵션 1 텍스트 및 득표수 설정
        //    (리스트에 항목이 없을 경우를 대비한 안전장치 포함)
        if (data.Options != null && data.Options.Count > 0)
        {
            _option1Text.text = data.Options[0];
        }
        _option1VoteCountText.text = $"{data.Option1Votes} 표";

        // 3. 옵션 2 텍스트 및 득표수 설정
        if (data.Options != null && data.Options.Count > 1)
        {
            _option2Text.text = data.Options[1];
        }
        _option2VoteCountText.text = $"{data.Option2Votes} 표";
    }
}