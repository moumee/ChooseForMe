// ProfilePanelUI.cs
using UnityEngine;
using TMPro; // TextMeshPro ����� ���� �߰�
using Firebase.Auth; // ���� ����� UID�� �������� ���� �߰�
using System.Threading.Tasks; // �񵿱� �۾��� ���� �߰�

public class ProfilePanelUI : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Header("�Ŵ��� ����")]
    // �ٸ� GameObject�� �ִ� ProfileManager�� �����ϱ� ���� [SerializeField] ���
    [SerializeField] private ProfileManager _profileManager;

    /// <summary>
    /// �� GameObject�� Ȱ��ȭ�� ������ Unity�� ���� �ڵ����� ȣ��˴ϴ�.
    /// </summary>
    private async void OnEnable()
    {
        // ProfileManager�� ����Ǿ� �ִ���, ����ڰ� �α��� �������� Ȯ��
        if (_profileManager == null)
        {
            Debug.LogError("ProfileManager�� ������� �ʾҽ��ϴ�!");
            return;
        }

        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("�α��εǾ� ���� �ʽ��ϴ�.");
            // �ʿ��ϴٸ� �ؽ�Ʈ�� "�α��� �ʿ�" ������ ����
            return;
        }

        // ������ �ҷ����� ��, UI�� "�ε� ��..." ���·� ���� (���� ����)
        _nicknameText.text = "�ε� ��...";
        _levelText.text = "Lv. ?";
        _scoreText.text = "? ��";

        // ProfileManager�� �޼ҵ带 ȣ���Ͽ� �� �����͸� �񵿱������� �����ɴϴ�.
        // Task.WhenAll�� ����ϸ� ���� �񵿱� �۾��� ���ķ� �����ϰ� ��� ���� ������ ��ٸ� �� �ֽ��ϴ�.
        var nicknameTask = _profileManager.GetUserNicknameAsync(uid);
        var levelTask = _profileManager.GetUserLevelAsync(uid);
        var scoreTask = _profileManager.GetUserScoreAsync(uid);

        await Task.WhenAll(nicknameTask, levelTask, scoreTask);

        // �� �۾��� ����� �����ɴϴ�.
        string nickname = await nicknameTask;
        int level = await levelTask;
        int score = await scoreTask;

        // ������ �����ͷ� UI �ؽ�Ʈ�� ������Ʈ�մϴ�.
        _nicknameText.text = nickname ?? "�̸� ����";
        _levelText.text = $"Lv. {level}";
        _scoreText.text = $"{score} ��";
    }
}