// LeaderboardUI.cs (고정된 슬롯 사용 버전)
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Manager Connection")]
    [SerializeField] private LeaderboardManager _leaderboardManager;

    [Header("UI Slots")]
    [Tooltip("미리 만들어둔 5개의 순위 표시 UI 슬롯을 여기에 연결합니다.")]
    [SerializeField] private List<RankEntryUI> _rankEntries; // 5개의 슬롯

    // 리더보드 UI가 활성화될 때마다 랭킹을 새로고침합니다.
    private async void OnEnable()
    {
        await RefreshLeaderboard();
    }

    /// <summary>
    /// 리더보드 데이터를 불러와 미리 준비된 UI 슬롯에 채워넣습니다.
    /// </summary>
    public async Task RefreshLeaderboard()
    {
        if (_leaderboardManager == null || _rankEntries == null || _rankEntries.Count == 0)
        {
            Debug.LogError("필수 컴포넌트가 Inspector에 연결되지 않았습니다.");
            return;
        }

        // 1. LeaderboardManager에게 상위 5명의 데이터 요청
        List<RankerData> topRankers = await _leaderboardManager.GetTopRankersAsync(5);

        // 2. 받아온 랭커 데이터만큼 루프를 돌며 UI 슬롯에 데이터 채우기
        for (int i = 0; i < _rankEntries.Count; i++)
        {
            // 2-1. 현재 순위(i)에 해당하는 랭커 데이터가 있는지 확인
            if (i < topRankers.Count)
            {
                // 데이터가 있으면 UI 슬롯을 활성화하고 데이터 설정
                _rankEntries[i].gameObject.SetActive(true);
                _rankEntries[i].SetData(topRankers[i]);
            }
            else
            {
                // 랭커 데이터가 5개 미만일 경우, 남는 UI 슬롯은 비활성화
                _rankEntries[i].gameObject.SetActive(false);
            }
        }
    }
}