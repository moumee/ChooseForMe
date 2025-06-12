using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class ScreenContentUI : MonoBehaviour
{
    // 다운로드한 스프라이트를 저장해둘 static 캐시 (앱 전체에서 공유)
    private static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

    private ContentData currentData;
    private Coroutine _imageALoadingCoroutine;
    private Coroutine _imageBLoadingCoroutine;
    private string _loadedImageUrlA;
    private string _loadedImageUrlB;

    [Header("Phase Parent Groups")]
    [SerializeField] private GameObject votePhaseGroup;
    [SerializeField] private GameObject resultPhaseGroup;

    [Header("Vote/Prediction Phase UI")]
    [SerializeField] private TMP_Text vote_UserTitleText;
    [SerializeField] private TMP_Text vote_StateQuestionText;
    [SerializeField] private TMP_Text vote_StateDescriptionText;
    [SerializeField] private TMP_Text vote_OptionAText;
    [SerializeField] private TMP_Text vote_OptionBText;
    [SerializeField] private Image vote_ItemAImage;
    [SerializeField] private Image vote_ItemBImage;
    
    [Header("Result Phase UI")]
    [SerializeField] private TMP_Text result_TitleText;
    [SerializeField] private TMP_Text result_OptionAText;
    [SerializeField] private TMP_Text result_OptionBText;
    [SerializeField] private Slider result_SliderA;
    [SerializeField] private TMP_Text result_PercentA;
    [SerializeField] private Slider result_SliderB;
    [SerializeField] private TMP_Text result_PercentB;
    [SerializeField] private TMP_Text result_PredictionResultText;
    [SerializeField] private TMP_Text result_SliderLabelA;
    [SerializeField] private TMP_Text result_SliderLabelB;
    
    [Header("Comments Section")]
    [SerializeField] private Transform result_CommentsContentArea;
    [SerializeField] private GameObject result_CommentPrefab;

    public void SetContent(ContentData data)
    {
        this.currentData = data;
        if (currentData == null) return;

        bool isResultState = data.currentState == VoteState.Result;
        votePhaseGroup.SetActive(!isResultState);
        resultPhaseGroup.SetActive(isResultState);

        if (isResultState)
        {
            SetupResultUI();
        }
        else
        {
            SetupVotePhaseUI();
        }
    }

    private void SetupVotePhaseUI()
    {
        vote_UserTitleText.text = currentData.voteTitle;
        vote_OptionAText.text = currentData.itemAName;
        vote_OptionBText.text = currentData.itemBName;

        if (currentData.currentState == VoteState.InitialVote) {
            vote_StateQuestionText.text = "자신의 원픽은?";
            vote_StateDescriptionText.text = "자신의 생각대로 투표해주세요.";
        } else {
            vote_StateQuestionText.text = "현재 우세한 항목은?";
            vote_StateDescriptionText.text = "현재 득표 수가 더 많다고 생각되는 항목을 선택해주세요.";
        }
        
        LoadImage(ref _imageALoadingCoroutine, ref _loadedImageUrlA, currentData.itemAImageUrl, vote_ItemAImage);
        LoadImage(ref _imageBLoadingCoroutine, ref _loadedImageUrlB, currentData.itemBImageUrl, vote_ItemBImage);
    }

    private void SetupResultUI()
    {
        result_TitleText.text = $"[결과] {currentData.voteTitle}";
        result_OptionAText.text = currentData.itemAName;
        result_OptionBText.text = currentData.itemBName;
        result_SliderA.value = currentData.itemAResultPercent;
        result_SliderB.value = currentData.itemBResultPercent;
        result_PercentA.text = currentData.itemAResultPercent.ToString("P0");
        result_PercentB.text = currentData.itemBResultPercent.ToString("P0");

        // ==========================================================
        // ▼▼▼▼▼▼▼▼▼▼ 여기가 수정된 부분입니다 ▼▼▼▼▼▼▼▼▼▼
        // 슬라이더 위 라벨에는 이제 '이름'만 표시합니다.
        if (result_SliderLabelA != null)
            result_SliderLabelA.text = currentData.itemAName; // 퍼센트 부분 삭제
        if (result_SliderLabelB != null)
            result_SliderLabelB.text = currentData.itemBName; // 퍼센트 부분 삭제
        // ==========================================================

        bool predictionSuccess = false;
        if (currentData.itemAResultPercent > currentData.itemBResultPercent && currentData.userPredictionChoice == 0)
            predictionSuccess = true;
        else if (currentData.itemBResultPercent > currentData.itemAResultPercent &&
                 currentData.userPredictionChoice == 1) predictionSuccess = true;

        result_PredictionResultText.text = predictionSuccess ? "예측 성공!" : "예측 실패";
        UpdateComments();
    }
    
    private void LoadImage(ref Coroutine coroutine, ref string loadedUrl, string newUrl, Image targetImage)
    {
        if (targetImage == null) return;
        if (!string.IsNullOrEmpty(loadedUrl) && loadedUrl == newUrl && targetImage.sprite != null) return;

        if (gameObject.activeInHierarchy) {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        
        loadedUrl = newUrl;
        
        if (string.IsNullOrEmpty(newUrl))
        {
            targetImage.sprite = null;
            return;
        }

        if (_spriteCache.ContainsKey(newUrl))
        {
            targetImage.sprite = _spriteCache[newUrl];
        }
        else
        {
            targetImage.sprite = null;
            if (gameObject.activeInHierarchy) {
                coroutine = StartCoroutine(LoadImageCoroutine(newUrl, targetImage));
            }
        }
    }

    private IEnumerator LoadImageCoroutine(string url, Image targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (this == null || !this.enabled) yield break;
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                targetImage.sprite = sprite;
                
                if (!_spriteCache.ContainsKey(url))
                {
                    _spriteCache.Add(url, sprite);
                }
            }
            else
            {
                Debug.LogError($"Image download failed: {url} | Error: {request.error}");
            }
        }
    }

    private void UpdateComments()
    {
        if (result_CommentsContentArea == null) return;
        foreach (Transform child in result_CommentsContentArea) Destroy(child.gameObject);
        if (currentData.comments == null) return;
        foreach (string commentText in currentData.comments)
        {
            if (result_CommentPrefab == null) continue;
            GameObject commentObject = Instantiate(result_CommentPrefab, result_CommentsContentArea);
            TMP_Text textComponent = commentObject.GetComponentInChildren<TMP_Text>();
            if (textComponent != null) textComponent.text = commentText;
        }
    }
    
    // ==========================================================
    // ▼▼▼▼▼▼▼▼▼▼ 여기가 새로 수정한 버튼 관리 로직입니다 ▼▼▼▼▼▼▼▼▼▼
    // ==========================================================
    
    /// <summary>
    /// 투표/예측 단계의 버튼들이 클릭될 때 호출될 공용 메서드.
    /// 이 메서드를 Inspector의 OnClick()에 연결합니다.
    /// </summary>
    /// <param name="choice">0은 A항목, 1은 B항목</param>
    public void OnOptionButtonClicked(int choice)
    {
        if (currentData == null) return;

        // 현재 상태에 따라 다른 함수를 호출
        switch (currentData.currentState)
        {
            case VoteState.InitialVote:
                HandleInitialVote(choice);
                break;
            case VoteState.Prediction:
                HandlePredictionVote(choice);
                break;
            case VoteState.Result:
                // 결과 화면에서는 버튼이 눌려도 아무것도 하지 않음
                break;
        }
    }

    // private으로 변경하여 내부 로직으로만 사용
    private void HandleInitialVote(int choice)
    {
        if (currentData.currentState != VoteState.InitialVote) return;

        currentData.userVoteChoice = choice;
        currentData.currentState = VoteState.Prediction;
        SetContent(currentData);
        // TODO: 여기에 Firebase에 투표 결과를 저장하는 로직을 호출할 수 있습니다.
        // 예: pollDataManager.SubmitVoteAsync(currentData.pollId, choice);
    }
    
    // private으로 변경하여 내부 로직으로만 사용
    private void HandlePredictionVote(int choice)
    {
        if (currentData.currentState != VoteState.Prediction) return;

        currentData.userPredictionChoice = choice;
        currentData.currentState = VoteState.Result;
        SetContent(currentData);
        // TODO: 여기에 Firebase에 예측 결과를 저장하는 로직을 호출할 수 있습니다.
    }
}