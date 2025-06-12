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
    /// Firestore���� ���� ��Ŀ �����͸� �����ɴϴ�.
    /// </summary>
    /// <param name="count">������ ��Ŀ�� ��</param>
    /// <returns>RankerData ����Ʈ</returns>
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
                // Firestore ������ �ʵ尡 ���� ��츦 ����� ������ ó��
                string nickname = document.ContainsField("nickname") ? document.GetValue<string>("nickname") : "�̸�����";
                int score = document.ContainsField("score") ? document.GetValue<int>("score") : 0;

                rankerList.Add(new RankerData { Rank = rank, Nickname = nickname, Score = score });
                rank++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�������� ������ ��ȸ ����: {e}");
        }

        return rankerList;
    }
}