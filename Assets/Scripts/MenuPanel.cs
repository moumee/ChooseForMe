using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Button myVoteButton;
    
    [SerializeField] private Button createVoteButton;

    
    void Awake()
    {
        myVoteButton.onClick.AddListener(OnMyVoteButtonClick);
        createVoteButton.onClick.AddListener(()=>{PanelManager.Instance.EnablePanel(PanelType.CreateVote);});
    }

    void OnMyVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.MyVote);
    }
    
}
