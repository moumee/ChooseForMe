using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseAnonymousLogin : MonoBehaviour
{
    // Start(), Awake() ��� ����
    public async Task<FirebaseUser> SignInAnonymouslyAsync(FirebaseAuth auth)
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log($"�̹� �α��εǾ� �ֽ��ϴ�: {auth.CurrentUser.UserId}");
            return auth.CurrentUser;
        }

        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync();
            Debug.Log($"�͸� �α��� ����: {result.User.UserId}");
            return result.User;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�͸� �α��� ����: {e.Message}");
            return null;
        }
    }
}