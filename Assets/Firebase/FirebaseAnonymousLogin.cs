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
            Debug.Log($"�̹� �α��εǾ� �ֽ��ϴ�. UID: {auth.CurrentUser.UserId}");
            return auth.CurrentUser;
        }

        try
        {
            // 1. AuthResult Ÿ������ ����� �޽��ϴ�.
            AuthResult result = await auth.SignInAnonymouslyAsync();

            // 2. ��� ��ü�� .User ������Ƽ�� ���� FirebaseUser ��ü�� �����մϴ�.
            FirebaseUser user = result.User;
            Debug.Log($"�͸� �α��� ����! UID: {user.UserId}");
            return user;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�͸� �α��� ����: {e.Message}");
            return null;
        }
    }
}