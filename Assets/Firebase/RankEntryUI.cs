// RankEntryUI.cs
using UnityEngine;
using TMPro;

public class RankEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    // �����͸� �޾ƿ� UI �ؽ�Ʈ�� ä��� �޼ҵ�
    public void SetData(RankerData data)
    {
        _nicknameText.text = data.Nickname;
        _scoreText.text = $"{data.Score}��";
    }

    // �����Ͱ� ���� �� UI�� ��Ȱ��ȭ�ϴ� �޼ҵ�
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}