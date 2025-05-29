using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour
{
    [SerializeField] private Toggle menuToggle;
    [SerializeField] private Toggle homeToggle;
    [SerializeField] private Toggle myPageToggle;
    
    void Awake()
    {
        menuToggle.onValueChanged.AddListener(OnMenuToggleValueChanged);
        homeToggle.onValueChanged.AddListener(OnHomeToggleValueChanged);
        myPageToggle.onValueChanged.AddListener(OnMyPageToggleValueChanged);
    }

    void OnMenuToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            PanelManager.Instance.EnablePanel(PanelType.Menu);
        }
    }

    void OnHomeToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            PanelManager.Instance.EnablePanel(PanelType.Home);
        }
    }

    void OnMyPageToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            PanelManager.Instance.EnablePanel(PanelType.MyPage);
        }
    }
}
