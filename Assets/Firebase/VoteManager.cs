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
    /// ������� ��ǥ�� ����ϰ�, �ش� ��ǥ�� ���� ī���͸� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="pollId">��ǥ�� ���� ID</param>
    /// <param name="selectedOptionIndex">����ڰ� ������ �ɼ��� �ε��� (0 �Ǵ� 1)</param>
    public async Task SubmitVote(string pollId, int selectedOptionIndex)
    {
        FirebaseUser user = _auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("�α��εǾ� ���� �ʾ� ��ǥ�� �� �����ϴ�.");
            return;
        }

        // --- [����] �� ���� �ٸ� ������ ���ÿ� ������Ʈ�ϱ� ���� ���� �غ� ---
        // 1. ��ǥ �Խñ� ��ü�� ���� ���� (ī���͸� ������Ʈ�ϱ� ����)
        DocumentReference pollDocRef = _db.Collection("polls").Document(pollId);

        // 2. �� ��ǥ�� ���� ���� ��ǥ ��� ���� ���� (���� ��� ��ǥ�ߴ��� ����ϱ� ����)
        DocumentReference myVoteRef = pollDocRef.Collection("votes").Document(user.UserId);

        try
        {
            // --- [�߰�] WriteBatch�� ����Ͽ� ���� ���� �۾��� �ϳ��� ���� ---
            WriteBatch batch = _db.StartBatch();

            // �۾� 1: ���� ��ǥ ��� ����
            var voteData = new Dictionary<string, object>
            {
                { "choice", selectedOptionIndex },
                { "votedAt", FieldValue.ServerTimestamp }
            };
            batch.Set(myVoteRef, voteData);

            // �۾� 2: ��ǥ �Խñ��� ī���� ������Ʈ
            // ������ �ɼǿ� ���� ������Ʈ�� �ʵ� �̸��� ����
            string counterFieldToUpdate = selectedOptionIndex == 0 ? "option1Votes" : "option2Votes";

            var pollUpdates = new Dictionary<string, object>
            {
                // ������ �ɼ��� ī���� 1 ����
                { counterFieldToUpdate, FieldValue.Increment(1) },
                // ��ü ��ǥ �� ī���� 1 ����
                { "totalVoteCount", FieldValue.Increment(1) }
            };
            batch.Update(pollDocRef, pollUpdates);

            // --- [����] ����� ��� �۾��� �� ���� ���� ---
            await batch.CommitAsync();

            Debug.Log($"��ǥ �� ���� ������Ʈ �Ϸ�! Poll ID: {pollId}, ����: {selectedOptionIndex}");
        }
        catch (Exception e)
        {
            Debug.LogError($"��ǥ ���� �� ���� �� ���� �߻�: {e.Message}");
        }
    }
}