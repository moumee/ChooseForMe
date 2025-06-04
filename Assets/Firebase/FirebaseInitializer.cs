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
            Debug.Log("Firebase 초기화 준비 완료.");
            return true;
        }
        else
        {
            Debug.LogError($"Firebase 초기화 실패: Status: {dependencyStatus}");
            return false;
        }
    }
}