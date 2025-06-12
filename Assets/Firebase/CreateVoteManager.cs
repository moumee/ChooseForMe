using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class CreateVoteManager : MonoBehaviour
{
    // --- JavaScript �÷����� �Լ� ���� ---
#if UNITY_WEBGL && !UNITY_EDITOR
            [DllImport("__Internal")]
            private static extern void UploadFile(string gameObjectName, string callbackMethodName, string fileType);
#endif

    // --- ������ �׽�Ʈ�� �ʵ� ---
#if UNITY_EDITOR
    [Header("������ �׽�Ʈ�� �̹���")]
    public Texture2D testImage1; // Inspector â���� �׽�Ʈ�� �̹��� ������ ���⿡ ����
    public Texture2D testImage2;
#endif

    [Header("FirebaseManager Connection")]
    [Tooltip("FirebaseManager ������Ʈ�� �����ؾ� �մϴ�.")]
    [SerializeField] private ProfileManager _profileManager;

    [Header("UI Elements")]
    public TMP_InputField titleInput; // [추가] 제목 입력 필드
    public TMP_InputField option1Input;
    public TMP_InputField option2Input;
    public Button image1Button;
    public Button image2Button;
    public Button uploadVoteButton;

    // --- Private Fields ---
    private byte[] _image1Bytes;
    private byte[] _image2Bytes;
    private int _currentPickingOption;

    // --- Firebase Instances ---
    private FirebaseFirestore _db;
    private FirebaseStorage _storage;
    private FirebaseAuth _auth;

    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _storage = FirebaseStorage.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;

        if (_profileManager == null) Debug.LogError("ProfileManager�� ������� �ʾҽ��ϴ�!");
        // ��ư ������ �Ҵ�
        image1Button.onClick.AddListener(() => PickImageForOption(1));
        image2Button.onClick.AddListener(() => PickImageForOption(2));
        uploadVoteButton.onClick.AddListener(HandleCreateVoteClicked);
    }

    /// <summary>
    /// �̹��� ���� ��ư Ŭ�� �� �÷����� �´� ���� ��Ŀ�� ȣ���մϴ�.
    /// </summary>
    void PickImageForOption(int optionNumber)
    {
        _currentPickingOption = optionNumber;

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL ���� ȯ�濡���� JavaScript �÷����� �Լ��� ȣ���մϴ�.
        UploadFile(gameObject.name, "OnImageSelectedFromWebGL", "image/*");
#elif UNITY_EDITOR
        // ������ ȯ�濡���� �׽�Ʈ�� �̹����� �ٷ� byte �迭�� ��ȯ�մϴ�.
        Debug.Log("������ ���: �׽�Ʈ �̹����� ����մϴ�.");
        Texture2D selectedTestImage = (optionNumber == 1) ? testImage1 : testImage2;
        if (selectedTestImage != null)
        {
            byte[] bytes = GetReadableTextureBytes(selectedTestImage);
            ProcessSelectedImageBytes(bytes);
        }
        else
        {
            Debug.LogWarning($"�׽�Ʈ �̹��� {optionNumber}�� �������� �ʾҽ��ϴ�.");
        }
#else
        // ����� �� �ٸ� ȯ�濡���� NativeGallery�� ���� ������ �ʿ��մϴ�.
        Debug.LogWarning("�� �÷��������� �̹��� ��Ŀ ����� �������� �ʽ��ϴ�.");
#endif
    }

    /// <summary>
    /// JavaScript���� ���� ������ �Ϸ�Ǹ� ȣ��� �ݹ� �޼ҵ� (WebGL ����)
    /// </summary>
    public void OnImageSelectedFromWebGL(string base64Data)
    {
        if (string.IsNullOrEmpty(base64Data)) return;

        var parts = base64Data.Split(',');
        if (parts.Length < 2) return;

        byte[] bytes = Convert.FromBase64String(parts[1]);
        ProcessSelectedImageBytes(bytes);
    }

    /// <summary>
    /// ���õ� �̹����� byte �����͸� �������� ó���ϴ� �޼ҵ�
    /// </summary>
    private void ProcessSelectedImageBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return;

        if (_currentPickingOption == 1)
        {
            _image1Bytes = bytes;
            Debug.Log("Option 1 �̹����� �����ͷ� ����Ǿ����ϴ�.");
        }
        else
        {
            _image2Bytes = bytes;
            Debug.Log("Option 2 �̹����� �����ͷ� ����Ǿ����ϴ�.");
        }
        // TODO: ���õ� �̹����� UI�� �̸������ �����ִ� ���� �߰�
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
        string title = titleInput.text;
        string opt1 = option1Input.text;
        string opt2 = option2Input.text;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(opt1) || string.IsNullOrEmpty(opt2))
        {
            Debug.LogWarning("��� �ؽ�Ʈ �ʵ带 ä���� �մϴ�.");
            return;
        }

        uploadVoteButton.interactable = false; // �ߺ� Ŭ�� ����

        try
        {
            // 1. �̹��� ���� ���ε� �� �ۼ��� �г��� ���� ��ȸ
            Task<string> uploadTask1 = (_image1Bytes != null) ? UploadImageAsync(_image1Bytes, "option1") : Task.FromResult<string>(null);
            Task<string> uploadTask2 = (_image2Bytes != null) ? UploadImageAsync(_image2Bytes, "option2") : Task.FromResult<string>(null);
            Task<string> nicknameTask = _profileManager.GetUserNicknameAsync(user.UserId);

            await Task.WhenAll(uploadTask1, uploadTask2, nicknameTask);

            string imageUrl1 = await uploadTask1;
            string imageUrl2 = await uploadTask2;
            string creatorNickname = await nicknameTask ?? "�͸�";

            // 2. ���� ������ ���� ����
            var pollData = new Dictionary<string, object>
            {
                { "question", title }, // [추가]
                { "options", new List<string> { opt1, opt2 } },
                { "creatorUid", user.UserId },
                { "creatorNickname", creatorNickname },
                { "createdAt", FieldValue.ServerTimestamp },
                { "pollType", "ChoicePoll" },
                { "optionImages", new Dictionary<string, string>
                    {
                        { "0", imageUrl1 ?? "" },
                        { "1", imageUrl2 ?? "" }
                    }
                },
                { "totalVoteCount", 0 },
                { "option1Votes", 0 },
                { "option2Votes", 0 },
                { "isClosed", false }
            };

            // 3. 'polls' �÷��ǿ� ���� �߰�
            DocumentReference addedDocRef = await _db.Collection("polls").AddAsync(pollData);
            Debug.Log($"��ǥ�� ���������� �����Ǿ����ϴ�! Poll ID: {addedDocRef.Id}");

            ClearInputFields();
        }
        catch (Exception e)
        {
            Debug.LogError($"��ǥ ���� ����: {e.Message}");
        }
        finally
        {
            uploadVoteButton.interactable = true;
        }
    }

    /// <summary>
    /// �۾� �Ϸ� �� �Է� �ʵ带 �ʱ�ȭ�ϴ� �޼ҵ�
    /// </summary>
    private void ClearInputFields()
    {
        titleInput.text = ""; // [추가]
        option1Input.text = "";
        option2Input.text = "";
        _image1Bytes = null;
        _image2Bytes = null;
    }

    /// <summary>
    /// �̹��� byte �迭�� Firebase Storage�� ���ε��ϰ� �ٿ�ε� URL�� ��ȯ�մϴ�.
    /// </summary>
    private async Task<string> UploadImageAsync(byte[] bytes, string fileNamePart)
    {
        try
        {
            string fileName = $"{fileNamePart}_{Guid.NewGuid()}.png"; // Ȯ���ڴ� PNG�� ���� (Ȥ�� MimeType�� ����)
            StorageReference imageRef = _storage.GetReference($"pollImages/{_auth.CurrentUser.UserId}/{fileName}");
            var metadata = new MetadataChange { ContentType = "image/png" };

            await imageRef.PutBytesAsync(bytes, metadata);
            Uri downloadUri = await imageRef.GetDownloadUrlAsync();
            return downloadUri.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"�̹��� ���ε� ���� ({fileNamePart}): {e.Message}");
            return null;
        }
    }

    private byte[] GetReadableTextureBytes(Texture2D sourceTexture)
    {
        // 1. ���� �ؽ�ó�� �����Ͽ� ���� �ؽ�ó�� ���� ũ��� ����ϴ�.
        RenderTexture rt = RenderTexture.GetTemporary(
            sourceTexture.width,
            sourceTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // 2. ���� �ؽ�ó(sourceTexture)�� ������ ���� �ؽ�ó(rt)�� ����(Blit)�մϴ�.
        Graphics.Blit(sourceTexture, rt);

        // 3. ���� Ȱ��ȭ�� ���� �ؽ�ó�� ����ϰ�, �츮�� ���� �ؽ�ó�� Ȱ��ȭ�մϴ�.
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        // 4. Ȱ��ȭ�� ���� �ؽ�ó���� �ȼ��� �о�� ���ο� Texture2D�� �����մϴ�.
        // �� �ؽ�ó�� �б� ������ ARGB32 �����Դϴ�.
        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();

        // 5. Ȱ�� ���� �ؽ�ó�� ������� �����ϰ�, �ӽ� ���� �ؽ�ó�� �����մϴ�.
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        // 6. ���� �б� ������ ���纻(readableTexture)�� PNG�� ���ڵ��մϴ�.
        return readableTexture.EncodeToPNG();
    }
}