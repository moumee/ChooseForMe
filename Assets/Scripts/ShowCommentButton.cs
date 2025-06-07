using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ShowCommentButton : MonoBehaviour
{
    [SerializeField] private GameObject comments;

    private Button _button;
    private RectTransform _commentsRectTransform;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ShowComments);

        _commentsRectTransform = comments.GetComponent<RectTransform>();
    }

    private void ShowComments()
    {
        _button.gameObject.SetActive(false);
        comments.SetActive(true);

        _commentsRectTransform.DOAnchorPos(_commentsRectTransform.anchoredPosition, 0.2f)
            .From(new Vector2(_commentsRectTransform.anchoredPosition.x, -_commentsRectTransform.rect.height - Screen.height * 0.5f))
            .SetEase(Ease.OutQuad);

    }
}
