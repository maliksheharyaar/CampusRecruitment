using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using StudentRecruitment.EndlessRunner;

public class AnalyticsManager : MonoBehaviour
{
    private static AnalyticsManager instance;
    public static AnalyticsManager Instance => instance;

    private const string USER_ID_KEY = "Analytics_UserID";
    private const string ANALYTICS_FILE = "AnalyticsLog.json";
    private string persistentDataPath;
    private string assetsDataPath;
    private List<AnalyticsEvent> events = new List<AnalyticsEvent>();
    private string userId;


    [Serializable]
    private class PageData
    {
        public int pageNumber;
        public string pageTitle;
        public int viewCount;
    }

    [Serializable]
    private class AnalyticsEvent
    {
        public string user_id;
        public string program;
        public bool minigame_started;
        public bool minigame_completed;
        public string book_page_unlocked;
        public bool book_crafted;
        public string timestamp;
        public List<PageData> pages = new List<PageData>();

        public AnalyticsEvent(string userId, string program)
        {
            this.user_id = userId;
            this.program = program;
            this.timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format
        }
    }

    [Serializable]
    private class AnalyticsData
    {
        public List<AnalyticsEvent> events = new List<AnalyticsEvent>();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAnalytics();
            SubscribeToGameEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SubscribeToGameEvents()
    {
        // Subscribe to EndlessRunner events
        EndlessRunnerManager.OnGameStateChanged += HandleGameStateChanged;
        Debug.Log("[AnalyticsManager] Subscribed to EndlessRunner.OnGameStateChanged");
        
        // Subscribe to GameProgress events
        GameProgress.OnPageUnlocked += HandlePageUnlocked;
        GameProgress.OnBookCrafted += HandleBookCrafted;
        Debug.Log("[AnalyticsManager] Subscribed to GameProgress.OnPageUnlocked and OnBookCrafted");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (EndlessRunnerManager.Instance != null)
        {
            EndlessRunnerManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        GameProgress.OnPageUnlocked -= HandlePageUnlocked;
        GameProgress.OnBookCrafted -= HandleBookCrafted;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Running:
                TrackMinigameStart("Business");
                break;
            case GameState.Win:
                TrackMinigameComplete("Business");
                break;
        }
    }

    private void HandlePageUnlocked(string program, string pageTitle)
    {
        Debug.Log($"[AnalyticsManager] Received page unlock event - Program: {program}, Page: {pageTitle}");
        
        // Get the current state from BookManager
        var bookManager = FindObjectOfType<BookManager>();
        if (bookManager != null)
        {
            // Get the page index from PlayerPrefs
            int pageIndex = PlayerPrefs.GetInt("PendingRewardPage", -1);
            if (pageIndex >= 0)
            {
                // Get the page title from BookManager's page list
                string actualPageTitle = bookManager.GetPageTitle(pageIndex);
                if (!string.IsNullOrEmpty(actualPageTitle))
                {
                    // Create event with the current page title
                    var evt = new AnalyticsEvent(userId, program)
                    {
                        book_page_unlocked = actualPageTitle
                    };
                    events.Add(evt);
                    SaveAnalyticsData();
                    Debug.Log($"[AnalyticsManager] Recorded page unlock - Page #{pageIndex + 1}: {actualPageTitle}");
                    return;
                }
            }
        }
        
        // Fallback if we can't get the page title from BookManager
        var fallbackEvt = new AnalyticsEvent(userId, program)
        {
            book_page_unlocked = pageTitle
        };
        events.Add(fallbackEvt);
        SaveAnalyticsData();
    }

    private void HandleBookCrafted(string program)
    {
        Debug.Log($"[AnalyticsManager] Received book crafted event - Program: {program}");
        
        // Check if the book crafting button is enabled
        var bookManager = FindObjectOfType<BookManager>();
        if (bookManager != null)
        {
            bool isBookCrafted = !bookManager.IsCraftButtonEnabled();
            if (isBookCrafted)
            {
                TrackBookCraft(program);
            }
        }
    }

    private void InitializeAnalytics()
    {
        // Generate or retrieve user ID
        userId = PlayerPrefs.GetString(USER_ID_KEY, Guid.NewGuid().ToString());
        PlayerPrefs.SetString(USER_ID_KEY, userId);

        // Set up file paths
        persistentDataPath = Path.Combine(Application.persistentDataPath, ANALYTICS_FILE);
        assetsDataPath = Path.Combine(Application.dataPath, "Analytics", ANALYTICS_FILE);

        // Create Analytics directory in Assets if it doesn't exist
        string analyticsDir = Path.Combine(Application.dataPath, "Analytics");
        if (!Directory.Exists(analyticsDir))
        {
            Directory.CreateDirectory(analyticsDir);
            Debug.Log($"[AnalyticsManager] Created Analytics directory at: {analyticsDir}");
        }

        Debug.Log($"[AnalyticsManager] Analytics files will be saved to:\nPersistent: {persistentDataPath}\nAssets: {assetsDataPath}");

        // Load existing data if available
        LoadAnalyticsData();
    }

