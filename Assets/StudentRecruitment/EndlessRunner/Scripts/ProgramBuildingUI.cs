using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace StudentRecruitment.EndlessRunner
{
    public class ProgramBuildingUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject programPanel;
        [SerializeField] private TextMeshProUGUI programTitleText;
        [SerializeField] private TextMeshProUGUI programDescriptionText;
        [SerializeField] private Button playMiniGameButton;
        [SerializeField] private Button craftBookButton;
        [SerializeField] private Button closeButton;
        
        [Header("Pages Collection UI")]
        [SerializeField] private GameObject[] pageIcons; // Array of page UI elements
        [SerializeField] private TextMeshProUGUI[] pageTexts; // Array of page texts/descriptions
        [SerializeField] private GameObject pagesPanel;
        
        [Header("Book UI")]
        [SerializeField] private GameObject bookPanel;
        [SerializeField] private TextMeshProUGUI bookContentText;
        
        [Header("Crafting")]
        [SerializeField] private TextMeshProUGUI craftingRequirementsText;
        [SerializeField] private TextMeshProUGUI currentCoinsText;
        
        [Header("Program Settings")]
        [SerializeField] private string programTitle = "Business Program";
        [SerializeField] private string programDescription = "Learn about the Business Program through our fun interactive challenge!";
        [SerializeField] private string miniGameSceneName = "MiniGameScene";
        
        [Header("Book Content")]
        [TextArea(5, 10)]
        [SerializeField] private string bookContent = "Congratulations on completing the Business Program Book!\n\nThe Business program at our university offers comprehensive training in economics, management, marketing, and entrepreneurship. Our graduates go on to successful careers in finance, consulting, and corporate leadership.";
        
        // Page content
        [Header("Page Content")]
        [TextArea(3, 5)]
        [SerializeField] private string[] pageDescriptions = new string[5] {
            "Page 1: Introduction to Business - An overview of our Business program and its focus areas.",
            "Page 2: Faculty Highlights - Learn about our award-winning Business faculty members.",
            "Page 3: Career Opportunities - Discover the various career paths available to Business graduates.",
            "Page 4: Student Success Stories - Read testimonials from successful Business program alumni.",
            "Page 5: Program Requirements - Details about courses, internships, and graduation requirements."
        };
        
        private void Start()
        {
            // Set up button listeners
            if (playMiniGameButton != null)
                playMiniGameButton.onClick.AddListener(LaunchMiniGame);
                
            if (craftBookButton != null)
                craftBookButton.onClick.AddListener(TryCraftBook);
                
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseUI);
                
            // Initialize UI to be hidden
            if (programPanel != null)
                programPanel.SetActive(false);
        }
        
        public void ShowProgramUI()
        {
            // Show the main panel
            if (programPanel != null)
                programPanel.SetActive(true);
                
            // Set program info
            if (programTitleText != null)
                programTitleText.text = programTitle;
                
            if (programDescriptionText != null)
                programDescriptionText.text = programDescription;
                
            // Update the pages UI
            UpdatePagesUI();
            
            // Update book UI
            UpdateBookUI();
            
            // Update coin display
            UpdateCoinDisplay();
            
            // Update crafting requirements
            UpdateCraftingRequirements();
        }
        
        private void UpdatePagesUI()
        {
            int collectedPagesCount = GameProgress.GetBusinessPagesCount();
            
            // Enable pages panel if there are any collected pages
            if (pagesPanel != null)
                pagesPanel.SetActive(collectedPagesCount > 0 || GameProgress.IsBusinessBookCrafted());
            
            // Update each page icon's state
            for (int i = 0; i < pageIcons.Length && i < 5; i++)
            {
                bool hasPage = GameProgress.HasBusinessPage(i);
                
                // Enable collected pages, disable missing ones
                if (pageIcons[i] != null)
                    pageIcons[i].SetActive(hasPage);
                
                // Set page text if available
                if (i < pageTexts.Length && pageTexts[i] != null)
                {
                    pageTexts[i].text = hasPage ? pageDescriptions[i] : "??? (Page not yet collected)";
                }
            }
        }
        
        private void UpdateBookUI()
        {
            bool bookCrafted = GameProgress.IsBusinessBookCrafted();
            
            // Show/hide the book panel based on whether it's been crafted
            if (bookPanel != null)
                bookPanel.SetActive(bookCrafted);
                
            // Update book content
            if (bookContentText != null && bookCrafted)
                bookContentText.text = bookContent;
                
            // Update craft button state
            if (craftBookButton != null)
            {
                bool canCraft = !bookCrafted && 
                               GameProgress.HasAllBusinessPages() && 
                               GameProgress.GetCoins() >= 100; // 100 is the craft cost
                
                craftBookButton.interactable = canCraft;
            }
        }
        
        private void UpdateCoinDisplay()
        {
            if (currentCoinsText != null)
                currentCoinsText.text = $"Coins: {GameProgress.GetCoins()}";
        }
        
        private void UpdateCraftingRequirements()
        {
            if (craftingRequirementsText != null)
            {
                var (requiredPages, requiredCoins) = GameProgress.GetBusinessBookRequirements();
                craftingRequirementsText.text = $"Crafting requires: {GameProgress.GetBusinessPagesCount()}/{requiredPages} pages and {requiredCoins} coins";
            }
        }
        
        private void LaunchMiniGame()
        {
            // Hide UI
            if (programPanel != null)
                programPanel.SetActive(false);
                
            // Load mini-game scene
            SceneManager.LoadScene(miniGameSceneName);
        }
        
        private void TryCraftBook()
        {
            if (GameProgress.TryCraftBusinessBook())
            {
                // Play craft sound
                AudioManager.Instance?.PlaySound("BookCraft");
                
                // Update UI to show the crafted book
                UpdateBookUI();
                UpdateCoinDisplay();
            }
            else
            {
                Debug.Log("Failed to craft book - requirements not met");
            }
        }
        
        private void CloseUI()
        {
            if (programPanel != null)
                programPanel.SetActive(false);
        }
    }
} 