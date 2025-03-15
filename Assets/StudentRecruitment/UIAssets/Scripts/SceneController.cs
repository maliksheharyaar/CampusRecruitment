using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "MainScene";
    [SerializeField] private string miniGameSceneName = "MiniGame"; // To be implemented later

    private void OnEnable()
    {
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene load events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clean up any lingering materials
        CleanupMaterials();
    }

    private void CleanupMaterials()
    {
        // Find and cleanup any temporary materials
        var materials = FindObjectsOfType<Material>();
        foreach (var material in materials)
        {
            if (material != null && material.name.Contains("Temporary"))
            {
                DestroyImmediate(material);
            }
        }
    }

    public void ReturnToMainScene()
    {
        StartCoroutine(LoadSceneAsync(mainSceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Clean up before loading
        CleanupMaterials();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is ready
        while (asyncLoad.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // Update loading UI if you have one
            yield return null;
        }
        
        asyncLoad.allowSceneActivation = true;
    }

    public void LaunchMiniGame()
    {
        if (string.IsNullOrEmpty(miniGameSceneName))
        {
            Debug.LogError("Mini game scene name is not set!");
            return;
        }

        // Clean up before loading
        CleanupMaterials();
        StartCoroutine(LoadSceneAsync(miniGameSceneName));
    }
}