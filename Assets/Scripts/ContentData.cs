using UnityEngine;
using System.Collections.Generic;

public enum VoteState
{
    InitialVote,
    Prediction,
    Result
}

[System.Serializable]
public class ContentData
{
    // --- 공통 정보 ---
    public string pollId; // Firestore 문서 ID를 저장하기 위한 필드
    public string voteTitle; // 예: "오늘 뭐 먹을까?"

    // --- 선택지 정보 ---
    public string itemAName;
    public string itemAImageUrl; // Sprite -> string 으로 변경
    public string itemBName;
    public string itemBImageUrl; // Sprite -> string 으로 변경

    // --- 결과 정보 ---
    public float itemAResultPercent;
    public float itemBResultPercent;
    public List<string> comments;

    // --- 상태 정보 ---
    public VoteState currentState = VoteState.InitialVote;
    [HideInInspector] public int userVoteChoice = -1;
    [HideInInspector] public int userPredictionChoice = -1;
}