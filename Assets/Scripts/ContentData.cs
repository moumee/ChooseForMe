using UnityEngine;
using System.Collections.Generic; // List 사용

// 각 콘텐츠의 현재 상태를 나타내는 열거형(enum) 정의
public enum VoteState
{
    InitialVote, // 1. 본 투표
    Prediction, // 2. 예측 투표
    Result // 3. 결과 확인
}

[System.Serializable]
public class ContentData
{
    public string voteTitle;

    // --- 상태 1, 2 공통 정보 ---
    public string itemAName;
    public Sprite itemAImage;
    public string itemBName;
    public Sprite itemBImage;

    // --- 상태 3 정보 ---
    public float itemAResultPercent; // A 항목 최종 득표율 (0.0 ~ 1.0)
    public float itemBResultPercent; // B 항목 최종 득표율
    public List<string> comments; // 댓글 리스트

    // --- 상태 추적 및 사용자 데이터 ---
    [Tooltip("현재 이 콘텐츠의 상태")] public VoteState currentState = VoteState.InitialVote;

    [HideInInspector] // 사용자의 선택은 Inspector에 보일 필요 없음
    public int userVoteChoice = -1; // -1: 미선택, 0: A선택, 1: B선택

    [HideInInspector] public int userPredictionChoice = -1;
}