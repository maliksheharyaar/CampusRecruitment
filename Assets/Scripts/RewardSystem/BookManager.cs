using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

[Serializable]
public class Page
{
    public int pageNumber;
    public string pageTitle;
    public string pageContent;
    public bool isCollected = false;
}

public class BookManager : MonoBehaviour
{
    // Singleton instance
    public static BookManager Instance { get; private set; }

    [Header("Page Configuration")]
    [SerializeField] private List<Page> allPages = new List<Page>();
    [SerializeField] private int requiredCoinsForBook = 50;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinCountText;
    [SerializeField] private TextMeshProUGUI pageCountText;
    [SerializeField] private Button craftBookButton;
    [SerializeField] private TextMeshProUGUI craftButtonText;
    
    [Header("Page List UI")]
    [SerializeField] private Transform pageListContainer;
    [SerializeField] private GameObject pageButtonPrefab;
    [SerializeField] private ScrollRect pageListScrollRect;

    [Header("Book View UI")]
    [SerializeField] private GameObject bookViewPanel;
    [SerializeField] private TextMeshProUGUI bookPageTitle;
    [SerializeField] private TextMeshProUGUI bookPageContent;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private TextMeshProUGUI pageNumberText;
    [SerializeField] private Button closeBookButton;

    [Header("Page Detail UI")]
    [SerializeField] private GameObject pageDetailPanel;
    [SerializeField] private TextMeshProUGUI pageDetailTitle;
    [SerializeField] private TextMeshProUGUI pageDetailContent;
    [SerializeField] private Button closePageDetailButton;

