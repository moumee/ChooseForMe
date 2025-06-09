using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class CreateVoteManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField option1Input;
    public TMP_InputField option2Input;
    public Button uploadVoteButton;

    // --- Firebase Instances ---
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;

        // ��ư ������ �Ҵ�
        uploadVoteButton.onClick.AddListener(HandleCreateVoteClicked);
    }

    /// <summary>
    /// UI�� ��ǥ ���� ��ư Ŭ�� �� �����ϰ� �񵿱� �޼ҵ带 ȣ���ϴ� �ڵ鷯
    /// </summary>
    private void HandleCreateVoteClicked()
    {
        // _= �� "�� �񵿱� �۾��� ���� ������ ��ٸ� �ʿ�� ����"�� �ǹ��� �ֽ� C# �����Դϴ�.
        // CreateVoteAsync ���ο��� ��� ���� ó���� ����մϴ�.
        _ = CreateVoteAsync();
    }

    /// <summary>
    /// �ؽ�Ʈ ����� ��ǥ ������ Firestore�� �����ϴ� �ٽ� �޼ҵ�
    /// </summary>
    private async Task CreateVoteAsync()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("�α��ε��� �ʾ� ��ǥ�� ������ �� �����ϴ�.");
            return;
        }
        
        string opt1 = option1Input.text;
        string opt2 = option2Input.text;

        if (string.IsNullOrEmpty(opt1) || string.IsNullOrEmpty(opt2))
        {
            Debug.LogWarning("��� �ؽ�Ʈ �ʵ带 ä���� �մϴ�.");
            return;
        }

        uploadVoteButton.interactable = false; // �ߺ� Ŭ�� ����

        try
        {
            // 1. Firestore�� ������ ������ ���� ����
            // VoteManager���� ������ ���� �ʼ����� �ʵ�鸸 �����մϴ�.
            var pollData = new Dictionary<string, object>
            {
                { "options", new List<string> { opt1, opt2 } },
                { "creatorUid", user.UserId },
                { "createdAt", FieldValue.ServerTimestamp },
                { "pollType", "ChoicePoll" }, // �켱 ����
                
                // VoteManager�� ����� ī���� �ʵ带 0���� �ʱ�ȭ�Ͽ� ����
                { "totalVoteCount", 0 },
                { "option1Votes", 0 },
                { "option2Votes", 0 },

                { "isClosed", false }
            };

            // 2. 'polls' �÷��ǿ� ���ο� ���� �߰�
            await _db.Collection("polls").AddAsync(pollData);

            Debug.Log("�⺻ ��ǥ�� ���������� �����Ǿ����ϴ�!");
            ClearInputFields();
        }
        catch (Exception e)
        {
            Debug.LogError($"��ǥ ���� ����: {e.Message}");
        }
        finally
        {
            uploadVoteButton.interactable = true; // ��ư �ٽ� Ȱ��ȭ
        }
    }

    /// <summary>
    /// �۾� �Ϸ� �� �Է� �ʵ带 �ʱ�ȭ�ϴ� �޼ҵ�
    /// </summary>
    private void ClearInputFields()
    {
        option1Input.text = "";
        option2Input.text = "";
    }
}