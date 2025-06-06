// ProfilePanelUI.cs
using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 추가
using Firebase.Auth; // 현재 사용자 UID를 가져오기 위해 추가
using System.Threading.Tasks; // 비동기 작업을 위해 추가

public class ProfilePanelUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Header("매니저 연결")]
    // 다른 GameObject에 있는 ProfileManager를 연결하기 위해 [SerializeField] 사용
    [SerializeField] private ProfileManager _profileManager;

    /// <summary>
    /// 이 GameObject가 활성화될 때마다 Unity에 의해 자동으로 호출됩니다.
    /// </summary>
    private async void OnEnable()
    {
        // ProfileManager가 연결되어 있는지, 사용자가 로그인 상태인지 확인
        if (_profileManager == null)
        {
            Debug.LogError("ProfileManager가 연결되지 않았습니다!");
            return;
        }

        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("로그인되어 있지 않습니다.");
            // 필요하다면 텍스트를 "로그인 필요" 등으로 변경
            return;
        }

        // 데이터 불러오기 전, UI를 "로딩 중..." 상태로 변경 (선택 사항)
        _nicknameText.text = "로딩 중...";
        _levelText.text = "Lv. ?";
        _scoreText.text = "? 점";

        // ProfileManager의 메소드를 호출하여 각 데이터를 비동기적으로 가져옵니다.
        // Task.WhenAll을 사용하면 여러 비동기 작업을 병렬로 실행하고 모두 끝날 때까지 기다릴 수 있습니다.
        var nicknameTask = _profileManager.GetUserNicknameAsync(uid);
        var levelTask = _profileManager.GetUserLevelAsync(uid);
        var scoreTask = _profileManager.GetUserScoreAsync(uid);

        await Task.WhenAll(nicknameTask, levelTask, scoreTask);

        // 각 작업의 결과를 가져옵니다.
        string nickname = await nicknameTask;
        int level = await levelTask;
        int score = await scoreTask;

        // 가져온 데이터로 UI 텍스트를 업데이트합니다.
        _nicknameText.text = nickname ?? "이름 없음";
        _levelText.text = $"Lv. {level}";
        _scoreText.text = $"{score} 점";
    }
}