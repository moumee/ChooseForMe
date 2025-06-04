using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
