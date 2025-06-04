using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Button myVoteButton;

    [SerializeField] private Button searchVoteButton;
    
    void Awake()
    {
        myVoteButton.onClick.AddListener(OnMyVoteButtonClick);
        searchVoteButton.onClick.AddListener(OnSearchVoteButtonClick);
    }

    void OnMyVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.MyVote);
    }

    void OnSearchVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.SearchVote);
    }
}
