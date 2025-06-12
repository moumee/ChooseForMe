// StartupManager.cs (최종 버전)
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage; // Storage도 관리하도록 추가
using System;

public class StartupManager : MonoBehaviour
{
    [Header("핵심 서비스")]
    [SerializeField] private FirebaseInitializer _firebaseInitializer;
    [SerializeField] private FirebaseAnonymousLogin _firebaseLogin;

    [Header("초기화 대상 매니저")]
    [SerializeField] private ProfileManager _profileManager;
    [SerializeField] private LeaderboardManager _leaderboardManager;
    [SerializeField] private PollDataManager _pollDataManager;
    [SerializeField] private CreateVoteManager _createVoteManager;

    // ... 다른 매니저들도 여기에 추가 ...

    [Header("관리할 UI 패널")]
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _mainPanel;

    private async void Start()
    {
        ProfileManager.OnProfileInitialized += ActivateMainPanel;

        _loadingPanel.SetActive(true);
        _mainPanel.SetActive(false);
        if (_profileManager.nicknameSetupUI != null) _profileManager.nicknameSetupUI.SetActive(false);

        await RunInitializationSequence();
    }

    private void OnDestroy()
    {
        ProfileManager.OnProfileInitialized -= ActivateMainPanel;
    }

    private async Task RunInitializationSequence()
    {
        try
        {
            // 1. Firebase 핵심 앱 초기화
            bool isInitialized = await _firebaseInitializer.InitializeFirebaseAsync();
            if (!isInitialized) throw new Exception("Firebase 초기화 실패.");

            // 2. Firebase 인스턴스를 여기서 단 한번만 생성
            var firestore = FirebaseFirestore.DefaultInstance;
            var auth = FirebaseAuth.DefaultInstance;
            var storage = FirebaseStorage.DefaultInstance;

            // 3. 다른 모든 매니저들에게 인스턴스를 전달하며 초기화
            _profileManager.Initialize(firestore);
            _leaderboardManager.Initialize(firestore);
            _pollDataManager.Initialize(firestore);
            _createVoteManager.Initialize(firestore, storage, auth);

            // 4. 익명 로그인 실행
            FirebaseUser user = await _firebaseLogin.SignInAnonymouslyAsync(auth);
            if (user == null) throw new Exception("로그인 실패.");

            // 5. 사용자 프로필 확인
            _profileManager.SetUser(user);
            bool isNewUser = await _profileManager.CheckAndCreateProfile();

            _loadingPanel.SetActive(false);

            if (isNewUser)
            {
                _profileManager.nicknameSetupUI.SetActive(true);
            }
            else
            {
                ActivateMainPanel();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[StartupManager] 초기화 시퀀스 중 오류 발생: {e.Message}");
            // TODO: 오류 발생 시 UI 처리
        }
    }

    private void ActivateMainPanel()
    {
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        if (_profileManager.nicknameSetupUI != null) _profileManager.nicknameSetupUI.SetActive(false);
        if (_mainPanel != null) _mainPanel.SetActive(true);
    }
}