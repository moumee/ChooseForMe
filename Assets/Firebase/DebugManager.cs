using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [Header("��ư ����")]
    public Button showProfilePanelButton;
    [Header("������ �г� ����")]
    public GameObject profilePanel; // Ȱ��ȭ��ų ������ �г�

    private ProfileManager _profileManager;
    private LeaderboardManager _leaderboardManager; // LeaderboardManager ���� �߰�

    void Start()
    {
        // ���� GameObject�� �ִ� ProfileManager ������Ʈ�� ã�ƿͼ� ������ �����մϴ�.
        _profileManager = GetComponent<ProfileManager>();
        // LeaderboardManager ������Ʈ ã�ƿ���
        _leaderboardManager = GetComponent<LeaderboardManager>();
        if (_profileManager == null)
        {
            Debug.LogError("DebugManager: ProfileManager�� ã�� �� �����ϴ�! ���� GameObject�� �ִ��� Ȯ�����ּ���.");
        }
        if (showProfilePanelButton != null) showProfilePanelButton.onClick.AddListener(OnShowProfilePanelClicked);
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

    /// <summary>
    /// �������� ���ΰ�ħ ��ư�� ������ �޼ҵ�
    /// </summary>
    public async void OnRefreshLeaderboardClicked()
    {
        if (_leaderboardManager == null)
        {
            Debug.Log("leaderboard null"); 
            return;
        }
        try
        {
            Debug.Log("�������� �����͸� ���ΰ�ħ�մϴ�...");
            // LeaderboardManager�� �޼ҵ带 ȣ���Ͽ� ���� 5���� ��ŷ�� �����ɴϴ�.
            List<RankerData> top5 = await _leaderboardManager.GetTopRankersAsync(5);

            if (top5.Count > 0)
            {
                Debug.Log("--- �������� Top 5 (���ΰ�ħ) ---");
                foreach (var ranker in top5)
                {
                    Debug.Log($"{ranker.Rank}��: {ranker.Nickname} ({ranker.Score}��)");
                }
            }
            else
            {
                Debug.Log("�������� �����Ͱ� ���ų� ��ȸ�� �����߽��ϴ�.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�������� ���ΰ�ħ �� ���� �߻�: {e.Message}");
        }
        
    }

    /// <summary>
    /// ���� �߰� ��ư�� ������ �޼ҵ�
    public async void OnAddScoreButtonClicked()
    {
        if (_profileManager == null) return;

        Debug.Log("�׽�Ʈ: ���� 5 �߰� ��û...");
        await _profileManager.AddScoreAsync(5);
    }
    /// <summary>
    /// '������ ����' ��ư�� ������ �޼ҵ��Դϴ�.
    /// </summary>
    public void OnShowProfilePanelClicked()
    {
        if (profilePanel != null)
        {
            Debug.Log("������ �г��� Ȱ��ȭ�մϴ�.");
            profilePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Profile Panel�� DebugManager�� ������� �ʾҽ��ϴ�!");
        }
    }
}
