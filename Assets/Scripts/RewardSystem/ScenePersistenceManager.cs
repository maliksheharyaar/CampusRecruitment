using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersistenceManager : MonoBehaviour
{
    // Singleton instance
    public static ScenePersistenceManager Instance { get; private set; }
    
    // Reference to our BookManager
    private BookManager bookManager;
    
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Register for scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("ScenePersistenceManager initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        // Check if the loaded scene is CanvasTestScene
        if (scene.name == "CanvasTestScene")
        {
            // Ensure we have a BookManager
            EnsureBookManagerExists();
        }
    }
    
    private void EnsureBookManagerExists()
    {
        // First check if we already have a reference
        if (bookManager != null)
        {
            Debug.Log("BookManager reference exists");
            return;
        }
        
        // Try to find existing BookManager
        bookManager = FindObjectOfType<BookManager>();
        
        // If not found, create a new one
        if (bookManager == null)
        {
            Debug.Log("Creating new BookManager");
            GameObject bookManagerObj = new GameObject("BookManager");
            bookManager = bookManagerObj.AddComponent<BookManager>();
            DontDestroyOnLoad(bookManagerObj);
        }
        else
        {
            Debug.Log("Found existing BookManager");
        }
    }
    
    // Public method to get the BookManager
    public BookManager GetBookManager()
    {
        EnsureBookManagerExists();
        return bookManager;
    }
} 