using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Simple scene loader with loading screen to be called through dialogue or events
/// </summary>
public class SimpleSceneLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load when called")]
    [SerializeField] private string targetSceneName = "MainScene";
    
    [Header("Loading Screen")]
    [Tooltip("Canvas containing the loading screen elements")]
    [SerializeField] private GameObject loadingScreenCanvas;
    
    [Tooltip("Progress bar/slider to show loading progress")]
    [SerializeField] private Slider loadingProgressBar;
    
    [Tooltip("Optional text to display loading percentage")]
    [SerializeField] private TMP_Text loadingPercentageText;
    
    [Tooltip("How long to display loading screen after loading completes (seconds)")]
    [SerializeField] private float minimumLoadingScreenTime = 0.5f;

    /// <summary>
    /// Call this method from dialogue events or buttons to load the scene with loading screen
    /// </summary>
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            Debug.LogError("[SimpleSceneLoader] No target scene specified!");
        }
    }
    
    /// <summary>
    /// Loads the scene asynchronously with a loading screen
    /// </summary>
    private IEnumerator LoadSceneAsync()
    {
        // Activate loading screen
        if (loadingScreenCanvas != null)
        {
            loadingScreenCanvas.SetActive(true);
        }
        
        // Reset progress bar
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 0f;
        }
        
        // Update text
        UpdateLoadingPercentage(0);
        
        // Start async load operation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        
        // Don't allow scene activation until we're ready
        asyncLoad.allowSceneActivation = false;
        
        float startTime = Time.time;
        float progress = 0f;
        
        // Keep checking progress until load is almost complete (0.9f is the max value before activation)
        while (progress < 0.9f)
        {
            progress = asyncLoad.progress;
            
            // Update loading bar
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress / 0.9f;
            }
            
            // Update text
            UpdateLoadingPercentage(progress / 0.9f);
            
            yield return null;
        }
        
        // Ensure we display the loading screen for at least the minimum time
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumLoadingScreenTime)
        {
            yield return new WaitForSeconds(minimumLoadingScreenTime - elapsedTime);
        }
        
        // Set loading bar to 100%
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 1f;
        }
        
        // Update text to 100%
        UpdateLoadingPercentage(1f);
        
        // Small delay to show 100% completion
        yield return new WaitForSeconds(0.2f);
        
        // Allow scene to activate
        asyncLoad.allowSceneActivation = true;
    }
    
    /// <summary>
    /// Updates the loading percentage text if available
    /// </summary>
    private void UpdateLoadingPercentage(float progress)
    {
        if (loadingPercentageText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            loadingPercentageText.text = $"{percentage}%";
        }
    }
} 