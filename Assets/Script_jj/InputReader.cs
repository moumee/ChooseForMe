using UnityEngine;
using TMPro;

public class MyInputReader : MonoBehaviour
{
    public TMP_InputField inputField;

    public static string userInputValue;

    public void PrintInput()
    {
        string currentValue = inputField.text;
        userInputValue = currentValue; // ���� ���� static ������ ����
        Debug.Log("���� ��ǲ�ʵ� ��: " + currentValue);
    }

}
