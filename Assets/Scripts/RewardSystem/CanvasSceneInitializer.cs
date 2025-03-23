using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasSceneInitializer : MonoBehaviour
{
    [SerializeField] private bool tagUIElements = true;
    
    private void Awake()
    {
        Debug.Log("CanvasSceneInitializer Awake");
        
        // Make sure we have a ScenePersistenceManager
        EnsurePersistenceManagerExists();
        
        // Tag UI elements for easier finding if needed
        if (tagUIElements)
        {
            TagUIElements();
        }
    }
    
    private void Start()
    {
        Debug.Log("CanvasSceneInitializer Start");
        
        // Force UI update once the scene is loaded
        BookManager bookManager = FindObjectOfType<BookManager>();
        if (bookManager != null)
        {
            Debug.Log("Found BookManager, initializing UI");
        }
        else
        {
            Debug.LogWarning("BookManager not found, will be created by PersistenceManager");
        }
    }
    
    private void EnsurePersistenceManagerExists()
    {
        // Check if we already have a persistence manager
        ScenePersistenceManager persistenceManager = FindObjectOfType<ScenePersistenceManager>();
        
        if (persistenceManager == null)
        {
            // Create a new persistence manager
            GameObject persistenceObj = new GameObject("ScenePersistenceManager");
            persistenceObj.AddComponent<ScenePersistenceManager>();
            DontDestroyOnLoad(persistenceObj);
            
            Debug.Log("Created ScenePersistenceManager");
        }
        else
        {
            Debug.Log("Found existing ScenePersistenceManager");
        }
    }
    
    private void TagUIElements()
    {
        // Tag critical UI elements to make them easier to find programmatically
        
        // Find and tag text elements
        TMPro.TextMeshProUGUI[] textElements = FindObjectsOfType<TMPro.TextMeshProUGUI>();
        foreach (TMPro.TextMeshProUGUI text in textElements)
        {
            // Tag based on name
            if (text.name.Contains("CoinCount"))
                text.gameObject.tag = "CoinCountText";
            else if (text.name.Contains("PageCount"))
                text.gameObject.tag = "PageCountText";
        }
        
        // Find and tag buttons
        UnityEngine.UI.Button[] buttons = FindObjectsOfType<UnityEngine.UI.Button>();
        foreach (UnityEngine.UI.Button button in buttons)
        {
            if (button.name.Contains("CraftBook"))
                button.gameObject.tag = "CraftBookButton";
        }
        
        // Find and tag containers
        GameObject pageList = GameObject.Find("PageListContent");
        if (pageList != null)
            pageList.tag = "PageListContainer";
        
        // Find and tag panels
        GameObject bookPanel = GameObject.Find("BookViewPanel");
        if (bookPanel != null)
            bookPanel.tag = "BookViewPanel";
        
        GameObject pageDetailPanel = GameObject.Find("PageDetailPanel");
        if (pageDetailPanel != null)
            pageDetailPanel.tag = "PageDetailPanel";
        
        // Find and tag scroll views
        UnityEngine.UI.ScrollRect[] scrollRects = FindObjectsOfType<UnityEngine.UI.ScrollRect>();
        foreach (UnityEngine.UI.ScrollRect scrollRect in scrollRects)
        {
            if (scrollRect.name.Contains("PageList"))
                scrollRect.gameObject.tag = "PageListScrollRect";
        }
        
        Debug.Log("Tagged UI elements for BookManager reference finding");
    }
} 