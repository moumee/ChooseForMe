// LeaderboardManager.cs
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private FirebaseFirestore _db;

    async void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;

        // --- 테스트를 위한 호출 ---
        Debug.Log("리더보드 데이터 조회를 시작합니다...");
        List<RankerData> top5 = await GetTopRankersAsync(5);

        if (top5.Count > 0)
        {
            Debug.Log("--- 상위 5위 랭킹 ---");
            foreach (var ranker in top5)
            {
                Debug.Log($"{ranker.Rank}위: {ranker.Nickname} ({ranker.Score}점)");
            }
        }
        else
        {
            Debug.Log("리더보드 데이터가 없거나 조회에 실패했습니다.");
        }
    }

    /// <summary>
    /// Firestore에서 상위 랭커 데이터를 가져옵니다.
    /// </summary>
    /// <param name="count">가져올 랭커의 수</param>
    /// <returns>RankerData 리스트</returns>
    public async Task<List<RankerData>> GetTopRankersAsync(int count)
    {
        Query topRankersQuery = _db.Collection("users")
                                   .OrderByDescending("score")
                                   .Limit(count);

        List<RankerData> rankerList = new List<RankerData>();

        try
        {
            QuerySnapshot snapshot = await topRankersQuery.GetSnapshotAsync();
            int rank = 1;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                // Firestore 문서에 필드가 없는 경우를 대비한 안전한 처리
                string nickname = document.ContainsField("nickname") ? document.GetValue<string>("nickname") : "이름없음";
                int score = document.ContainsField("score") ? document.GetValue<int>("score") : 0;

                rankerList.Add(new RankerData { Rank = rank, Nickname = nickname, Score = score });
                rank++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"리더보드 데이터 조회 실패: {e}");
        }

        return rankerList;
    }
}