using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "MainScene";
    [SerializeField] private string miniGameSceneName = "MiniGame"; //  To be implemented later

    public void ReturnToMainScene()
    {
        // Show loading UI here if needed
        StartCoroutine(LoadSceneAsync("MainScene"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Optional: Show loading progress
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // Update loading UI if you have one
            yield return null;
        }
    }

    public void LaunchMiniGame()
    {
        // For future implementation
        Debug.Log("Mini game will be implemented later");
        // SceneManager.LoadScene(miniGameSceneName);
    }
}