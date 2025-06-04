using Firebase.Auth;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseAnonymousLogin : MonoBehaviour
{
    public async Task<FirebaseUser> SignInAnonymouslyAsync()
    {
        var auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            Debug.Log($"이미 로그인되어 있습니다. UID: {auth.CurrentUser.UserId}");
            return auth.CurrentUser;
        }

        try
        {
            // 1. AuthResult 타입으로 결과를 받습니다.
            AuthResult result = await auth.SignInAnonymouslyAsync();

            // 2. 결과 객체의 .User 프로퍼티를 통해 FirebaseUser 객체에 접근합니다.
            FirebaseUser user = result.User;
            Debug.Log($"익명 로그인 성공! UID: {user.UserId}");
            return user;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"익명 로그인 실패: {e.Message}");
            return null;
        }
    }
}