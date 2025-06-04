using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;
using System;

public class StartupManager : MonoBehaviour
{
    [Header("�ʼ� ������Ʈ")]
    [SerializeField] private FirebaseInitializer _firebaseInitializer;
    [SerializeField] private FirebaseAnonymousLogin _firebaseLogin;
    [SerializeField] private ProfileManager _profileManager;

    [Header("������ UI �г�")]
    [SerializeField] private GameObject _loadingPanel; // �ʱ�ȭ/�α��� �� ������ �ε� �г�
    [SerializeField] private GameObject _mainPanel;    // ��� ���� �Ϸ� �� ������ ���� ������ �г�

    private void Start()
    {
        // ProfileManager�� �г��� ���� �Ϸ� �̺�Ʈ�� �����մϴ�.
        ProfileManager.OnProfileInitialized += ActivateMainPanel;

        // �ʱ� UI ���� ����: �ε� �гθ� Ȱ��ȭ
        _loadingPanel.SetActive(true);
        _mainPanel.SetActive(false);
        _profileManager.nicknameSetupUI.SetActive(false);

        // �� ���۰� ���ÿ� ���� �������� �ڵ����� �����մϴ�.
        RunInitializationSequence();
    }

    private void OnDestroy()
    {
        ProfileManager.OnProfileInitialized -= ActivateMainPanel;
    }

    /// <summary>
    /// Firebase �ʱ�ȭ���� ������ Ȯ�α����� ������ �����մϴ�.
    /// �� �̻� ��ư Ŭ������ ȣ������ �����Ƿ� private���� �����մϴ�.
    /// </summary>
    private async void RunInitializationSequence()
    {
        try
        {
            // 1. Firebase �ʱ�ȭ
            bool isInitialized = await _firebaseInitializer.InitializeFirebaseAsync();
            if (!isInitialized) throw new Exception("Firebase �ʱ�ȭ ����.");

            // 2. �͸� �α���
            FirebaseUser user = await _firebaseLogin.SignInAnonymouslyAsync();
            if (user == null) throw new Exception("�α��� ����.");

            // --- ������� ���� ---

            // 3. ����� ������ Ȯ�� �� ���Ǻ� ó��
            // [����] ProfileManager�� ����� ������ ���� �������ݴϴ�.
            _profileManager.SetUser(user);

            // [����] ���� CheckAndCreateProfile�� ���� ���� ȣ���մϴ�.
            bool isNewUser = await _profileManager.CheckAndCreateProfile();

            // --- ������� ���� ---

            // �ε� �г��� ���� ������ �������Ƿ� ��Ȱ��ȭ�մϴ�.
            _loadingPanel.SetActive(false);

            if (isNewUser)
            {
                // �ű� ���� -> �г��� ���� UI Ȱ��ȭ
                Debug.Log("[StartupManager] �ű� ������Դϴ�. �г��� ���� UI�� Ȱ��ȭ�մϴ�.");
                _profileManager.nicknameSetupUI.SetActive(true);
            }
            else
            {
                // ���� ���� -> ��� ���� �г� Ȱ��ȭ
                ActivateMainPanel();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[StartupManager] �ʱ�ȭ ������ �� ���� �߻�: {e.Message}");
            _loadingPanel.SetActive(true);
        }
    }

    /// <summary>
    /// ��� ������ �Ϸ�Ǿ��� �� ���� �г��� Ȱ��ȭ�ϴ� �޼ҵ��Դϴ�.
    /// </summary>
    private void ActivateMainPanel()
    {
        Debug.Log("��� �غ� �Ϸ�. ���� �г��� Ȱ��ȭ�մϴ�.");

        // �ٸ� �гε��� Ȯ���� ��Ȱ��ȭ�ϰ� ���� �гθ� Ȱ��ȭ�մϴ�.
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        if (_profileManager.nicknameSetupUI != null) _profileManager.nicknameSetupUI.SetActive(false);

        _mainPanel.SetActive(true);
    }
}