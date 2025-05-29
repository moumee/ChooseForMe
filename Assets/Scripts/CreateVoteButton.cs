using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateVoteButton : MonoBehaviour
{
    private Button createVoteButton;

    private void Awake()
    {
        createVoteButton = GetComponent<Button>();
        createVoteButton.onClick.AddListener(OnClickCreateVoteButton);
    }

    private void OnClickCreateVoteButton()
    {
        PanelManager.Instance.EnablePanel(PanelType.CreateVote);
    }

}
