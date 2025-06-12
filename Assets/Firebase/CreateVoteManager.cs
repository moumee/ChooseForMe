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

using SFB; // Standalone File Browser 사용을 위해 추가

public class CreateVoteManager : MonoBehaviour
{
    [Header("Manager Connection")]
    [Tooltip("@Managers 오브젝트에 연결해야 합니다.")]
    [SerializeField] private ProfileManager _profileManager;

    [Header("UI Elements")]
    public TMP_InputField titleInput;
    public TMP_InputField option1Input;
    public TMP_InputField option2Input;
    public Button image1Button;
    public Button image2Button;
    public Button uploadVoteButton;
    // TODO: 투표 유형(PredictionPoll, UserSelectionPoll)을 선택할 UI 추가 필요
    [Header("Image Previews")]
    public Image option1ImagePreview;
    public Image option2ImagePreview;


    // --- Private Fields ---
    private byte[] _image1Bytes;
    private byte[] _image2Bytes;
    private int _currentPickingOption;

    // --- Firebase Instances ---
    private FirebaseFirestore _db;
    private FirebaseStorage _storage;
    private FirebaseAuth _auth;


    public void Initialize(FirebaseFirestore db, FirebaseStorage storage, FirebaseAuth auth)
    {
        _db = db;
        _storage = storage;
        _auth = auth;
    }


    void Start()
    {

        if (_profileManager == null) Debug.LogError("ProfileManager가 연결되지 않았습니다!");

        image1Button.onClick.AddListener(() => PickImageForOption(1));
        image2Button.onClick.AddListener(() => PickImageForOption(2));
        uploadVoteButton.onClick.AddListener(HandleCreateVoteClicked);
    }

