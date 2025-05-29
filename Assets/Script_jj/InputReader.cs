using UnityEngine;
using TMPro;

public class MyInputReader : MonoBehaviour
{
    public TMP_InputField inputField;

    public static string userInputValue;

    public void PrintInput()
    {
        string currentValue = inputField.text;
        userInputValue = currentValue; // 현재 값을 static 변수에 저장
        Debug.Log("현재 인풋필드 값: " + currentValue);
    }

}
