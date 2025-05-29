using TMPro;
using UnityEngine;

public enum PanelType
{
    Home,
    MyVote,
    OtherVote,
    Menu,
    Login,
    MyPage
}

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject myVotePanel;
    [SerializeField] private GameObject otherVotePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject myPagePanel;
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        DisableAllPanels();
        loginPanel.SetActive(true);
    }

    void Update()
    {
        
    }

    void DisableAllPanels()
    {
        homePanel.SetActive(false);
        myVotePanel.SetActive(false);
        otherVotePanel.SetActive(false);
        menuPanel.SetActive(false);
        loginPanel.SetActive(false);
        // myPagePanel.SetActive(false);
    }

    public void EnablePanel(PanelType panelType)
    {
        DisableAllPanels();
        switch (panelType)
        {
            case PanelType.Home:
                homePanel.SetActive(true);
                break;
            case PanelType.MyVote:
                myVotePanel.SetActive(true);
                break;
            case PanelType.OtherVote:
                otherVotePanel.SetActive(true);
                break;
            case PanelType.Menu:
                menuPanel.SetActive(true);
                break;
            case PanelType.Login:
                loginPanel.SetActive(true);
                break;
            case PanelType.MyPage:
                myPagePanel.SetActive(true);
                break;
            default:
                Debug.LogError("Invalid PanelType");
                break;
        }
    }
}
