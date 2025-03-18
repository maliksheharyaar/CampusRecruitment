using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to any GameObject to create a portal back to a specified scene
/// </summary>
public class SceneTransporter : MonoBehaviour
{
    [Tooltip("Name of the scene to load when triggered")]
    [SerializeField] private string targetSceneName = "MainScene";
    
    [Tooltip("Optional transition delay in seconds")]
    [SerializeField] private float transitionDelay = 0.0f;
    
    [Tooltip("Whether to show debug messages")]
    [SerializeField] private bool showDebug = true;
    
    [Tooltip("Optional key to press when in trigger zone")]
    [SerializeField] private KeyCode activationKey = KeyCode.None;
    
    private bool playerInRange = false;
    
    /// <summary>
    /// Immediately load the target scene
    /// </summary>
    public void TransportToScene()
    {
        if (showDebug)
            Debug.Log($"[SceneTransporter] Loading scene: {targetSceneName}");
            
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneTransporter] No target scene specified!");
            return;
        }
        
        if (transitionDelay <= 0)
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Invoke(nameof(DelayedSceneLoad), transitionDelay);
        }
    }
    
    private void DelayedSceneLoad()
    {
        SceneManager.LoadScene(targetSceneName);
    }
    
    // Optional trigger zone implementation
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (showDebug)
                Debug.Log("[SceneTransporter] Player entered transport zone");
                
            // If no key is required, transport immediately
            if (activationKey == KeyCode.None)
            {
                TransportToScene();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (showDebug)
                Debug.Log("[SceneTransporter] Player exited transport zone");
        }
    }
    
    private void Update()
    {
        // If a key is required and player is in range
        if (activationKey != KeyCode.None && playerInRange)
        {
            if (Input.GetKeyDown(activationKey))
            {
                if (showDebug)
                    Debug.Log($"[SceneTransporter] Activation key {activationKey} pressed");
                    
                TransportToScene();
            }
        }
    }
} 