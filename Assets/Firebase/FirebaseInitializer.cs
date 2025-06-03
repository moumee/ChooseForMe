using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{

    private void Awake()
    {
        if (FindObjectsByType<FirebaseInitializer>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);  // 이미 있으면 삭제
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
                Debug.Log("Firebase 초기화 완료");
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {task.Result}");
            }
        });
    }
}
