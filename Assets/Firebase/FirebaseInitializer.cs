using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{

    private void Awake()
    {
        if (FindObjectsByType<FirebaseInitializer>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);  // �̹� ������ ����
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase �ʱ�ȭ �Ϸ�");
            }
            else
            {
                Debug.LogError($"Firebase �ʱ�ȭ ����: {task.Result}");
            }
        });
    }
}