    private void LoadAnalyticsData()
    {
        try
        {
            // Try to load from persistent data path first
            if (File.Exists(persistentDataPath))
            {
                string jsonData = File.ReadAllText(persistentDataPath);
                var data = JsonUtility.FromJson<AnalyticsData>(jsonData);
                events = data.events;
                Debug.Log($"[AnalyticsManager] Loaded {events.Count} existing analytics events from persistent data");
            }
            // If not found in persistent data, try assets directory
            else if (File.Exists(assetsDataPath))
            {
                string jsonData = File.ReadAllText(assetsDataPath);
                var data = JsonUtility.FromJson<AnalyticsData>(jsonData);
                events = data.events;
                Debug.Log($"[AnalyticsManager] Loaded {events.Count} existing analytics events from assets directory");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnalyticsManager] Error loading analytics data: {e.Message}");
        }
    }

    private void SaveAnalyticsData()
    {
        try
        {
            var data = new AnalyticsData { events = events };
            string jsonData = JsonUtility.ToJson(data, true); // true for pretty print

            // Save to persistent data path
            File.WriteAllText(persistentDataPath, jsonData);
            Debug.Log($"[AnalyticsManager] Saved {events.Count} analytics events to persistent data");

            // Save to assets directory
            File.WriteAllText(assetsDataPath, jsonData);
            Debug.Log($"[AnalyticsManager] Saved {events.Count} analytics events to assets directory");

            // Refresh the AssetDatabase to show the new file in the Unity Editor
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnalyticsManager] Error saving analytics data: {e.Message}");
        }
    }

    private void TrackMinigameStart(string program)
    {
        var evt = new AnalyticsEvent(userId, program)
        {
            minigame_started = true
        };
        events.Add(evt);
        SaveAnalyticsData();
    }

    private void TrackMinigameComplete(string program)
    {
        var evt = new AnalyticsEvent(userId, program)
        {
            minigame_started = true,  // Always true after completion
            minigame_completed = true
        };
        events.Add(evt);
        SaveAnalyticsData();
    }

    private void TrackBookPageUnlock(string program, string pageTitle)
    {
        Debug.Log($"[AnalyticsManager] Tracking book page unlock - Program: {program}, Page: {pageTitle}");
        var evt = new AnalyticsEvent(userId, program)
        {
            book_page_unlocked = pageTitle
        };
        events.Add(evt);
        SaveAnalyticsData();
    }

    private void TrackBookCraft(string program)
    {
        var evt = new AnalyticsEvent(userId, program)
        {
            book_crafted = true
        };
        events.Add(evt);
        SaveAnalyticsData();
    }

    private void OnApplicationQuit()
    {
        SaveAnalyticsData();
    }

    // Add this method to update the page title when we return to CanvasTestScene
    public void UpdateLastPageTitle(string pageTitle)
    {
        if (events.Count > 0)
        {
            var lastEvent = events[events.Count - 1];
            if (string.IsNullOrEmpty(lastEvent.book_page_unlocked))
            {
                lastEvent.book_page_unlocked = pageTitle;
                SaveAnalyticsData();
                Debug.Log($"[AnalyticsManager] Updated last page title to: {pageTitle}");
            }
        }
    }

    // Add this method to track page views
    public void TrackPageView(int pageNumber, string pageTitle)
    {
        if (events.Count > 0)
        {
            var lastEvent = events[events.Count - 1];
            
            // Find existing page data
            var pageData = lastEvent.pages.Find(p => p.pageNumber == pageNumber);
            if (pageData == null)
            {
                // Create new page data if it doesn't exist
                pageData = new PageData
                {
                    pageNumber = pageNumber,
                    pageTitle = pageTitle,
                    viewCount = 0
                };
                lastEvent.pages.Add(pageData);
            }
            
            // Increment view count
            pageData.viewCount++;
            SaveAnalyticsData();
            Debug.Log($"[AnalyticsManager] Tracked page view - Page #{pageNumber}: {pageTitle}, Views: {pageData.viewCount}");
        }
    }

#if UNITY_EDITOR
    [MenuItem("Analytics/View Analytics Data")]
    private static void ViewAnalyticsData()
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, ANALYTICS_FILE);
        string assetsPath = Path.Combine(Application.dataPath, "Analytics", ANALYTICS_FILE);

        Debug.Log("=== Analytics Data ===");
        
        if (File.Exists(persistentPath))
        {
            Debug.Log($"Persistent Data Path ({persistentPath}):");
            Debug.Log(File.ReadAllText(persistentPath));
        }
        else
        {
            Debug.Log("No analytics data found in persistent data path");
        }

        if (File.Exists(assetsPath))
        {
            Debug.Log($"Assets Path ({assetsPath}):");
            Debug.Log(File.ReadAllText(assetsPath));
        }
        else
        {
            Debug.Log("No analytics data found in assets directory");
        }
    }

    [MenuItem("Analytics/Clear Analytics Data")]
    private static void ClearAnalyticsData()
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, ANALYTICS_FILE);
        string assetsPath = Path.Combine(Application.dataPath, "Analytics", ANALYTICS_FILE);

        if (File.Exists(persistentPath))
        {
            File.Delete(persistentPath);
            Debug.Log("[AnalyticsManager] Cleared analytics data from persistent data path");
        }

        if (File.Exists(assetsPath))
        {
            File.Delete(assetsPath);
            Debug.Log("[AnalyticsManager] Cleared analytics data from assets directory");
        }

        AssetDatabase.Refresh();
    }
#endif
} 