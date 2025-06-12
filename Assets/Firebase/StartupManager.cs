// StartupManager.cs (���� ����)
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage; // Storage�� �����ϵ��� �߰�
using System;

public class StartupManager : MonoBehaviour
{
    [Header("�ٽ� ����")]
    [SerializeField] private FirebaseInitializer _firebaseInitializer;
    [SerializeField] private FirebaseAnonymousLogin _firebaseLogin;

    [Header("�ʱ�ȭ ��� �Ŵ���")]
    [SerializeField] private ProfileManager _profileManager;
    [SerializeField] private LeaderboardManager _leaderboardManager;
    [SerializeField] private PollDataManager _pollDataManager;
    [SerializeField] private CreateVoteManager _createVoteManager;

    // ... �ٸ� �Ŵ����鵵 ���⿡ �߰� ...

    [Header("������ UI �г�")]
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
            // 1. Firebase �ٽ� �� �ʱ�ȭ
            bool isInitialized = await _firebaseInitializer.InitializeFirebaseAsync();
            if (!isInitialized) throw new Exception("Firebase �ʱ�ȭ ����.");

            // 2. Firebase �ν��Ͻ��� ���⼭ �� �ѹ��� ����
            var firestore = FirebaseFirestore.DefaultInstance;
            var auth = FirebaseAuth.DefaultInstance;
            var storage = FirebaseStorage.DefaultInstance;

            // 3. �ٸ� ��� �Ŵ����鿡�� �ν��Ͻ��� �����ϸ� �ʱ�ȭ
            _profileManager.Initialize(firestore);
            _leaderboardManager.Initialize(firestore);
            _pollDataManager.Initialize(firestore);
            _createVoteManager.Initialize(firestore, storage, auth);

            // 4. �͸� �α��� ����
            FirebaseUser user = await _firebaseLogin.SignInAnonymouslyAsync(auth);
            if (user == null) throw new Exception("�α��� ����.");

            // 5. ����� ������ Ȯ��
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
            Debug.LogError($"[StartupManager] �ʱ�ȭ ������ �� ���� �߻�: {e.Message}");
            // TODO: ���� �߻� �� UI ó��
        }
    }

    private void ActivateMainPanel()
    {
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        if (_profileManager.nicknameSetupUI != null) _profileManager.nicknameSetupUI.SetActive(false);
        if (_mainPanel != null) _mainPanel.SetActive(true);
    }
}