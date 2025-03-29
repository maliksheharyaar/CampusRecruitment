using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingCanvasManager : MonoBehaviour
{
    private static LoadingCanvasManager instance;
    public static LoadingCanvasManager Instance => instance;

    [SerializeField] private GameObject loadingCanvas;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[LoadingCanvasManager] Initialized as singleton");
        }
        else
        {
            Debug.Log("[LoadingCanvasManager] Destroying duplicate instance");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide loading canvas at start
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
            Debug.Log("[LoadingCanvasManager] Loading canvas hidden at start");
        }
        else
        {
            Debug.LogError("[LoadingCanvasManager] Loading canvas reference is missing!");
        }
    }

    public void ShowLoadingCanvas()
    {
        if (loadingCanvas != null && !isTransitioning)
        {
            isTransitioning = true;
            loadingCanvas.SetActive(true);
            Debug.Log("[LoadingCanvasManager] Loading canvas shown");
        }
        else if (isTransitioning)
        {
            Debug.LogWarning("[LoadingCanvasManager] Attempted to show loading canvas while already transitioning");
        }
    }

    public void HideLoadingCanvas()
    {
        if (loadingCanvas != null && isTransitioning)
        {
            isTransitioning = false;
            loadingCanvas.SetActive(false);
            Debug.Log("[LoadingCanvasManager] Loading canvas hidden");
        }
        else if (!isTransitioning)
        {
            Debug.LogWarning("[LoadingCanvasManager] Attempted to hide loading canvas while not transitioning");
        }
    }

    public bool IsTransitioning => isTransitioning;

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Debug.Log("[LoadingCanvasManager] Instance destroyed");
        }
    }
} 