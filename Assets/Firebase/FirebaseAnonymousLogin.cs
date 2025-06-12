using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseAnonymousLogin : MonoBehaviour
{
    // Start(), Awake() 모두 삭제
    public async Task<FirebaseUser> SignInAnonymouslyAsync(FirebaseAuth auth)
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log($"이미 로그인되어 있습니다: {auth.CurrentUser.UserId}");
            return auth.CurrentUser;
        }

        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync();
            Debug.Log($"익명 로그인 성공: {result.User.UserId}");
            return result.User;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"익명 로그인 실패: {e.Message}");
            return null;
        }
    }
}