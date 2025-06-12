using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyPagePanel : MonoBehaviour
{
    [SerializeField] private Button createVoteButton;

    private void Awake()
    {
        createVoteButton.onClick.AddListener((() => {PanelManager.Instance.EnablePanel(PanelType.CreateVote);}));
    }
}
