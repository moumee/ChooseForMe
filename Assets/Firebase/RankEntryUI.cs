// RankEntryUI.cs
using UnityEngine;
using TMPro;

public class RankEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    // 데이터를 받아와 UI 텍스트를 채우는 메소드
    public void SetData(RankerData data)
    {
        _nicknameText.text = data.Nickname;
        _scoreText.text = $"{data.Score}점";
    }

    // 데이터가 없을 때 UI를 비활성화하는 메소드
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}