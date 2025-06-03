using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseAnonymousLogin : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                // 익명 로그인 시작
                auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCompletedSuccessfully)
                    {
                        FirebaseUser user = auth.CurrentUser;
                        Debug.Log($"익명 로그인 성공! UID: {user.UserId}");
                    }
                    else
                    {
                        Debug.LogError($"익명 로그인 실패: {authTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {task.Result}");
            }
        });
    }
}
