using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace StudentRecruitment.EndlessRunner
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainGameUI;
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameObject winUI;
        [SerializeField] private GameObject pauseMenuUI;
        
        [Header("In-Game UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image[] hitIndicators;
        [SerializeField] private GameObject invincibilityIndicator;
        [SerializeField] private Slider invincibilityDurationSlider;
        
        [Header("Game Over UI")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button gameOverReturnButton;
        
        [Header("Win UI")]
        [SerializeField] private TextMeshProUGUI coinsAwardedText;
        [SerializeField] private TextMeshProUGUI pageAwardedText;
        [SerializeField] private Button continueButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float scoreCountSpeed = 0.05f;
        
        [Header("HUD Elements")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject[] lifeIcons;
        [SerializeField] private GameObject invincibilityIcon;
        [SerializeField] private GameObject speedBoostIcon;
        [SerializeField] private GameObject extraLifeIcon;
        [SerializeField] private Slider invincibilitySlider;
        [SerializeField] private Slider speedBoostSlider;
        
        [Header("Game State Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject newPageNotification;
        
        // References
        private EndlessRunnerManager runnerManager;
        private RunnerController playerController;
        
        // Keep track of power-up coroutines to cancel them
        private Coroutine invincibilityCoroutine;
        private Coroutine speedBoostCoroutine;
        
        // Flag to track if this UIManager is valid or being destroyed
        private bool isDestroyed = false;
        
        // Add variable for return scene name
        [SerializeField] private string returnSceneName = "CanvasTestScene";
        
        private void Awake()
        {
            // Initialize references
            runnerManager = FindObjectOfType<EndlessRunnerManager>();
            playerController = FindObjectOfType<RunnerController>();
            
            // Hide all non-main panels initially
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (winUI != null) winUI.SetActive(false);
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            if (invincibilityIndicator != null) invincibilityIndicator.SetActive(false);
        }
        
        private void Start()
        {
            // Initialize score
            UpdateScoreDisplay(0);
            
            // Set up button listeners
            if (retryButton != null)
                retryButton.onClick.AddListener(RestartGame);
                
            if (gameOverReturnButton != null)
                gameOverReturnButton.onClick.AddListener(ReturnToMainMenu);
                
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            
            // Initialize UI
            UpdateLives(3); // assuming 3 is the default
            
            // Hide all power-up icons and reset sliders
            if (invincibilityIcon != null) invincibilityIcon.SetActive(false);
            if (speedBoostIcon != null) speedBoostIcon.SetActive(false);
            if (extraLifeIcon != null) extraLifeIcon.SetActive(false);
            if (invincibilitySlider != null) invincibilitySlider.gameObject.SetActive(false);
            if (speedBoostSlider != null) speedBoostSlider.gameObject.SetActive(false);
            
            // Hide all game state panels
            SetAllGamePanelsInactive();
            
            // Show HUD
            if (hudPanel != null) hudPanel.SetActive(true);
        }
        
        private void OnEnable()
        {
            isDestroyed = false;
            
            // Register for events
            RegisterEvents();
        }
        
        private void RegisterEvents()
        {
            // Only register if not already destroyed
            if (isDestroyed) return;
            
            // Register manager events
            if (runnerManager != null)
            {
                EndlessRunnerManager.OnGameStateChanged += UpdateGameState;
                EndlessRunnerManager.OnScoreChanged += UpdateScoreDisplay;
            }
            
            // Register player events
            if (playerController != null)
            {
                RunnerController.OnHealthChanged += UpdateHealthDisplay;
                RunnerController.OnInvincibilityChanged += UpdateInvincibilityDisplay;
                RunnerController.OnLivesChanged += UpdateLives;
                RunnerController.OnPowerUpCollected += HandlePowerUp;
            }
        }
        
        private void OnDisable()
        {
            // Unregister from events
            UnregisterEvents();
            
            // Stop all coroutines when disabled
            StopAllCoroutines();
            
            // Explicitly null out coroutine references
            invincibilityCoroutine = null;
            speedBoostCoroutine = null;
        }
        
        private void UnregisterEvents()
        {
            // Unregister manager events
            EndlessRunnerManager.OnGameStateChanged -= UpdateGameState;
            EndlessRunnerManager.OnScoreChanged -= UpdateScoreDisplay;
            
            // Unregister player events
            RunnerController.OnHealthChanged -= UpdateHealthDisplay;
            RunnerController.OnInvincibilityChanged -= UpdateInvincibilityDisplay;
            RunnerController.OnLivesChanged -= UpdateLives;
            RunnerController.OnPowerUpCollected -= HandlePowerUp;
            
            // Remove button listeners if they exist
            if (retryButton != null)
                retryButton.onClick.RemoveAllListeners();
                
            if (gameOverReturnButton != null)
                gameOverReturnButton.onClick.RemoveAllListeners();
                
            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();
        }
        
        private void OnDestroy()
        {
            // Mark as destroyed and stop all coroutines
            isDestroyed = true;
            
            // Stop any active coroutines
            StopAllCoroutines();
            
            // Make sure we're unregistered from events
            UnregisterEvents();
            
            // Clear references
            runnerManager = null;
            playerController = null;
            
            // Force garbage collection to clean up any lingering allocations
            System.GC.Collect();
        }
        
        // Display score
        public void UpdateScoreDisplay(int score)
        {
            if (isDestroyed) return;
            
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
        
        // Update health indicators
        public void UpdateHealthDisplay(int currentHealth, int maxHealth)
        {
            if (isDestroyed) return;
            
            if (hitIndicators != null && hitIndicators.Length > 0)
            {
                // Make sure we don't exceed the array bounds
                int indicatorCount = Mathf.Min(maxHealth, hitIndicators.Length);
                
                for (int i = 0; i < indicatorCount; i++)
                {
                    // Set active if health is above this indicator's threshold
                    if (hitIndicators[i] != null)
                    {
                        hitIndicators[i].gameObject.SetActive(i < currentHealth);
                    }
                }
            }
        }
        
        // Update invincibility indicator
        public void UpdateInvincibilityDisplay(bool isInvincible, float remainingDuration, float totalDuration)
        {
            if (isDestroyed) return;
            
            if (invincibilityIndicator != null)
            {
                invincibilityIndicator.SetActive(isInvincible);
                
                if (invincibilityDurationSlider != null && isInvincible)
                {
                    invincibilityDurationSlider.maxValue = totalDuration;
                    invincibilityDurationSlider.value = remainingDuration;
                }
            }
        }
        
        // Update lives display
        private void UpdateLives(int currentLives)
        {
            if (isDestroyed) return;
            
            if (lifeIcons == null || lifeIcons.Length == 0) return;
            
            // Update life icons (activate or deactivate based on current lives)
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].SetActive(i < currentLives);
                }
            }
        }
        
        // Handle power-up collection
        private void HandlePowerUp(PowerUpType type)
        {
            if (isDestroyed) return;
            
            try
            {
                // Handle different power-up types
                switch (type)
                {
                    case PowerUpType.Invincibility:
                        ShowPowerUpEffect(invincibilityIcon, invincibilitySlider, ref invincibilityCoroutine);
                        break;
                    case PowerUpType.SpeedBoost:
                        ShowPowerUpEffect(speedBoostIcon, speedBoostSlider, ref speedBoostCoroutine);
                        break;
                    case PowerUpType.ExtraLife:
                        // For extra life, just flash the icon briefly
                        if (extraLifeIcon != null && !isDestroyed)
                        {
                            StartCoroutine(FlashIcon(extraLifeIcon, 1.0f));
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error handling power-up: " + e.Message);
            }
        }
        
        // Show power-up effect with icon and slider
        private void ShowPowerUpEffect(GameObject icon, Slider slider, ref Coroutine coroutine)
        {
            if (isDestroyed) return;
            
            try
            {
                // Get duration from EndlessRunnerManager
                float duration = 5f;
                if (EndlessRunnerManager.Instance != null)
                {
                    duration = EndlessRunnerManager.Instance.PowerUpDuration;
                }
                
                if (mainGameUI != null) mainGameUI.SetActive(true);
                if (gameOverUI != null) gameOverUI.SetActive(false);
                if (winUI != null) winUI.SetActive(false);
                if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
                
                // Cancel existing coroutine if one is running
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }
                
                // Start new coroutine only if not destroyed
                if (!isDestroyed)
                {
                    coroutine = StartCoroutine(PowerUpEffectCoroutine(icon, slider, duration));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error showing power-up effect: " + e.Message);
            }
        }
        
        // Coroutine to handle power-up UI effect
        private IEnumerator PowerUpEffectCoroutine(GameObject icon, Slider slider, float duration)
        {
            // Check for destroyed component at each yield point
            if (isDestroyed) yield break;
            
            // Show icon and slider
            if (icon != null) icon.SetActive(true);
            if (slider != null)
            {
                slider.gameObject.SetActive(true);
                slider.maxValue = duration;
                slider.value = duration;
            }
            
            // Count down timer
            float timer = duration;
            while (timer > 0 && !isDestroyed)
            {
                timer -= Time.deltaTime;
                if (slider != null && !isDestroyed) slider.value = timer;
                yield return null;
                
                // Check if we've been destroyed during this frame
                if (isDestroyed) yield break;
            }
            
            // Hide icon and slider
            if (icon != null && !isDestroyed) icon.SetActive(false);
            if (slider != null && !isDestroyed) slider.gameObject.SetActive(false);
        }
        
        // Flash an icon briefly
        private IEnumerator FlashIcon(GameObject icon, float duration)
        {
            if (isDestroyed) yield break;
            
            icon.SetActive(true);
            
            float timer = duration;
            float flashRate = 0.1f;
            bool isVisible = true;
            
            while (timer > 0 && !isDestroyed)
            {
                timer -= flashRate;
                isVisible = !isVisible;
                if (icon != null && !isDestroyed) icon.SetActive(isVisible);
                yield return new WaitForSeconds(flashRate);
                
                // Check if we've been destroyed
                if (isDestroyed) yield break;
            }
            
            if (icon != null && !isDestroyed) icon.SetActive(false);
        }
        
        // Update UI based on game state
        public void UpdateGameState(GameState newState)
        {
            // Hide all panels first
            HideAllPanels();
            
            // Show appropriate panel
            switch (newState)
            {
                case GameState.Running:
                    if (mainGameUI != null) mainGameUI.SetActive(true);
                    break;
                    
                case GameState.GameOver:
                    ShowGameOver();
                    break;
                    
                case GameState.Win:
                    ShowVictory();
                    break;
                    
                case GameState.Paused:
                    if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
                    break;
            }
        }
        
        private void HideAllPanels()
        {
            if (mainGameUI != null) mainGameUI.SetActive(false);
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (winUI != null) winUI.SetActive(false);
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        }
        
        // Show game over panel
        private void ShowGameOver()
        {
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
                
                // Update final score
                if (finalScoreText != null && runnerManager != null)
                {
                    int finalScore = runnerManager.CurrentScore;
                    finalScoreText.text = $"Final Score: {finalScore}";
                }
            }
        }
        
        // Show victory panel
        private void ShowVictory()
        {
            if (winUI != null)
            {
                winUI.SetActive(true);
                
                // Set reward text
                if (coinsAwardedText != null && runnerManager != null)
                {
                    int coinsAwarded = runnerManager.CalculateCoinsReward();
                    StartCoroutine(AnimateCountingText(coinsAwardedText, 0, coinsAwarded, "Coins Awarded: "));
                }
                
                if (pageAwardedText != null && runnerManager != null)
                {
                    bool pageCollected = runnerManager.HasCollectedPage;
                    pageAwardedText.text = pageCollected ? "Notebook Page: Collected!" : "Notebook Page: Not Found";
                    pageAwardedText.color = pageCollected ? Color.green : Color.red;
                }
            }
        }
        
        // Helper to set all game panels inactive
        private void SetAllGamePanelsInactive()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        }
        
        // Restart button handler
        private void RestartGame()
        {
            if (runnerManager != null)
            {
                runnerManager.RestartGame();
            }
        }
        
        // Return to main menu handler
        private void ReturnToMainMenu()
        {
            if (runnerManager != null)
            {
                runnerManager.ReturnToMainMenu();
            }
        }
        
        // Animate counting text for rewards
        private IEnumerator AnimateCountingText(TextMeshProUGUI textElement, int startValue, int endValue, string prefix)
        {
            float currentValue = startValue;
            float duration = Mathf.Abs(endValue - startValue) * scoreCountSpeed;
            float elapsedTime = 0;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                currentValue = Mathf.Lerp(startValue, endValue, t);
                textElement.text = $"{prefix}{Mathf.FloorToInt(currentValue)}";
                yield return null;
            }
            
            textElement.text = $"{prefix}{endValue}";
        }
        
        // Show pause UI
        public void ShowPauseUI()
        {
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(true);
            }
            
            // Hide other panels
            if (mainGameUI != null) mainGameUI.SetActive(false);
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (winUI != null) winUI.SetActive(false);
        }
        
        // Hide pause UI
        public void HidePauseUI()
        {
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(false);
            }
            
            // Show main game UI
            if (mainGameUI != null) mainGameUI.SetActive(true);
        }
        
        // Add method to handle continue button click
        public void OnContinueButtonClicked()
        {
            // Check if there's a rewards manager to get the scene name
            EndlessRunnerRewards rewardsManager = FindObjectOfType<EndlessRunnerRewards>();
            if (rewardsManager != null)
            {
                // Use the scene name from rewards manager
                returnSceneName = rewardsManager.GetCanvasSceneName();
            }

            // Load the return scene
            SceneManager.LoadScene(returnSceneName);
            Debug.Log($"Returning to scene: {returnSceneName}");
        }
        
        // Update the GameOver method to reference the continue button
        public void GameOver()
        {
            if (isDestroyed) return;
            
            // First hide all panels
            HideAllPanels();
            
            // Show the game over panel
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
                
                // Find the continue button if it exists
                Button continueButton = gameOverUI.GetComponentInChildren<Button>();
                if (continueButton != null)
                {
                    // Add onClick listener
                    continueButton.onClick.RemoveAllListeners(); 
                    continueButton.onClick.AddListener(OnContinueButtonClicked);
                }
            }
            
            // Hide the main game UI
            if (mainGameUI != null) mainGameUI.SetActive(false);
        }
        
        // Update the Win method to reference the continue button
        public void Win()
        {
            if (isDestroyed) return;
            
            // First hide all panels
            HideAllPanels();
            
            // Show the win panel
            if (winUI != null)
            {
                winUI.SetActive(true);
                
                // Find the continue button if it exists
                Button continueButton = winUI.GetComponentInChildren<Button>();
                if (continueButton != null)
                {
                    // Add onClick listener
                    continueButton.onClick.RemoveAllListeners();
                    continueButton.onClick.AddListener(OnContinueButtonClicked);
                }
            }
            
            // Hide the main game UI
            if (mainGameUI != null) mainGameUI.SetActive(false);
        }
    }
} 