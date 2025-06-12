// LeaderboardUI.cs (������ ���� ��� ����)
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Manager Connection")]
    [SerializeField] private LeaderboardManager _leaderboardManager;

    [Header("UI Slots")]
    [Tooltip("�̸� ������ 5���� ���� ǥ�� UI ������ ���⿡ �����մϴ�.")]
    [SerializeField] private List<RankEntryUI> _rankEntries; // 5���� ����

    // �������� UI�� Ȱ��ȭ�� ������ ��ŷ�� ���ΰ�ħ�մϴ�.
    private async void OnEnable()
    {
        await RefreshLeaderboard();
    }

    /// <summary>
    /// �������� �����͸� �ҷ��� �̸� �غ�� UI ���Կ� ä���ֽ��ϴ�.
    /// </summary>
    public async Task RefreshLeaderboard()
    {
        if (_leaderboardManager == null || _rankEntries == null || _rankEntries.Count == 0)
        {
            Debug.LogError("�ʼ� ������Ʈ�� Inspector�� ������� �ʾҽ��ϴ�.");
            return;
        }

        // 1. LeaderboardManager���� ���� 5���� ������ ��û
        List<RankerData> topRankers = await _leaderboardManager.GetTopRankersAsync(5);

        // 2. �޾ƿ� ��Ŀ �����͸�ŭ ������ ���� UI ���Կ� ������ ä���
        for (int i = 0; i < _rankEntries.Count; i++)
        {
            // 2-1. ���� ����(i)�� �ش��ϴ� ��Ŀ �����Ͱ� �ִ��� Ȯ��
            if (i < topRankers.Count)
            {
                // �����Ͱ� ������ UI ������ Ȱ��ȭ�ϰ� ������ ����
                _rankEntries[i].gameObject.SetActive(true);
                _rankEntries[i].SetData(topRankers[i]);
            }
            else
            {
                // ��Ŀ �����Ͱ� 5�� �̸��� ���, ���� UI ������ ��Ȱ��ȭ
                _rankEntries[i].gameObject.SetActive(false);
            }
        }
    }
}