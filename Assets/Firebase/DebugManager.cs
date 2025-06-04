using UnityEngine;
using System.Threading.Tasks;

public class DebugManager : MonoBehaviour
{
    private ProfileManager _profileManager;

    void Start()
    {
        // 같은 GameObject에 있는 ProfileManager 컴포넌트를 찾아와서 변수에 저장합니다.
        _profileManager = GetComponent<ProfileManager>();
        if (_profileManager == null)
        {
            Debug.LogError("DebugManager: ProfileManager를 찾을 수 없습니다! 같은 GameObject에 있는지 확인해주세요.");
        }
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
}