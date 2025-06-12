// MyPollsUI.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine.UI;

public class MyPollsUI : MonoBehaviour
{
    [Header("Manager Connection")]
    [SerializeField] private PollDataManager _pollDataManager;

    [Header("UI & Prefab")]
    [SerializeField] private Transform _pollListContainer;   // ��ǥ ī����� ������ �θ� �����̳�
    [SerializeField] private PollDisplay _pollCardPrefab;    // ��ǥ ī�� UI ������

    [SerializeField] private Button createVoteButton;

    private void Awake()
    {
        createVoteButton.onClick.AddListener(()=>{PanelManager.Instance.EnablePanel(PanelType.CreateVote);});
    }

    // '���� �ø� ��ǥ' UI�� Ȱ��ȭ�� ������ ����� ���ΰ�ħ�մϴ�.
    private async void OnEnable()
    {
        await RefreshMyPolls();
    }

    /// <summary>
    /// ���� �ø� ��ǥ �����͸� �ҷ��� ȭ�鿡 UI�� �����մϴ�.
    /// </summary>
    public async Task RefreshMyPolls()
    {
        if (_pollDataManager == null || _pollCardPrefab == null || _pollListContainer == null)
        {
            Debug.LogError("�ʼ� ������Ʈ�� Inspector�� ������� �ʾҽ��ϴ�.");
            return;
        }

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("�α��εǾ� ���� �ʾ� ���� �ø� ��ǥ�� �� �� �����ϴ�.");
            return;
        }

        // 1. ���� ��� ����
        foreach (Transform child in _pollListContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. PollDataManager���� "���� ����" ��ǥ ��� ������ ��û
        List<PollData> myPolls = await _pollDataManager.GetPollsByCreatorAsync(user.UserId, 30); // �ִ� 30��

        if (myPolls == null || myPolls.Count == 0)
        {
            Debug.Log("���� �ø� ��ǥ�� �����ϴ�.");
            // TODO: "�ۼ��� ��ǥ�� �����ϴ�" �ؽ�Ʈ UI ǥ��
            return;
        }

        // 3. �޾ƿ� ������� UI ī�� ����
        foreach (PollData pollData in myPolls)
        {
            PollDisplay newCard = Instantiate(_pollCardPrefab, _pollListContainer);
            newCard.SetData(pollData);
        }
    }
}