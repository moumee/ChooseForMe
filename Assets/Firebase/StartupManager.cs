using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;
using System;

public class StartupManager : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [SerializeField] private FirebaseInitializer _firebaseInitializer;
    [SerializeField] private FirebaseAnonymousLogin _firebaseLogin;
    [SerializeField] private ProfileManager _profileManager;

    [Header("관리할 UI 패널")]
    [SerializeField] private GameObject _loadingPanel; // 초기화/로그인 중 보여줄 로딩 패널
    [SerializeField] private GameObject _mainPanel;    // 모든 과정 완료 후 보여줄 메인 콘텐츠 패널

    private void Start()
    {
        // ProfileManager의 닉네임 설정 완료 이벤트를 구독합니다.
        ProfileManager.OnProfileInitialized += ActivateMainPanel;

        // 초기 UI 상태 설정: 로딩 패널만 활성화
        _loadingPanel.SetActive(true);
        _mainPanel.SetActive(false);
        _profileManager.nicknameSetupUI.SetActive(false);

        // 앱 시작과 동시에 메인 시퀀스를 자동으로 실행합니다.
        RunInitializationSequence();
    }

    private void OnDestroy()
    {
        ProfileManager.OnProfileInitialized -= ActivateMainPanel;
    }

    /// <summary>
    /// Firebase 초기화부터 프로필 확인까지의 과정을 실행합니다.
    /// 더 이상 버튼 클릭으로 호출하지 않으므로 private으로 변경합니다.
    /// </summary>
    private async void RunInitializationSequence()
    {
        try
        {
            // 1. Firebase 초기화
            bool isInitialized = await _firebaseInitializer.InitializeFirebaseAsync();
            if (!isInitialized) throw new Exception("Firebase 초기화 실패.");

            // 2. 익명 로그인
            FirebaseUser user = await _firebaseLogin.SignInAnonymouslyAsync();
            if (user == null) throw new Exception("로그인 실패.");

            // --- 여기부터 수정 ---

            // 3. 사용자 프로필 확인 및 조건부 처리
            // [수정] ProfileManager에 사용자 정보를 먼저 설정해줍니다.
            _profileManager.SetUser(user);

            // [수정] 이제 CheckAndCreateProfile은 인자 없이 호출합니다.
            bool isNewUser = await _profileManager.CheckAndCreateProfile();

            // --- 여기까지 수정 ---

            // 로딩 패널은 이제 역할을 다했으므로 비활성화합니다.
            _loadingPanel.SetActive(false);

            if (isNewUser)
            {
                // 신규 유저 -> 닉네임 설정 UI 활성화
                Debug.Log("[StartupManager] 신규 사용자입니다. 닉네임 설정 UI를 활성화합니다.");
                _profileManager.nicknameSetupUI.SetActive(true);
            }
            else
            {
                // 기존 유저 -> 즉시 메인 패널 활성화
                ActivateMainPanel();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[StartupManager] 초기화 시퀀스 중 오류 발생: {e.Message}");
            _loadingPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 모든 과정이 완료되었을 때 메인 패널을 활성화하는 메소드입니다.
    /// </summary>
    private void ActivateMainPanel()
    {
        Debug.Log("모든 준비 완료. 메인 패널을 활성화합니다.");

        // 다른 패널들은 확실히 비활성화하고 메인 패널만 활성화합니다.
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        if (_profileManager.nicknameSetupUI != null) _profileManager.nicknameSetupUI.SetActive(false);

        _mainPanel.SetActive(true);
    }
}