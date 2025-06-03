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

                // �͸� �α��� ����
                auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCompletedSuccessfully)
                    {
                        FirebaseUser user = auth.CurrentUser;
                        Debug.Log($"�͸� �α��� ����! UID: {user.UserId}");
                    }
                    else
                    {
                        Debug.LogError($"�͸� �α��� ����: {authTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"Firebase �ʱ�ȭ ����: {task.Result}");
            }
        });
    }
}
