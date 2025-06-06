using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [Header("버튼 연결")]
    public Button showProfilePanelButton;
    [Header("관리할 패널 연결")]
    public GameObject profilePanel; // 활성화시킬 프로필 패널

    private ProfileManager _profileManager;
    private LeaderboardManager _leaderboardManager; // LeaderboardManager 참조 추가

    void Start()
    {
        // 같은 GameObject에 있는 ProfileManager 컴포넌트를 찾아와서 변수에 저장합니다.
        _profileManager = GetComponent<ProfileManager>();
        // LeaderboardManager 컴포넌트 찾아오기
        _leaderboardManager = GetComponent<LeaderboardManager>();
        if (_profileManager == null)
        {
            Debug.LogError("DebugManager: ProfileManager를 찾을 수 없습니다! 같은 GameObject에 있는지 확인해주세요.");
        }
        if (showProfilePanelButton != null) showProfilePanelButton.onClick.AddListener(OnShowProfilePanelClicked);
    }

    /// <summary>
    /// 테스트 버튼의 OnClick() 이벤트에 연결하여 사용할 메소드입니다.
    /// ProfileManager에 경험치 추가를 요청합니다.
    /// </summary>
    public async void OnAddExperienceButtonClicked()
    {
        if (_profileManager == null)
        {
            Debug.LogError("ProfileManager가 연결되지 않아 실행할 수 없습니다.");
            return;
        }

        Debug.Log("테스트: 경험치 25 추가를 요청합니다...");

        // ProfileManager의 메소드를 호출하여 경험치 추가와 레벨업 처리를 한 번에 요청합니다.
        // 추가할 경험치 양(예: 25)은 여기서 자유롭게 조절할 수 있습니다.
        ProfileManager.LevelUpResult result = await _profileManager.AddExperienceAsync(25);

        if (result.DidLevelUp)
        {
            Debug.Log($"레벨업 결과: {result.LevelsGained} 레벨 상승하여 {result.NewLevel} 레벨 달성!");
            // 나중에 이 곳에서 UI 애니메이션 등을 호출할 수 있습니다.
        }
        else
        {
            Debug.Log("경험치는 올랐지만 레벨업은 하지 않았습니다.");
        }
    }

    /// <summary>
    /// 리더보드 새로고침 버튼에 연결할 메소드
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
            Debug.Log("리더보드 데이터를 새로고침합니다...");
            // LeaderboardManager의 메소드를 호출하여 상위 5명의 랭킹을 가져옵니다.
            List<RankerData> top5 = await _leaderboardManager.GetTopRankersAsync(5);

            if (top5.Count > 0)
            {
                Debug.Log("--- 리더보드 Top 5 (새로고침) ---");
                foreach (var ranker in top5)
                {
                    Debug.Log($"{ranker.Rank}위: {ranker.Nickname} ({ranker.Score}점)");
                }
            }
            else
            {
                Debug.Log("리더보드 데이터가 없거나 조회에 실패했습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"리더보드 새로고침 중 오류 발생: {e.Message}");
        }
        
    }

    /// <summary>
    /// 점수 추가 버튼에 연결할 메소드
    public async void OnAddScoreButtonClicked()
    {
        if (_profileManager == null) return;

        Debug.Log("테스트: 점수 5 추가 요청...");
        await _profileManager.AddScoreAsync(5);
    }
    /// <summary>
    /// '프로필 보기' 버튼에 연결할 메소드입니다.
    /// </summary>
    public void OnShowProfilePanelClicked()
    {
        if (profilePanel != null)
        {
            Debug.Log("프로필 패널을 활성화합니다.");
            profilePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Profile Panel이 DebugManager에 연결되지 않았습니다!");
        }
    }
}
