using UnityEngine;
using TMPro; // TextMeshPro ����� ���� �ʿ�
using System.Collections.Generic; // List<> ����� ���� �ʿ�

public class PollDisplay : MonoBehaviour
{
    [Header("ǥ���� UI �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI _questionText;       // ��ǥ ����
    [SerializeField] private TextMeshProUGUI _option1Text;          // �ɼ� 1 �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI _option2Text;          // �ɼ� 2 �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI _option1VoteCountText; // �ɼ� 1 ��ǥ��
    [SerializeField] private TextMeshProUGUI _option2VoteCountText; // �ɼ� 2 ��ǥ��

    /// <summary>
    /// �ܺ�(��: PollListManager)���� PollData�� �޾ƿ� UI�� �� �ؽ�Ʈ �ʵ带 ä��ϴ�.
    /// </summary>
    /// <param name="data">ǥ���� ��ǥ�� ������ ��� ��ü</param>
    public void SetData(PollData data)
    {
        // �����Ͱ� ���� �������� ���, �� ī�带 ��Ȱ��ȭ�ϰ� ����
        if (data == null)
        {
            Debug.LogWarning("ǥ���� ��ǥ �����Ͱ� �����ϴ�.");
            gameObject.SetActive(false);
            return;
        }

        // 1. ��ǥ ���� ����
        _questionText.text = data.Question;

        // 2. �ɼ� 1 �ؽ�Ʈ �� ��ǥ�� ����
        //    (����Ʈ�� �׸��� ���� ��츦 ����� ������ġ ����)
        if (data.Options != null && data.Options.Count > 0)
        {
            _option1Text.text = data.Options[0];
        }
        _option1VoteCountText.text = $"{data.Option1Votes} ǥ";

        // 3. �ɼ� 2 �ؽ�Ʈ �� ��ǥ�� ����
        if (data.Options != null && data.Options.Count > 1)
        {
            _option2Text.text = data.Options[1];
        }
        _option2VoteCountText.text = $"{data.Option2Votes} ǥ";
    }
}