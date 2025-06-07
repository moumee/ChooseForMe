using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class VoteManager : MonoBehaviour
{
    private FirebaseAuth _auth;
    private FirebaseFirestore _db;

    void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _db = FirebaseFirestore.DefaultInstance;
    }

    /// <summary>
    /// 사용자의 투표를 기록하고, 해당 투표의 집계 카운터를 업데이트합니다.
    /// </summary>
    /// <param name="pollId">투표의 고유 ID</param>
    /// <param name="selectedOptionIndex">사용자가 선택한 옵션의 인덱스 (0 또는 1)</param>
    public async Task SubmitVote(string pollId, int selectedOptionIndex)
    {
        FirebaseUser user = _auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("로그인되어 있지 않아 투표할 수 없습니다.");
            return;
        }

        // --- [수정] 두 개의 다른 문서를 동시에 업데이트하기 위한 참조 준비 ---
        // 1. 투표 게시글 자체의 문서 참조 (카운터를 업데이트하기 위함)
        DocumentReference pollDocRef = _db.Collection("polls").Document(pollId);

        // 2. 이 투표에 대한 나의 투표 기록 문서 참조 (내가 어디에 투표했는지 기록하기 위함)
        DocumentReference myVoteRef = pollDocRef.Collection("votes").Document(user.UserId);

        try
        {
            // --- [추가] WriteBatch를 사용하여 여러 쓰기 작업을 하나로 묶기 ---
            WriteBatch batch = _db.StartBatch();

            // 작업 1: 나의 투표 기록 저장
            var voteData = new Dictionary<string, object>
            {
                { "choice", selectedOptionIndex },
                { "votedAt", FieldValue.ServerTimestamp }
            };
            batch.Set(myVoteRef, voteData);

            // 작업 2: 투표 게시글의 카운터 업데이트
            // 선택한 옵션에 따라 업데이트할 필드 이름을 결정
            string counterFieldToUpdate = selectedOptionIndex == 0 ? "option1Votes" : "option2Votes";

            var pollUpdates = new Dictionary<string, object>
            {
                // 선택한 옵션의 카운터 1 증가
                { counterFieldToUpdate, FieldValue.Increment(1) },
                // 전체 투표 수 카운터 1 증가
                { "totalVoteCount", FieldValue.Increment(1) }
            };
            batch.Update(pollDocRef, pollUpdates);

            // --- [수정] 묶어둔 모든 작업을 한 번에 실행 ---
            await batch.CommitAsync();

            Debug.Log($"투표 및 집계 업데이트 완료! Poll ID: {pollId}, 선택: {selectedOptionIndex}");
        }
        catch (Exception e)
        {
            Debug.LogError($"투표 제출 및 집계 중 오류 발생: {e.Message}");
        }
    }
}