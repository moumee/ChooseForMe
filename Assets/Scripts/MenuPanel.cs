using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Button myVoteButton;

    [SerializeField] private Button otherVoteButton;
    
    void Awake()
    {
        myVoteButton.onClick.AddListener(OnMyVoteButtonClick);
        otherVoteButton.onClick.AddListener(OnOtherVoteButtonClick);
    }

    void OnMyVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.MyVote);
    }

    void OnOtherVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.OtherVote);
    }
}
