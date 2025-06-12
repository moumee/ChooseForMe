using UnityEngine;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class PollDataManager : MonoBehaviour
{
    private FirebaseFirestore _db;
    //private Dictionary<string, PollData> _pollCache = new Dictionary<string, PollData>();

    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
    }

    //public async Task<PollData> GetPollDataAsync(string pollId)
    //{
    //    if (string.IsNullOrEmpty(pollId)) return null;

    //    // 캐시 확인 로직은 동일
    //    if (_pollCache.ContainsKey(pollId))
    //    {
    //        return _pollCache[pollId];
    //    }

    //    DocumentReference docRef = _db.Collection("polls").Document(pollId);
    //    try
    //    {
    //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

    //        // [수정] 헬퍼 메소드를 호출하여 변환 작업을 위임
    //        PollData pollData = ConvertDocumentToPollData(snapshot);

    //        if (pollData != null)
    //        {
    //            // 캐시에 저장
    //            _pollCache[pollId] = pollData;
    //            return pollData;
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"해당 ID의 투표를 찾을 수 없거나 변환에 실패했습니다: {pollId}");
    //            return null;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"투표 데이터 조회 실패: {e.Message}");
    //        return null;
    //    }
    //}

    /// <summary>
    /// [테스트용] polls 컬렉션의 모든 문서를 한 번에 가져옵니다.
    /// 주의: 실제 앱에서는 절대 사용하지 마세요.
    /// </summary>
    /// <returns>모든 투표 데이터의 리스트</returns>
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
            Debug.Log($"{pollList.Count}개의 투표를 모두 가져왔습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"모든 투표 조회 실패: {e.Message}");
        }

        return pollList;
    }

    /// <summary>
    /// Firestore의 DocumentSnapshot을 C#의 PollData 객체로 변환합니다.
    /// </summary>
    /// <param name="snapshot">변환할 Firestore 문서 스냅샷</param>
    /// <returns>변환된 PollData 객체. 스냅샷이 없으면 null을 반환합니다.</returns>
    private PollData ConvertDocumentToPollData(DocumentSnapshot snapshot)
    {
        // 스냅샷이 존재하지 않으면 변환할 수 없으므로 null 반환
        if (!snapshot.Exists) return null;

        // 문서 데이터를 C#의 Dictionary 형태로 변환
        Dictionary<string, object> data = snapshot.ToDictionary();

        // PollData 객체를 생성하고, 각 필드를 안전하게 채워 넣습니다.
        PollData pollData = new PollData
        {
            // 문서의 ID는 스냅샷 자체에서 가져옵니다.
            Id = snapshot.Id,

            // 필드가 존재하지 않을 경우를 대비하여 기본값을 설정합니다.
            Question = data.ContainsKey("question") ? (string)data["question"] : "질문 없음",
            CreatorNickname = data.ContainsKey("creatorNickname") ? (string)data["creatorNickname"] : "익명",

            // Firestore의 숫자는 long 타입으로 받는 것이 안전합니다.
            TotalVoteCount = data.ContainsKey("totalVoteCount") ? Convert.ToInt64(data["totalVoteCount"]) : 0L,
            Option1Votes = data.ContainsKey("option1Votes") ? Convert.ToInt64(data["option1Votes"]) : 0L,
            Option2Votes = data.ContainsKey("option2Votes") ? Convert.ToInt64(data["option2Votes"]) : 0L,

            // Firestore의 배열(Array)은 List<object>로 오므로, 각 항목을 string으로 변환합니다.
            Options = data.ContainsKey("options") ? ((List<object>)data["options"]).ConvertAll(item => item.ToString()) : new List<string>(),

            // Firestore의 맵(Map)은 Dictionary<string, object>로 오므로, 변환 헬퍼를 사용합니다.
            OptionImages = ConvertObjectDictToStringDict(data.ContainsKey("optionImages") ? (Dictionary<string, object>)data["optionImages"] : null)
        };

        return pollData;
    }

    /// <summary>
    /// Dictionary<string, object>를 Dictionary<string, string>으로 변환하는 헬퍼 메소드
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