    // Private variables
    private int currentCoins = 0;
    private int currentPageIndex = 0;
    private bool isBookCrafted = false;
    private List<Page> collectedPages = new List<Page>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load saved data
        LoadPlayerData();
    }

    private void Start()
    {
        // Load saved data
        LoadPlayerData();
        
        // Check for pending rewards from EndlessRunner
        if (PlayerPrefs.GetInt("HasPendingRewards", 0) == 1)
        {
            int pendingCoins = PlayerPrefs.GetInt("PendingRewardCoins", 0);
            int pendingPage = PlayerPrefs.GetInt("PendingRewardPage", -1);
            
            // Clear pending rewards immediately
            PlayerPrefs.DeleteKey("HasPendingRewards");
            PlayerPrefs.DeleteKey("PendingRewardCoins");
            PlayerPrefs.DeleteKey("PendingRewardPage");
            PlayerPrefs.Save(); // Explicitly save for WebGL
            
            // Apply rewards
            if (pendingCoins > 0)
            {
                AddCoins(pendingCoins);
            }
            
            if (pendingPage >= 0)
            {
                CollectPage(pendingPage);
                ShowRewardsNotification(pendingCoins, pendingPage);
            }
            else if (pendingCoins > 0)
            {
                ShowRewardsNotification(pendingCoins, -1);
            }
        }
        
        // Initialize UI
        UpdateUI();
        PopulatePageList();
        SetupButtonListeners();
        Debug.Log("UI initialized");
        // Ensure panels are initially disabled regardless of their state in the editor
        if (bookViewPanel) bookViewPanel.SetActive(false);
        if (pageDetailPanel) pageDetailPanel.SetActive(false);

        // Listen to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Remove scene change listener
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Clean up button listeners
        RemoveButtonListeners();
    }

    // Set up all button listeners
    private void SetupButtonListeners()
    {
        Debug.Log("Setting up button listeners");
        if (craftBookButton)
        {
            Debug.Log("Setting up craftBookButton listener");
            craftBookButton.onClick.AddListener(OnCraftBookClicked);
        }
        else
        {
            Debug.LogWarning("craftBookButton is not assigned");
        }
        
        if (nextPageButton)
        {
            Debug.Log("Setting up nextPageButton listener");
            nextPageButton.onClick.AddListener(OnNextPageClicked);
        }
        else
        {
            Debug.LogWarning("nextPageButton is not assigned");
        }
        if (prevPageButton)
        {
            Debug.Log("Setting up prevPageButton listener");
            prevPageButton.onClick.AddListener(OnPrevPageClicked);
        }
        else
        {
            Debug.LogWarning("prevPageButton is not assigned");
        }
        if (closeBookButton)
        {
            Debug.Log("Setting up closeBookButton listener");
            closeBookButton.onClick.AddListener(CloseBookView);
        }
        else
        {
            Debug.LogWarning("closeBookButton is not assigned");
        }
        if (closePageDetailButton)
        {
            Debug.Log("Setting up closePageDetailButton listener");
            closePageDetailButton.onClick.AddListener(ClosePageDetail);
        }
        else
        {
            Debug.LogWarning("closePageDetailButton is not assigned");
        }
    }

    private void RemoveButtonListeners()
    {
        Debug.Log("Removing button listeners");
        if (craftBookButton)
            craftBookButton.onClick.RemoveAllListeners();
        
        if (nextPageButton)
            nextPageButton.onClick.RemoveAllListeners();
        
        if (prevPageButton)
            prevPageButton.onClick.RemoveAllListeners();
        
        if (closeBookButton)
            closeBookButton.onClick.RemoveAllListeners();
        
        if (closePageDetailButton)
            closePageDetailButton.onClick.RemoveAllListeners();
    }

    // Modify the OnSceneLoaded method to properly handle returning to CanvasTestScene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "EndlessRunner")
        {
            // Find the EndlessRunnerManager and pass available pages
            StudentRecruitment.EndlessRunner.EndlessRunnerManager runnerManager = 
                FindObjectOfType<StudentRecruitment.EndlessRunner.EndlessRunnerManager>();
            
            if (runnerManager != null)
            {
                // Make the EndlessRunnerManager aware of our pages
                EndlessRunnerRewards rewardsManager = runnerManager.GetComponent<EndlessRunnerRewards>();
                if (rewardsManager != null)
                {
                    rewardsManager.SetAvailablePageIndices(GetUncollectedPageIndices());
                    Debug.Log("EndlessRunner scene loaded - pages data passed to rewards manager");
                }
                else
                {
                    Debug.Log("EndlessRunnerRewards component not found on EndlessRunnerManager");
                }
            }
        }
        else if (scene.name == "CanvasTestScene")
        {
            // Ensure cursor is unlocked and visible in CanvasTestScene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // When returning to the main scene, we need to reconnect all references
            StartCoroutine(ReconnectUIReferences());
        }
    }
    
    // Modify the ReconnectUIReferences coroutine to respect panel states
    private IEnumerator ReconnectUIReferences()
    {
        // Wait for end of frame to ensure all scene objects are loaded
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Reconnecting UI references in CanvasTestScene");
        
        // Find UI references again since they're lost when the scene reloads
        FindUIReferences();
        
        // Remove any existing listeners before adding new ones
        RemoveButtonListeners();
        
        // Reload data from PlayerPrefs
        LoadPlayerData();
        
        // Update the UI with current values
        UpdateUI();
        
        // Repopulate page list
        PopulatePageList();
        
        // Check for pending rewards
        CheckPendingRewards();
        
        // Ensure panels are initially disabled
        if (bookViewPanel) bookViewPanel.SetActive(false);
        if (pageDetailPanel) pageDetailPanel.SetActive(false);
        
        // Set up button listeners after everything is initialized
        SetupButtonListeners();
        
        Debug.Log("UI initialization complete, panels set to disabled state");
    }
    
    // Modify FindUIReferences to also find inactive GameObjects
    private void FindUIReferences()
    {
        Debug.Log("Starting FindUIReferences");
        
        // Find UI references by tag
        FindUIElementByTag(ref coinCountText, "CoinCountText", "TextMeshProUGUI");
        FindUIElementByTag(ref pageCountText, "PageCountText", "TextMeshProUGUI");
        FindUIElementByTag(ref craftBookButton, "CraftBookButton", "Button");
        FindUIElementByTag(ref pageListContainer, "PageListContainer", "Transform");
        FindUIElementByTag(ref pageListScrollRect, "PageListScrollRect", "ScrollRect");
        
        // Find craft button text if we have the button
        if (craftBookButton != null && craftButtonText == null)
        {
            craftButtonText = craftBookButton.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log($"CraftButtonText found: {craftButtonText != null}");
        }
        
        // Find panels
        FindPanel(ref bookViewPanel, "BookViewPanel");
        FindPanel(ref pageDetailPanel, "PageDetailPanel");
        
        // Find book view elements
        if (bookViewPanel != null)
        {
            Debug.Log("Searching for book view elements");
            FindUIElementInPanel(ref bookPageTitle, bookViewPanel, "Title", "TextMeshProUGUI");
            FindUIElementInPanel(ref bookPageContent, bookViewPanel, "Content", "TextMeshProUGUI");
            FindUIElementInPanel(ref nextPageButton, bookViewPanel, "NextButton", "Button");
            FindUIElementInPanel(ref prevPageButton, bookViewPanel, "PrevButton", "Button");
            FindUIElementInPanel(ref pageNumberText, bookViewPanel, "PageNumber", "TextMeshProUGUI");
            FindUIElementInPanel(ref closeBookButton, bookViewPanel, "CloseButton", "Button");
        }
        
        // Find page detail elements
        if (pageDetailPanel != null)
        {
            Debug.Log("Searching for page detail elements");
            FindUIElementInPanel(ref pageDetailTitle, pageDetailPanel, "Title", "TextMeshProUGUI");
            FindUIElementInPanel(ref pageDetailContent, pageDetailPanel, "Content", "TextMeshProUGUI");
            FindUIElementInPanel(ref closePageDetailButton, pageDetailPanel, "CloseButton", "Button");
        }
    }
    
    private void FindUIElementByTag<T>(ref T component, string tag, string componentType) where T : Component
    {
        if (component == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(tag);
            if (obj != null)
            {
                component = obj.GetComponent<T>();
                Debug.Log($"{tag} found: {component != null}");
            }
            else
            {
                Debug.LogWarning($"No GameObject found with tag: {tag}");
            }
        }
    }

    private void FindPanel(ref GameObject panel, string panelName)
    {
        if (panel == null)
        {
            // Try finding by tag first
            panel = GameObject.FindWithTag(panelName);
            Debug.Log($"{panelName} found by tag: {panel != null}");
            
            // If not found, try finding by name
            if (panel == null)
            {
                panel = FindInActiveObjectByName(panelName);
                Debug.Log($"{panelName} found by name: {panel != null}");
            }
        }
    }

    private void FindUIElementInPanel<T>(ref T component, GameObject panel, string elementName, string componentType) where T : Component
    {
        if (component == null)
        {
            // Try finding by exact name first
            component = panel.transform.Find(elementName)?.GetComponent<T>();
            Debug.Log($"{elementName} found by name: {component != null}");
            
            // If not found, try finding by component type
            if (component == null)
            {
                component = panel.GetComponentInChildren<T>(true);
                Debug.Log($"{elementName} found by component type: {component != null}");
            }
            
            // If still not found, try finding by name pattern
            if (component == null)
            {
                T[] allComponents = panel.GetComponentsInChildren<T>(true);
                foreach (T comp in allComponents)
                {
                    if (comp.name.Contains(elementName))
                    {
                        component = comp;
                        Debug.Log($"{elementName} found by name pattern: {comp.name}");
                        break;
                    }
                }
            }
        }
    }

    // Helper method to find inactive GameObjects by name
    private GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }

    // Update LoadPlayerData to handle reloading properly
    private void LoadPlayerData()
    {
        Debug.Log("Loading player data from PlayerPrefs");
        
        currentCoins = PlayerPrefs.GetInt("CurrentCoins", 0);
        isBookCrafted = PlayerPrefs.GetInt("IsBookCrafted", 0) == 1;

        // Load collected pages
        for (int i = 0; i < allPages.Count; i++)
        {
            allPages[i].isCollected = PlayerPrefs.GetInt($"Page_{i}_Collected", 0) == 1;
        }
        
        Debug.Log($"Data loaded: {currentCoins} coins, {CountCollectedPages()}/{allPages.Count} pages, Book crafted: {isBookCrafted}");
    }
    
    // Helper method to count collected pages
    private int CountCollectedPages()
    {
        int count = 0;
        foreach (Page page in allPages)
        {
            if (page.isCollected)
                count++;
        }
        return count;
    }

    // Check for and process any pending rewards from EndlessRunner
    private void CheckPendingRewards()
    {
        if (PlayerPrefs.GetInt("HasPendingRewards", 0) == 1)
        {
            int pendingCoins = PlayerPrefs.GetInt("PendingRewardCoins", 0);
            int pendingPage = PlayerPrefs.GetInt("PendingRewardPage", -1);
            
            // Clear pending rewards flag
            PlayerPrefs.SetInt("HasPendingRewards", 0);
            PlayerPrefs.Save();
            
            // Process rewards
            if (pendingCoins > 0)
            {
                AddCoins(pendingCoins);
            }
            
            if (pendingPage >= 0 && pendingPage < allPages.Count)
            {
                CollectPage(pendingPage);
                
                // Show notification
                ShowRewardsNotification(pendingCoins, pendingPage);
            }
            else
            {
                // Show notification for coins only
                ShowRewardsNotification(pendingCoins, -1);
            }
        }
    }
    
    // Public version of GetUncollectedPageIndices for external use
    public List<int> GetUncollectedPageIndices()
    {
        List<int> uncollectedIndices = new List<int>();
        for (int i = 0; i < allPages.Count; i++)
        {
            if (!allPages[i].isCollected)
            {
                uncollectedIndices.Add(i);
            }
        }
        return uncollectedIndices;
    }
    
    // Display a notification with rewards earned
    private void ShowRewardsNotification(int coins, int pageIndex)
    {
        if (coins > 0 || pageIndex >= 0)
        {
            string message = $"You earned {coins} coins";
            if (pageIndex >= 0 && pageIndex < allPages.Count)
            {
                message += $" and page #{pageIndex + 1}: {allPages[pageIndex].pageTitle}!";
                
                // Update analytics with the page title
                var analyticsManager = FindObjectOfType<AnalyticsManager>();
                if (analyticsManager != null)
                {
                    analyticsManager.UpdateLastPageTitle(allPages[pageIndex].pageTitle);
                }
            }
            else if (pageIndex >= 0)
            {
                message += " and a new page!";
            }
            else
            {
                message += "!";
            }
            
            Debug.Log($"REWARD NOTIFICATION: {message}");
            // Show the notification UI here
        }
    }
    
    // Hide the reward notification after a delay
    private void HideRewardNotification()
    {
        // rewardNotificationPanel.SetActive(false);
    }

    // Update UI elements
    private void UpdateUI()
    {
        // Update coin count text
        if (coinCountText)
            coinCountText.text = $"{currentCoins}/{requiredCoinsForBook}";

        // Count collected pages
        int collectedPageCount = 0;
        foreach (Page page in allPages)
        {
            if (page.isCollected)
                collectedPageCount++;
        }

        // Update page count text
        if (pageCountText)
            pageCountText.text = $"{collectedPageCount}/{allPages.Count}";

        // Update craft button
        if (craftBookButton && craftButtonText)
        {
            bool canCraftBook = collectedPageCount == allPages.Count && currentCoins >= requiredCoinsForBook;
            
            craftBookButton.interactable = isBookCrafted || canCraftBook;
            craftButtonText.text = isBookCrafted ? "View Book" : "Craft Book";
        }
    }

    // Populate the page list with buttons
    private void PopulatePageList()
    {
        if (pageListContainer == null || pageButtonPrefab == null)
            return;

        // Clear existing children
        foreach (Transform child in pageListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each page
        for (int i = 0; i < allPages.Count; i++)
        {
            Page page = allPages[i];
            GameObject buttonObj = Instantiate(pageButtonPrefab, pageListContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText)
                buttonText.text = $"#{page.pageNumber}: {page.pageTitle}";

            button.interactable = page.isCollected;

            // Set up button click event - Use a local variable to avoid closure issues
            int index = i;
            button.onClick.AddListener(() => OnPageButtonClicked(index));
        }
    }

    // Handle page button click
    private void OnPageButtonClicked(int pageIndex)
    {
        // Show page detail panel
        if (pageDetailPanel && pageIndex >= 0 && pageIndex < allPages.Count)
        {
            Page selectedPage = allPages[pageIndex];
            pageDetailPanel.SetActive(true);
            
            if (pageDetailTitle)
                pageDetailTitle.text = selectedPage.pageTitle;
            
            if (pageDetailContent)
                pageDetailContent.text = selectedPage.pageContent;

            // Track page view in analytics
            var analyticsManager = FindObjectOfType<AnalyticsManager>();
            if (analyticsManager != null)
            {
                analyticsManager.TrackPageView(selectedPage.pageNumber, selectedPage.pageTitle);
            }
        }
    }

    // Handle craft book button click
    private void OnCraftBookClicked()
    {
        if (isBookCrafted)
        {
            // If already crafted, just show the book
            ShowBookView();
        }
        else
        {
            // Check if we can craft the book
            int collectedPageCount = 0;
            foreach (Page page in allPages)
            {
                if (page.isCollected)
                    collectedPageCount++;
            }

            if (collectedPageCount == allPages.Count && currentCoins >= requiredCoinsForBook)
            {
                // Craft the book
                isBookCrafted = true;
                currentCoins -= requiredCoinsForBook;
                SavePlayerData();
                UpdateUI();

                // Show the book
                ShowBookView();
            }
        }
    }

    // Show the book view panel
    private void ShowBookView()
    {
        if (bookViewPanel)
        {
            UpdateCollectedPagesList();
            bookViewPanel.SetActive(true);
            currentPageIndex = 0;
            DisplayCurrentPage();
        }
    }

    // Close the book view panel
    private void CloseBookView()
    {
        if (bookViewPanel)
            bookViewPanel.SetActive(false);
    }

    // Close the page detail panel
    public void ClosePageDetail()
    {
        if (pageDetailPanel)
        {   Debug.Log("Closing page detail panel");
            pageDetailPanel.SetActive(false);
        }
    }

    // Display the current page in the book view
    private void DisplayCurrentPage()
    {
        // Make sure currentPageIndex is valid
        if (collectedPages.Count == 0)
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, collectedPages.Count - 1);
        Page currentPage = collectedPages[currentPageIndex];

        // Update UI
        if (bookPageTitle)
            bookPageTitle.text = currentPage.pageTitle;
        
        if (bookPageContent)
            bookPageContent.text = currentPage.pageContent;
        
        if (pageNumberText)
            pageNumberText.text = $"Page {currentPageIndex + 1}/{collectedPages.Count}";

        // Update button interactability
        if (prevPageButton)
            prevPageButton.interactable = currentPageIndex > 0;
        
        if (nextPageButton)
            nextPageButton.interactable = currentPageIndex < collectedPages.Count - 1;
    }

    // Add this method to update collected pages list
    private void UpdateCollectedPagesList()
    {
        collectedPages.Clear();
        foreach (Page page in allPages)
        {
            if (page.isCollected)
                collectedPages.Add(page);
        }
        collectedPages.Sort((a, b) => a.pageNumber.CompareTo(b.pageNumber));
    }

    // Handle next page button click
    private void OnNextPageClicked()
    {
        if (currentPageIndex < collectedPages.Count - 1)
        {
            currentPageIndex++;
            DisplayCurrentPage();
        }
    }

    // Handle previous page button click
    private void OnPrevPageClicked()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            DisplayCurrentPage();
        }
    }

    // Add coins to the player's total
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        SavePlayerData();
        UpdateUI();
    }

    // Add a collected page
    public void CollectPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < allPages.Count)
        {
            allPages[pageIndex].isCollected = true;
            SavePlayerData();
            UpdateUI();
            PopulatePageList();
        }
    }

    // Handle rewards from the EndlessRunner scene
    public void HandleEndlessRunnerRewards(int coinsEarned, int pageIndex)
    {
        AddCoins(coinsEarned);
        CollectPage(pageIndex);
        Debug.Log($"Received rewards from EndlessRunner: {coinsEarned} coins and page #{pageIndex}");
    }

    // Save player data to PlayerPrefs
    private void SavePlayerData()
    {
        PlayerPrefs.SetInt("CurrentCoins", currentCoins);
        PlayerPrefs.SetInt("IsBookCrafted", isBookCrafted ? 1 : 0);
        
        // Save collected pages
        for (int i = 0; i < allPages.Count; i++)
        {
            PlayerPrefs.SetInt($"Page_{i}_Collected", allPages[i].isCollected ? 1 : 0);
        }
        
        // Always save PlayerPrefs explicitly for WebGL
        PlayerPrefs.Save();
    }

    // Load the EndlessRunner scene
    public void StartEndlessRunner()
    {
        SceneManager.LoadScene("EndlessRunner");
    }

    // Add ContextMenu attribute to make ResetAllData accessible from inspector
    [ContextMenu("Reset All Game Data")]
    public void ResetAllData()
    {
        // Reset coins
        currentCoins = 0;
        
        // Reset book crafted state
        isBookCrafted = false;
        
        // Reset all pages to uncollected
        foreach (Page page in allPages)
        {
            page.isCollected = false;
        }
        
        // Save the reset data
        SavePlayerData();
        
        // Update UI to reflect changes
        UpdateUI();
        PopulatePageList();
        
        Debug.Log("All data has been reset: coins and pages cleared");
    }

    // Get the title of a page by its index
    public string GetPageTitle(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < allPages.Count)
        {
            return allPages[pageIndex].pageTitle;
        }
        return null;
    }

    // Add this method to check if the craft button is enabled
    public bool IsCraftButtonEnabled()
    {
        return craftBookButton != null && craftBookButton.interactable;
    }
} 