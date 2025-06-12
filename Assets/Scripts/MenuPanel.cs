using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Button myVoteButton;

    
    void Awake()
    {
        myVoteButton.onClick.AddListener(OnMyVoteButtonClick);
    }

    void OnMyVoteButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.MyVote);
    }
    
}
