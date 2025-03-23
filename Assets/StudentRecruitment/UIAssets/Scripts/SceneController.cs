using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static bool isTransitioning = false;
    
    [SerializeField] private string mainSceneName = "MainScene";
    [SerializeField] private string miniGameSceneName = "MiniGameScene";

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
    
    private void Start()
    {
        // Reset transition flag on start
        isTransitioning = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clean up any lingering materials
        CleanupMaterials();
        
        // Reset transition flag
        isTransitioning = false;
        
        Debug.Log($"[SceneController] Scene loaded: {scene.name}");
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
        if (isTransitioning) return;
        isTransitioning = true;
        
        Debug.Log("[SceneController] Returning to main scene");
        
        // Make sure cursor is unlocked for UI
        ForceUnlockCursor();
        
        // Load main scene directly
        SceneManager.LoadScene(mainSceneName);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        
        // Clean up before loading
        CleanupMaterials();

        Debug.Log($"[SceneController] Loading scene: {sceneName}");
        
        // Make sure cursor is unlocked for loading screen
        ForceUnlockCursor();

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
    
    private void ForceUnlockCursor()
    {
        // Make sure cursor is unlocked and visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LaunchMiniGame()
    {
        if (string.IsNullOrEmpty(miniGameSceneName))
        {
            Debug.LogError("[SceneController] Mini game scene name is not set!");
            return;
        }
        
        if (isTransitioning) return;

        // Clean up before loading
        CleanupMaterials();
        
        // Make sure cursor is unlocked for UI
        ForceUnlockCursor();
        
        Debug.Log($"[SceneController] Launching mini-game scene: {miniGameSceneName}");
        
        StartCoroutine(LoadSceneAsync(miniGameSceneName));
    }
}