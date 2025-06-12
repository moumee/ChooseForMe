// LeaderboardManager.cs
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private FirebaseFirestore _db;


    public void Initialize(FirebaseFirestore db)
    {
        _db = db;
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