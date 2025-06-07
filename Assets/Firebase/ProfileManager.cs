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
    /// 신규 사용자의 초기 닉네임 설정이 완료되었을 때 발생하는 이벤트입니다.
    /// StartupManager가 이 이벤트를 구독하여 메인 패널을 활성화합니다.
    /// </summary>
    public static event Action OnProfileInitialized;

    /// <summary>
    /// 레벨업 결과를 담아 반환하기 위한 구조체입니다.
    /// </summary>
    public struct LevelUpResult
    {
        public bool DidLevelUp;   // 레벨업을 했는가?
        public int NewLevel;      // 새로운 레벨
        public int LevelsGained;  // 몇 레벨이나 올랐는가?
    }

    // --- Inspector-Visible Fields ---

    [Header("UI 연결")]
    [Tooltip("닉네임 설정 UI의 부모 패널 오브젝트")]
    public GameObject nicknameSetupUI;
    [Tooltip("닉네임 입력을 위한 TextMeshPro InputField")]
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
    /// 외부(예: StartupManager)에서 현재 로그인된 사용자를 설정해주는 메소드입니다.
    /// </summary>
    /// <param name="user">Firebase Auth를 통해 로그인된 사용자 객체</param>
    public void SetUser(FirebaseUser user)
    {
        if (user != null)
        {
            _uid = user.UserId;
        }
    }

    /// <summary>
    /// 사용자의 프로필이 Firestore에 존재하는지 확인하고, 없으면 기본값으로 생성합니다.
    /// </summary>
    /// <returns>신규 유저인 경우 true, 기존 유저인 경우 false를 반환합니다.</returns>
    public async Task<bool> CheckAndCreateProfile()
    {
        if (string.IsNullOrEmpty(_uid)) throw new Exception("ProfileManager: User/UID가 설정되지 않았습니다.");

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);
        DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            Debug.Log($"신규 사용자입니다. 기본 프로필을 생성합니다. UID: {_uid}");
            var initialProfile = new Dictionary<string, object>
            {
                { "nickname", $"User_{_uid.Substring(0, 6)}" },
                { "exp", 0 },
                { "score", 0 },
                { "combo", 0 },
                { "level", 1 }, // 레벨 필드 기본값 추가
                { "createdAt", FieldValue.ServerTimestamp }
            };
            await userDocRef.SetAsync(initialProfile);
            return true; // 신규 유저
        }
        return false; // 기존 유저
    }

    /// <summary>
    /// 지정된 양의 경험치를 추가하고, 레벨업을 확인하여 모든 변경사항을 트랜잭션으로 한 번에 적용합니다.
    /// </summary>
    /// <param name="expToAdd">추가할 경험치 양</param>
    /// <returns>레벨업 결과 정보</returns>
    public async Task<LevelUpResult> AddExperienceAsync(int expToAdd)
    {
        if (string.IsNullOrEmpty(_uid)) throw new Exception("ProfileManager: User/UID가 설정되지 않아 경험치를 추가할 수 없습니다.");

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);
        LevelUpResult result = new LevelUpResult { DidLevelUp = false };

        await _db.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(userDocRef);
            if (!snapshot.Exists) throw new Exception("사용자 문서가 존재하지 않습니다.");

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
            Debug.Log($"레벨업! {result.NewLevel} 레벨 달성 ({result.LevelsGained} 레벨 상승)");
        }

        return result;
    }

    /// <summary>
    /// [기능 2] 지정된 양의 점수를 추가합니다.
    public async Task AddScoreAsync(int scoreToAdd)
    {
        if (string.IsNullOrEmpty(_uid)) throw new Exception("UID가 설정되지 않았습니다.");

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);

        // FieldValue.Increment()를 사용하여 점수만 안전하게 증가시킵니다.
        var updates = new Dictionary<string, object>
        {
            { "score", FieldValue.Increment(scoreToAdd) }
        };

        await userDocRef.UpdateAsync(updates);
        Debug.Log($"점수 {scoreToAdd} 추가 완료.");
    }


    /// <summary>
    /// '닉네임 결정' 버튼의 OnClick() 이벤트에 연결할 메소드입니다.
    /// </summary>
    public async void OnConfirmNicknameClicked()
    {
        string newNickname = nicknameInputField.text.Trim();
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("닉네임이 비어 있습니다.");
            return;
        }

        if (string.IsNullOrEmpty(_uid))
        {
            Debug.LogError("UID가 설정되지 않아 닉네임을 변경할 수 없습니다.");
            return;
        }

        DocumentReference userDocRef = _db.Collection("users").Document(_uid);
        var updates = new Dictionary<string, object> { { "nickname", newNickname } };
        await userDocRef.UpdateAsync(updates);

        Debug.Log("프로필 초기 설정 완료. OnProfileInitialized 이벤트를 발생시킵니다.");
        OnProfileInitialized?.Invoke();

        if (nicknameSetupUI != null) nicknameSetupUI.SetActive(false);
    }

    /// <summary>
    /// 특정 UID를 가진 사용자의 닉네임을 가져옵니다.
    /// </summary>
    /// <param name="uid">정보를 조회할 사용자의 UID</param>
    /// <returns>사용자 닉네임. 문서나 필드가 없으면 null을 반환합니다.</returns>
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
                Debug.LogWarning($"해당 UID({uid})의 문서나 nickname 필드가 없습니다.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"닉네임 조회 중 오류 발생: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 특정 UID를 가진 사용자의 레벨을 가져옵니다.
    /// </summary>
    /// <param name="uid">정보를 조회할 사용자의 UID</param>
    /// <returns>사용자 레벨. 문서나 필드가 없으면 -1을 반환합니다.</returns>
    public async Task<int> GetUserLevelAsync(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return -1;

        DocumentReference userDocRef = _db.Collection("users").Document(uid);
        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.ContainsField("level"))
            {
                // Firestore의 number는 long 타입으로 받는 것이 안전합니다.
                return Convert.ToInt32(snapshot.GetValue<long>("level"));
            }
            else
            {
                Debug.LogWarning($"해당 UID({uid})의 문서나 level 필드가 없습니다.");
                return -1;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"레벨 조회 중 오류 발생: {e.Message}");
            return -1;
        }
    }

    /// <summary>
    /// 특정 UID를 가진 사용자의 점수를 가져옵니다.
    /// </summary>
    /// <param name="uid">정보를 조회할 사용자의 UID</param>
    /// <returns>사용자 점수. 문서나 필드가 없으면 -1을 반환합니다.</returns>
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
                Debug.LogWarning($"해당 UID({uid})의 문서나 score 필드가 없습니다.");
                return -1;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"점수 조회 중 오류 발생: {e.Message}");
            return -1;
        }
    }
}