using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class CreateVoteManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField titleInput;
    public TMP_InputField option1Input;
    public TMP_InputField option2Input;
    public Button uploadVoteButton;

    // --- Firebase Instances ---
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;

        // 버튼 리스너 할당
        uploadVoteButton.onClick.AddListener(HandleCreateVoteClicked);
    }

    /// <summary>
    /// UI의 투표 생성 버튼 클릭 시 안전하게 비동기 메소드를 호출하는 핸들러
    /// </summary>
    private void HandleCreateVoteClicked()
    {
        // _= 는 "이 비동기 작업이 끝날 때까지 기다릴 필요는 없다"는 의미의 최신 C# 문법입니다.
        // CreateVoteAsync 내부에서 모든 예외 처리를 담당합니다.
        _ = CreateVoteAsync();
    }

    /// <summary>
    /// 텍스트 기반의 투표 문서를 Firestore에 생성하는 핵심 메소드
    /// </summary>
    private async Task CreateVoteAsync()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("로그인되지 않아 투표를 생성할 수 없습니다.");
            return;
        }

        string title = titleInput.text;
        string opt1 = option1Input.text;
        string opt2 = option2Input.text;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(opt1) || string.IsNullOrEmpty(opt2))
        {
            Debug.LogWarning("모든 텍스트 필드를 채워야 합니다.");
            return;
        }

        uploadVoteButton.interactable = false; // 중복 클릭 방지

        try
        {
            // 1. Firestore에 저장할 데이터 구조 생성
            // VoteManager와의 연동을 위해 필수적인 필드들만 포함합니다.
            var pollData = new Dictionary<string, object>
            {
                { "question", title },
                { "options", new List<string> { opt1, opt2 } },
                { "creatorUid", user.UserId },
                { "createdAt", FieldValue.ServerTimestamp },
                { "pollType", "ChoicePoll" }, // 우선 고정
                
                // VoteManager가 사용할 카운터 필드를 0으로 초기화하여 생성
                { "totalVoteCount", 0 },
                { "option1Votes", 0 },
                { "option2Votes", 0 },

                { "isClosed", false }
            };

            // 2. 'polls' 컬렉션에 새로운 문서 추가
            await _db.Collection("polls").AddAsync(pollData);

            Debug.Log("기본 투표가 성공적으로 생성되었습니다!");
            ClearInputFields();
        }
        catch (Exception e)
        {
            Debug.LogError($"투표 생성 실패: {e.Message}");
        }
        finally
        {
            uploadVoteButton.interactable = true; // 버튼 다시 활성화
        }
    }

    /// <summary>
    /// 작업 완료 후 입력 필드를 초기화하는 메소드
    /// </summary>
    private void ClearInputFields()
    {
        titleInput.text = "";
        option1Input.text = "";
        option2Input.text = "";
    }
}