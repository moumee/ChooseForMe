using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class ProfileManager : MonoBehaviour
{
    // --- Public Events & Structs ---

    /// <summary>
    /// �ű� ������� �ʱ� �г��� ������ �Ϸ�Ǿ��� �� �߻��ϴ� �̺�Ʈ�Դϴ�.
    /// StartupManager�� �� �̺�Ʈ�� �����Ͽ� ���� �г��� Ȱ��ȭ�մϴ�.
    /// </summary>
    public static event Action OnProfileInitialized;

    /// <summary>
    /// ������ ����� ��� ��ȯ�ϱ� ���� ����ü�Դϴ�.
    /// </summary>
    public struct LevelUpResult
    {
        public bool DidLevelUp;   // �������� �ߴ°�?
        public int NewLevel;      // ���ο� ����
        public int LevelsGained;  // �� �����̳� �ö��°�?
    }

    // --- Inspector-Visible Fields ---

    [Header("UI ����")]
    [Tooltip("�г��� ���� UI�� �θ� �г� ������Ʈ")]
    public GameObject nicknameSetupUI;
    [Tooltip("�г��� �Է��� ���� TextMeshPro InputField")]
    public TMP_InputField nicknameInputField;

    // --- Constants ---

    private const int ExpRequiredForLevelUp = 100;

    // --- Private Fields ---

    private FirebaseFirestore _db;
    private string _uid;

    // --- Unity Lifecycle Methods ---

    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        if (nicknameSetupUI != null) nicknameSetupUI.SetActive(false);
    }

    // --- Public Methods ---

    /// <summary>
    /// �ܺ�(��: StartupManager)���� ���� �α��ε� ����ڸ� �������ִ� �޼ҵ��Դϴ�.
    /// </summary>
    /// <param name="user">Firebase Auth�� ���� �α��ε� ����� ��ü</param>
    public void SetUser(FirebaseUser user)
    {
        if (user != null)
        {
            _uid = user.UserId;
        }
    }

    /// <summary>
    /// ������� �������� Firestore�� �����ϴ��� Ȯ���ϰ�, ������ �⺻������ �����մϴ�.
    /// </summary>
    /// <returns>�ű� ������ ��� true, ���� ������ ��� false�� ��ȯ�մϴ�.</returns>
    public async Task<bool> CheckAndCreateProfile()
    {
        if (string.IsNullOrEmpty(_uid)) throw new Exception("ProfileManager: User/UID�� �������� �ʾҽ��ϴ�.");

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);
        DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            Debug.Log($"�ű� ������Դϴ�. �⺻ �������� �����մϴ�. UID: {_uid}");
            var initialProfile = new Dictionary<string, object>
            {
                { "nickname", $"User_{_uid.Substring(0, 6)}" },
                { "exp", 0 },
                { "score", 0 },
                { "combo", 0 },
                { "level", 1 }, // ���� �ʵ� �⺻�� �߰�
                { "createdAt", FieldValue.ServerTimestamp }
            };
            await userDocRef.SetAsync(initialProfile);
            return true; // �ű� ����
        }
        return false; // ���� ����
    }

    /// <summary>
    /// ������ ���� ����ġ�� �߰��ϰ�, �������� Ȯ���Ͽ� ��� ��������� Ʈ��������� �� ���� �����մϴ�.
    /// </summary>
    /// <param name="expToAdd">�߰��� ����ġ ��</param>
    /// <returns>������ ��� ����</returns>
    public async Task<LevelUpResult> AddExperienceAsync(int expToAdd)
    {
        if (string.IsNullOrEmpty(_uid)) throw new Exception("ProfileManager: User/UID�� �������� �ʾ� ����ġ�� �߰��� �� �����ϴ�.");

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);
        LevelUpResult result = new LevelUpResult { DidLevelUp = false };

        await _db.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(userDocRef);
            if (!snapshot.Exists) throw new Exception("����� ������ �������� �ʽ��ϴ�.");

            long originalLevel = snapshot.GetValue<long>("level");
            long newExp = snapshot.GetValue<long>("exp") + expToAdd;

            long calculatedLevel = originalLevel;
            int levelsGained = 0;

            while (newExp >= ExpRequiredForLevelUp)
            {
                newExp -= ExpRequiredForLevelUp;
                calculatedLevel++;
                levelsGained++;
            }

            var updates = new Dictionary<string, object>
            {
                { "exp", newExp },
                { "level", calculatedLevel }
            };
            transaction.Update(userDocRef, updates);

            if (levelsGained > 0)
            {
                result.DidLevelUp = true;
                result.NewLevel = (int)calculatedLevel;
                result.LevelsGained = levelsGained;
            }
        });

        if (result.DidLevelUp)
        {
            Debug.Log($"������! {result.NewLevel} ���� �޼� ({result.LevelsGained} ���� ���)");
        }

        return result;
    }

    /// <summary>
    /// [��� 2] ������ ���� ������ �߰��մϴ�.
    public async Task AddScoreAsync(int scoreToAdd)
    {
        if (string.IsNullOrEmpty(_uid)) throw new Exception("UID�� �������� �ʾҽ��ϴ�.");

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);

        // FieldValue.Increment()�� ����Ͽ� ������ �����ϰ� ������ŵ�ϴ�.
        var updates = new Dictionary<string, object>
        {
            { "score", FieldValue.Increment(scoreToAdd) }
        };

        await userDocRef.UpdateAsync(updates);
        Debug.Log($"���� {scoreToAdd} �߰� �Ϸ�.");
    }


    /// <summary>
    /// '�г��� ����' ��ư�� OnClick() �̺�Ʈ�� ������ �޼ҵ��Դϴ�.
    /// </summary>
    public async void OnConfirmNicknameClicked()
    {
        string newNickname = nicknameInputField.text.Trim();
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("�г����� ��� �ֽ��ϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(_uid))
        {
            Debug.LogError("UID�� �������� �ʾ� �г����� ������ �� �����ϴ�.");
            return;
        }

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);
        var updates = new Dictionary<string, object> { { "nickname", newNickname } };
        await userDocRef.UpdateAsync(updates);

        Debug.Log("������ �ʱ� ���� �Ϸ�. OnProfileInitialized �̺�Ʈ�� �߻���ŵ�ϴ�.");
        OnProfileInitialized?.Invoke();

        if (nicknameSetupUI != null) nicknameSetupUI.SetActive(false);
    }

    /// <summary>
    /// Ư�� UID�� ���� ������� �г����� �����ɴϴ�.
    /// </summary>
    /// <param name="uid">������ ��ȸ�� ������� UID</param>
    /// <returns>����� �г���. ������ �ʵ尡 ������ null�� ��ȯ�մϴ�.</returns>
    public async Task<string> GetUserNicknameAsync(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return null;

        DocumentReference userDocRef = _db.Collection("users").Document(uid);
        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.ContainsField("nickname"))
            {
                return snapshot.GetValue<string>("nickname");
            }
            else
            {
                Debug.LogWarning($"�ش� UID({uid})�� ������ nickname �ʵ尡 �����ϴ�.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"�г��� ��ȸ �� ���� �߻�: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Ư�� UID�� ���� ������� ������ �����ɴϴ�.
    /// </summary>
    /// <param name="uid">������ ��ȸ�� ������� UID</param>
    /// <returns>����� ����. ������ �ʵ尡 ������ -1�� ��ȯ�մϴ�.</returns>
    public async Task<int> GetUserLevelAsync(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return -1;

        DocumentReference userDocRef = _db.Collection("users").Document(uid);
        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.ContainsField("level"))
            {
                // Firestore�� number�� long Ÿ������ �޴� ���� �����մϴ�.
                return Convert.ToInt32(snapshot.GetValue<long>("level"));
            }
            else
            {
                Debug.LogWarning($"�ش� UID({uid})�� ������ level �ʵ尡 �����ϴ�.");
                return -1;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ��ȸ �� ���� �߻�: {e.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Ư�� UID�� ���� ������� ������ �����ɴϴ�.
    /// </summary>
    /// <param name="uid">������ ��ȸ�� ������� UID</param>
    /// <returns>����� ����. ������ �ʵ尡 ������ -1�� ��ȯ�մϴ�.</returns>
    public async Task<int> GetUserScoreAsync(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return -1;

        DocumentReference userDocRef = _db.Collection("users").Document(uid);
        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.ContainsField("score"))
            {
                return Convert.ToInt32(snapshot.GetValue<long>("score"));
            }
            else
            {
                Debug.LogWarning($"�ش� UID({uid})�� ������ score �ʵ尡 �����ϴ�.");
                return -1;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ��ȸ �� ���� �߻�: {e.Message}");
            return -1;
        }
    }
}