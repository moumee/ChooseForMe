// MyPollsUI.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;

public class MyPollsUI : MonoBehaviour
{
    [Header("Manager Connection")]
    [SerializeField] private PollDataManager _pollDataManager;

    [Header("UI & Prefab")]
    [SerializeField] private Transform _pollListContainer;   // 투표 카드들이 생성될 부모 컨테이너
    [SerializeField] private PollDisplay _pollCardPrefab;    // 투표 카드 UI 프리팹

    // '내가 올린 투표' UI가 활성화될 때마다 목록을 새로고침합니다.
    private async void OnEnable()
    {
        await RefreshMyPolls();
    }

    /// <summary>
    /// 내가 올린 투표 데이터를 불러와 화면에 UI로 생성합니다.
    /// </summary>
    public async Task RefreshMyPolls()
    {
        if (_pollDataManager == null || _pollCardPrefab == null || _pollListContainer == null)
        {
            Debug.LogError("필수 컴포넌트가 Inspector에 연결되지 않았습니다.");
            return;
        }

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("로그인되어 있지 않아 내가 올린 투표를 볼 수 없습니다.");
            return;
        }

        // 1. 기존 목록 삭제
        foreach (Transform child in _pollListContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. PollDataManager에게 "내가 만든" 투표 목록 데이터 요청
        List<PollData> myPolls = await _pollDataManager.GetPollsByCreatorAsync(user.UserId, 30); // 최대 30개

        if (myPolls == null || myPolls.Count == 0)
        {
            Debug.Log("내가 올린 투표가 없습니다.");
            // TODO: "작성한 투표가 없습니다" 텍스트 UI 표시
            return;
        }

        // 3. 받아온 목록으로 UI 카드 생성
        foreach (PollData pollData in myPolls)
        {
            PollDisplay newCard = Instantiate(_pollCardPrefab, _pollListContainer);
            newCard.SetData(pollData);
        }
    }
}