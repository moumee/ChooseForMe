using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadMyScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
