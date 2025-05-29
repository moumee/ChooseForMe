using System;
using UnityEngine;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private float optionA_percentage;

    [SerializeField] private VoteButton optionA;
    [SerializeField] private VoteButton optionB;

    private void Start()
    {
        optionA.Percent = optionA_percentage / 100f;
        optionB.Percent = 1f - optionA.Percent;
        
        optionA.GetButton().onClick.AddListener(OnVoteButtonClick);
        optionB.GetButton().onClick.AddListener(OnVoteButtonClick);
    }

    private void OnVoteButtonClick()
    {
        optionA.OnVoteButtonClick();
        optionB.OnVoteButtonClick();
    }
}
