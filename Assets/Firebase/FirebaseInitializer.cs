using Firebase;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    public async Task<bool> InitializeFirebaseAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase �ʱ�ȭ �غ� �Ϸ�.");
            return true;
        }
        else
        {
            Debug.LogError($"Firebase �ʱ�ȭ ����: Status: {dependencyStatus}");
            return false;
        }
    }
}