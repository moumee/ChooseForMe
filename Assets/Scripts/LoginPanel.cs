using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] private Button loginButton;

    private void Awake()
    {
        loginButton.onClick.AddListener(OnLoginButtonClick);
    }

    void OnLoginButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.Home);
    }
}
