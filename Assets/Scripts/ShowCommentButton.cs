using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ShowCommentButton : MonoBehaviour
{
    [SerializeField] private GameObject comments;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ShowComments);
    }

    private void ShowComments()
    {
        comments.SetActive(true);
    }
}
