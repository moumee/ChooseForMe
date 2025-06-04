using UnityEngine;
using System.Threading.Tasks;

public class DebugManager : MonoBehaviour
{
    private ProfileManager _profileManager;

    void Start()
    {
        // ���� GameObject�� �ִ� ProfileManager ������Ʈ�� ã�ƿͼ� ������ �����մϴ�.
        _profileManager = GetComponent<ProfileManager>();
        if (_profileManager == null)
        {
            Debug.LogError("DebugManager: ProfileManager�� ã�� �� �����ϴ�! ���� GameObject�� �ִ��� Ȯ�����ּ���.");
        }
    }

    /// <summary>
    /// �׽�Ʈ ��ư�� OnClick() �̺�Ʈ�� �����Ͽ� ����� �޼ҵ��Դϴ�.
    /// ProfileManager�� ����ġ �߰��� ��û�մϴ�.
    /// </summary>
    public async void OnAddExperienceButtonClicked()
    {
        if (_profileManager == null)
        {
            Debug.LogError("ProfileManager�� ������� �ʾ� ������ �� �����ϴ�.");
            return;
        }

        Debug.Log("�׽�Ʈ: ����ġ 25 �߰��� ��û�մϴ�...");

        // ProfileManager�� �޼ҵ带 ȣ���Ͽ� ����ġ �߰��� ������ ó���� �� ���� ��û�մϴ�.
        // �߰��� ����ġ ��(��: 25)�� ���⼭ �����Ӱ� ������ �� �ֽ��ϴ�.
        ProfileManager.LevelUpResult result = await _profileManager.AddExperienceAsync(25);

        if (result.DidLevelUp)
        {
            Debug.Log($"������ ���: {result.LevelsGained} ���� ����Ͽ� {result.NewLevel} ���� �޼�!");
            // ���߿� �� ������ UI �ִϸ��̼� ���� ȣ���� �� �ֽ��ϴ�.
        }
        else
        {
            Debug.Log("����ġ�� �ö����� �������� ���� �ʾҽ��ϴ�.");
        }
    }
}