    /// <summary>
    /// 이미지 선택 버튼 클릭 시 Windows 파일 탐색기를 엽니다.
    /// </summary>
    void PickImageForOption(int optionNumber)
    {
        _currentPickingOption = optionNumber;

        // 파일 탐색기에 표시할 확장자 필터 설정
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        // 파일 탐색기 열기 (단일 파일 선택 모드)
        string[] paths = StandaloneFileBrowser.OpenFilePanel("이미지 선택", "", extensions, false);

        // 사용자가 파일을 선택했다면 (경로가 1개 이상 반환되면)
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            Debug.Log($"선택된 파일 경로: {paths[0]}");
            // 해당 경로의 파일을 byte 배열로 읽어와서 처리
            byte[] bytes = File.ReadAllBytes(paths[0]);
            ProcessSelectedImageBytes(bytes);
        }
    }

    /// <summary>
    /// 선택된 이미지의 byte 데이터를 내부 변수에 저장하고 로그를 출력합니다.
    /// </summary>
    /// <summary>
    /// 선택된 이미지의 byte 데이터를 내부 변수에 저장하고 UI에 미리보기를 표시합니다.
    /// </summary>
    private void ProcessSelectedImageBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return;

        // 1. byte 배열을 Texture2D로 변환합니다.
        //    - 새로운 Texture2D를 생성합니다. (크기는 중요하지 않음, LoadImage가 알아서 조절)
        //    - ImageConversion.LoadImage 함수는 byte 데이터를 읽어 텍스처를 구성합니다.
        Texture2D texture = new Texture2D(2, 2);
        if (!ImageConversion.LoadImage(texture, bytes))
        {
            Debug.LogError("이미지 데이터를 Texture2D로 변환하는 데 실패했습니다.");
            return;
        }

        // 2. Texture2D를 UI에 표시할 수 있는 Sprite로 변환합니다.
        Sprite newSprite = Sprite.Create(
            texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f) // 이미지의 정중앙을 중심축(pivot)으로 설정
        );

        // 3. 현재 선택 중인 옵션에 맞는 Image 컴포넌트와 byte 변수를 찾아 처리합니다.
        if (_currentPickingOption == 1)
        {
            _image1Bytes = bytes; // 나중에 업로드할 수 있도록 byte 데이터 저장
            if (option1ImagePreview != null)
            {
                option1ImagePreview.sprite = newSprite; // UI Image의 이미지를 교체
                option1ImagePreview.gameObject.SetActive(true); // 이미지가 없어서 꺼져있었다면 다시 켭니다.
            }
            Debug.Log("Option 1 이미지 미리보기를 업데이트했습니다.");
        }
        else
        {
            _image2Bytes = bytes;
            if (option2ImagePreview != null)
            {
                option2ImagePreview.sprite = newSprite;
                option2ImagePreview.gameObject.SetActive(true);
            }
            Debug.Log("Option 2 이미지 미리보기를 업데이트했습니다.");
        }
    }




    private void HandleCreateVoteClicked()
    {
        _ = CreateVoteAsync();
    }

    /// <summary>
    /// 입력된 정보와 이미지를 사용하여 투표 문서를 Firestore에 생성합니다.
    /// </summary>
    private async Task CreateVoteAsync()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null) { /* ... 로그인 확인 ... */ return; }

        string title = titleInput.text;
        string opt1 = option1Input.text;
        string opt2 = option2Input.text;
        string pollTypeString = "PredictionPoll"; // TODO: UI에서 실제 값 가져오기

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(opt1) || string.IsNullOrEmpty(opt2))
        {
            Debug.LogWarning("제목을 포함한 모든 텍스트 필드를 채워야 합니다.");
            return;
        }

        uploadVoteButton.interactable = false;

        try
        {
            Task<string> uploadTask1 = (_image1Bytes != null) ? UploadImageAsync(_image1Bytes, "option1") : Task.FromResult<string>(null);
            Task<string> uploadTask2 = (_image2Bytes != null) ? UploadImageAsync(_image2Bytes, "option2") : Task.FromResult<string>(null);
            Task<string> nicknameTask = _profileManager.GetUserNicknameAsync(user.UserId);

            await Task.WhenAll(uploadTask1, uploadTask2, nicknameTask);

            string imageUrl1 = await uploadTask1;
            string imageUrl2 = await uploadTask2;
            string creatorNickname = await nicknameTask ?? "익명";

            var pollData = new Dictionary<string, object>
            {
                { "question", title },
                { "options", new List<string> { opt1, opt2 } },
                { "creatorUid", user.UserId },
                { "creatorNickname", creatorNickname },
                { "createdAt", FieldValue.ServerTimestamp },
                { "pollType", pollTypeString }, 
                { "optionImages", new Dictionary<string, string> { {"0", imageUrl1 ?? ""}, {"1", imageUrl2 ?? ""} } },
                { "totalVoteCount", 0 },
                { "option1Votes", 0 },
                { "option2Votes", 0 },
                { "isClosed", false }
            };
            
            DocumentReference addedDocRef = await _db.Collection("polls").AddAsync(pollData);
            Debug.Log($"투표가 성공적으로 생성되었습니다! Poll ID: {addedDocRef.Id}");
            
            ClearInputFields();
        }
        catch (Exception e)
        {
            Debug.LogError($"투표 생성 실패: {e.Message}");
        }
        finally
        {
            uploadVoteButton.interactable = true;
        }
    }
    
    private void ClearInputFields()
    {
        titleInput.text = "";
        option1Input.text = "";
        option2Input.text = "";
        _image1Bytes = null;
        _image2Bytes = null;
    }

    /// <summary>
    /// 이미지 byte 배열을 Firebase Storage에 업로드하고 다운로드 URL을 반환합니다.
    /// </summary>
    private async Task<string> UploadImageAsync(byte[] bytes, string fileNamePart)
    {
        try
        {
            // 파일 확장자를 알 수 없으므로 png로 통일하거나, 파일 시그니처를 분석해야 함. png가 무난.
            string fileName = $"{fileNamePart}_{Guid.NewGuid()}.png";
            StorageReference imageRef = _storage.GetReference($"pollImages/{_auth.CurrentUser.UserId}/{fileName}");
            var metadata = new MetadataChange { ContentType = "image/png" };
            
            await imageRef.PutBytesAsync(bytes, metadata);
            Uri downloadUri = await imageRef.GetDownloadUrlAsync();
            return downloadUri.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"이미지 업로드 실패 ({fileNamePart}): {e.Message}");
            return null;
        }
    }
}