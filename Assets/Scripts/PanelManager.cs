using TMPro;
using UnityEngine;

public enum PanelType
{
    Home,
    MyVote,
    SearchVote,
    CreateVote,
    Menu,
    Login,
    MyPage
}

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject myVotePanel;
    [SerializeField] private GameObject searchVotePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject myPagePanel;
    [SerializeField] private GameObject createVotePanel;

    [Header("Popup")] 
    [SerializeField] private GameObject popup;
    
    
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
        
        popup.SetActive(false);
        
        // Start as login panel
        DisableAllPanels();
        loginPanel.SetActive(true);
    }
    
    void DisableAllPanels()
    {
        homePanel.SetActive(false);
        myVotePanel.SetActive(false);
        searchVotePanel.SetActive(false);
        createVotePanel.SetActive(false);
        menuPanel.SetActive(false);
        loginPanel.SetActive(false);
        myPagePanel.SetActive(false);
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
            case PanelType.SearchVote:
                searchVotePanel.SetActive(true);
                break;
            case PanelType.CreateVote:
                createVotePanel.SetActive(true);
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

    public void ShowPopup(string message)
    {
        popup.GetComponent<Popup>().SetMessage(message);
        popup.SetActive(true);
    }
}
