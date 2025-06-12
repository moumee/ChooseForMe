using UnityEngine;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class PollDataManager : MonoBehaviour
{
    private FirebaseFirestore _db;
    //private Dictionary<string, PollData> _pollCache = new Dictionary<string, PollData>();

    public void Initialize(FirebaseFirestore db)
    {
        _db = db;
    }

    //public async Task<PollData> GetPollDataAsync(string pollId)
    //{
    //    if (string.IsNullOrEmpty(pollId)) return null;

    //    // ĳ�� Ȯ�� ������ ����
    //    if (_pollCache.ContainsKey(pollId))
    //    {
    //        return _pollCache[pollId];
    //    }

    //    DocumentReference docRef = _db.Collection("polls").Document(pollId);
    //    try
    //    {
    //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

    //        // [����] ���� �޼ҵ带 ȣ���Ͽ� ��ȯ �۾��� ����
    //        PollData pollData = ConvertDocumentToPollData(snapshot);

    //        if (pollData != null)
    //        {
    //            // ĳ�ÿ� ����
    //            _pollCache[pollId] = pollData;
    //            return pollData;
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"�ش� ID�� ��ǥ�� ã�� �� ���ų� ��ȯ�� �����߽��ϴ�: {pollId}");
    //            return null;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"��ǥ ������ ��ȸ ����: {e.Message}");
    //        return null;
    //    }
    //}

    /// <summary>
    /// [�׽�Ʈ��] polls �÷����� ��� ������ �� ���� �����ɴϴ�.
    /// ����: ���� �ۿ����� ���� ������� ������.
    /// </summary>
    /// <returns>��� ��ǥ �������� ����Ʈ</returns>
    public async Task<List<PollData>> GetAllPollsAsync()
    {
        Query allPollsQuery = _db.Collection("polls").OrderByDescending("createdAt");
        List<PollData> pollList = new List<PollData>();

        try
        {
            QuerySnapshot snapshot = await allPollsQuery.GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                PollData pollData = ConvertDocumentToPollData(document);
                if (pollData != null)
                {
                    pollList.Add(pollData);
                }
            }
            Debug.Log($"{pollList.Count}���� ��ǥ�� ��� �����Խ��ϴ�.");
        }
        catch (Exception e)
        {
            Debug.LogError($"��� ��ǥ ��ȸ ����: {e.Message}");
        }

        return pollList;
    }

    /// <summary>
    /// Ư�� ����ڰ� ������ ��ǥ ����� �ֽż����� �����ɴϴ�.
    /// </summary>
    /// <param name="creatorUid">��ǥ�� ������ ������� UID</param>
    /// <param name="limit">������ �ִ� ����</param>
    /// <returns>�ش� ����ڰ� ���� ��ǥ �������� ����Ʈ</returns>
    public async Task<List<PollData>> GetPollsByCreatorAsync(string creatorUid, int limit)
    {
        if (string.IsNullOrEmpty(creatorUid)) return new List<PollData>();

        // 1. ���� ����: 'polls' �÷��ǿ��� 'creatorUid' �ʵ尡 ��ġ�ϴ� ������ ���͸��մϴ�.
        Query myPollsQuery = _db.Collection("polls")
                                  .WhereEqualTo("creatorUid", creatorUid) // creatorUid�� ��ġ�ϴ� �͸�
                                  .OrderByDescending("createdAt")      // �ֽż����� ����
                                  .Limit(limit);                       // ������ ������ŭ�� ��������

        List<PollData> pollList = new List<PollData>();
        try
        {
            QuerySnapshot snapshot = await myPollsQuery.GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
            {
                pollList.Add(ConvertDocumentToPollData(document));
            }
            Debug.Log($"'{creatorUid}' ����ڰ� ���� ��ǥ {pollList.Count}���� ã�ҽ��ϴ�.");
        }
        catch (Exception e)
        {
            Debug.LogError($"���� �ø� ��ǥ ��ȸ ����: {e.Message}");
            // �� ������ Firestore '���� ����(Composite Index)'�� �ʿ��ϴٴ� ������ �߻���ų �� �ֽ��ϴ�.
            // ���� �߻� ��, �ֿܼ� ��Ÿ���� URL�� Ŭ���Ͽ� ������ �����ؾ� �մϴ�.
        }
        return pollList;
    }



    /// <summary>
    /// Firestore�� DocumentSnapshot�� C#�� PollData ��ü�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="snapshot">��ȯ�� Firestore ���� ������</param>
    /// <returns>��ȯ�� PollData ��ü. �������� ������ null�� ��ȯ�մϴ�.</returns>
    private PollData ConvertDocumentToPollData(DocumentSnapshot snapshot)
    {
        // �������� �������� ������ ��ȯ�� �� �����Ƿ� null ��ȯ
        if (!snapshot.Exists) return null;

        // ���� �����͸� C#�� Dictionary ���·� ��ȯ
        Dictionary<string, object> data = snapshot.ToDictionary();

        // PollData ��ü�� �����ϰ�, �� �ʵ带 �����ϰ� ä�� �ֽ��ϴ�.
        PollData pollData = new PollData
        {
            // ������ ID�� ������ ��ü���� �����ɴϴ�.
            Id = snapshot.Id,

            // �ʵ尡 �������� ���� ��츦 ����Ͽ� �⺻���� �����մϴ�.
            Question = data.ContainsKey("question") ? (string)data["question"] : "���� ����",
            CreatorNickname = data.ContainsKey("creatorNickname") ? (string)data["creatorNickname"] : "�͸�",

            // Firestore�� ���ڴ� long Ÿ������ �޴� ���� �����մϴ�.
            TotalVoteCount = data.ContainsKey("totalVoteCount") ? Convert.ToInt64(data["totalVoteCount"]) : 0L,
            Option1Votes = data.ContainsKey("option1Votes") ? Convert.ToInt64(data["option1Votes"]) : 0L,
            Option2Votes = data.ContainsKey("option2Votes") ? Convert.ToInt64(data["option2Votes"]) : 0L,

            // Firestore�� �迭(Array)�� List<object>�� ���Ƿ�, �� �׸��� string���� ��ȯ�մϴ�.
            Options = data.ContainsKey("options") ? ((List<object>)data["options"]).ConvertAll(item => item.ToString()) : new List<string>(),

            // Firestore�� ��(Map)�� Dictionary<string, object>�� ���Ƿ�, ��ȯ ���۸� ����մϴ�.
            OptionImages = ConvertObjectDictToStringDict(data.ContainsKey("optionImages") ? (Dictionary<string, object>)data["optionImages"] : null)
        };

        return pollData;
    }

    /// <summary>
    /// Dictionary<string, object>�� Dictionary<string, string>���� ��ȯ�ϴ� ���� �޼ҵ�
    /// </summary>
    private Dictionary<string, string> ConvertObjectDictToStringDict(Dictionary<string, object> objDict)
    {
        var stringDict = new Dictionary<string, string>();
        if (objDict == null) return stringDict;

        foreach (var kvp in objDict)
        {
            if (kvp.Value is string strValue)
            {
                stringDict[kvp.Key] = strValue;
            }
        }
        return stringDict;
    }
}