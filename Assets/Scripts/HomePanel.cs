using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class HomePanel : MonoBehaviour
{
    [SerializeField] private Button createVoteButton;

    private void Awake()
    {
        createVoteButton.onClick.AddListener(OnCreateVoteButtonClick);
    }


    private void OnCreateVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.CreateVote);
    }
}
