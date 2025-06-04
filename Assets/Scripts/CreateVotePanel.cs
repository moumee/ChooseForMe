using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateVotePanel : MonoBehaviour
{
    [SerializeField] private Button submitButton;

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmitButtonClick);
    }

    private void OnSubmitButtonClick()
    {
        PanelManager.Instance.EnablePanel(PanelType.Home);
        PanelManager.Instance.ShowPopup("등록이 완료되었습니다.");
    }
}
