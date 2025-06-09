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
    // --- JavaScript 플러그인 함수 선언 ---
#if UNITY_WEBGL && !UNITY_EDITOR
            [DllImport("__Internal")]
            private static extern void UploadFile(string gameObjectName, string callbackMethodName, string fileType);
#endif

    // --- 에디터 테스트용 필드 ---
#if UNITY_EDITOR
    [Header("에디터 테스트용 이미지")]
    public Texture2D testImage1; // Inspector 창에서 테스트할 이미지 파일을 여기에 연결
    public Texture2D testImage2;
#endif

    [Header("FirebaseManager Connection")]
    [Tooltip("FirebaseManager 오브젝트에 연결해야 합니다.")]
    [SerializeField] private ProfileManager _profileManager;

    [Header("UI Elements")]
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

        if (_profileManager == null) Debug.LogError("ProfileManager가 연결되지 않았습니다!");
        // 버튼 리스너 할당
        image1Button.onClick.AddListener(() => PickImageForOption(1));
        image2Button.onClick.AddListener(() => PickImageForOption(2));
        uploadVoteButton.onClick.AddListener(HandleCreateVoteClicked);
    }

    /// <summary>
    /// 이미지 선택 버튼 클릭 시 플랫폼에 맞는 파일 피커를 호출합니다.
    /// </summary>
    void PickImageForOption(int optionNumber)
    {
        _currentPickingOption = optionNumber;

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 빌드 환경에서는 JavaScript 플러그인 함수를 호출합니다.
        UploadFile(gameObject.name, "OnImageSelectedFromWebGL", "image/*");
#elif UNITY_EDITOR
        // 에디터 환경에서는 테스트용 이미지를 바로 byte 배열로 변환합니다.
        Debug.Log("에디터 모드: 테스트 이미지를 사용합니다.");
        Texture2D selectedTestImage = (optionNumber == 1) ? testImage1 : testImage2;
        if (selectedTestImage != null)
        {
            byte[] bytes = GetReadableTextureBytes(selectedTestImage);
            ProcessSelectedImageBytes(bytes);
        }
        else
        {
            Debug.LogWarning($"테스트 이미지 {optionNumber}가 지정되지 않았습니다.");
        }
#else
        // 모바일 등 다른 환경에서는 NativeGallery와 같은 에셋이 필요합니다.
        Debug.LogWarning("이 플랫폼에서는 이미지 피커 기능이 지원되지 않습니다.");
#endif
    }

    /// <summary>
    /// JavaScript에서 파일 선택이 완료되면 호출될 콜백 메소드 (WebGL 전용)
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
    /// 선택된 이미지의 byte 데이터를 공통으로 처리하는 메소드
    /// </summary>
    private void ProcessSelectedImageBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return;

        if (_currentPickingOption == 1)
        {
            _image1Bytes = bytes;
            Debug.Log("Option 1 이미지가 데이터로 저장되었습니다.");
        }
        else
        {
            _image2Bytes = bytes;
            Debug.Log("Option 2 이미지가 데이터로 저장되었습니다.");
        }
        // TODO: 선택된 이미지를 UI에 미리보기로 보여주는 로직 추가
    }



    /// <summary>
    /// UI의 투표 생성 버튼 클릭 시 안전하게 비동기 메소드를 호출하는 핸들러
    /// </summary>
    private void HandleCreateVoteClicked()
    {
        // _= 는 "이 비동기 작업이 끝날 때까지 기다릴 필요는 없다"는 의미의 최신 C# 문법입니다.
        // CreateVoteAsync 내부에서 모든 예외 처리를 담당합니다.
        _ = CreateVoteAsync();
    }

    /// <summary>
    /// 텍스트 기반의 투표 문서를 Firestore에 생성하는 핵심 메소드
    /// </summary>
    private async Task CreateVoteAsync()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("로그인되지 않아 투표를 생성할 수 없습니다.");
            return;
        }

        string opt1 = option1Input.text;
        string opt2 = option2Input.text;

        if (string.IsNullOrEmpty(opt1) || string.IsNullOrEmpty(opt2))
        {
            Debug.LogWarning("모든 텍스트 필드를 채워야 합니다.");
            return;
        }

        uploadVoteButton.interactable = false; // 중복 클릭 방지

        try
        {
            // 1. 이미지 병렬 업로드 및 작성자 닉네임 병렬 조회
            Task<string> uploadTask1 = (_image1Bytes != null) ? UploadImageAsync(_image1Bytes, "option1") : Task.FromResult<string>(null);
            Task<string> uploadTask2 = (_image2Bytes != null) ? UploadImageAsync(_image2Bytes, "option2") : Task.FromResult<string>(null);
            Task<string> nicknameTask = _profileManager.GetUserNicknameAsync(user.UserId);

            await Task.WhenAll(uploadTask1, uploadTask2, nicknameTask);

            string imageUrl1 = await uploadTask1;
            string imageUrl2 = await uploadTask2;
            string creatorNickname = await nicknameTask ?? "익명";

            // 2. 최종 데이터 구조 생성
            var pollData = new Dictionary<string, object>
            {
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

            // 3. 'polls' 컬렉션에 문서 추가
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

    /// <summary>
    /// 작업 완료 후 입력 필드를 초기화하는 메소드
    /// </summary>
    private void ClearInputFields()
    {
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
            string fileName = $"{fileNamePart}_{Guid.NewGuid()}.png"; // 확장자는 PNG로 통일 (혹은 MimeType에 따라)
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

    private byte[] GetReadableTextureBytes(Texture2D sourceTexture)
    {
        // 1. 렌더 텍스처를 생성하여 원본 텍스처와 같은 크기로 만듭니다.
        RenderTexture rt = RenderTexture.GetTemporary(
            sourceTexture.width,
            sourceTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // 2. 원본 텍스처(sourceTexture)를 생성한 렌더 텍스처(rt)에 복사(Blit)합니다.
        Graphics.Blit(sourceTexture, rt);

        // 3. 현재 활성화된 렌더 텍스처를 백업하고, 우리의 렌더 텍스처를 활성화합니다.
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        // 4. 활성화된 렌더 텍스처에서 픽셀을 읽어올 새로운 Texture2D를 생성합니다.
        // 이 텍스처는 읽기 가능한 ARGB32 형식입니다.
        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();

        // 5. 활성 렌더 텍스처를 원래대로 복원하고, 임시 렌더 텍스처를 해제합니다.
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        // 6. 이제 읽기 가능한 복사본(readableTexture)을 PNG로 인코딩합니다.
        return readableTexture.EncodeToPNG();
    }
}