using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace StudentRecruitment.EndlessRunner
{
    public class PauseManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button returnToMainMenuButton;

        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey;
        
        private bool isPaused = false;
        private UIManager uiManager;
        private EndlessRunnerManager runnerManager;

        // Event to notify other systems when pause state changes
        public static event Action<bool> OnPauseStateChanged;

        [SerializeField] private GameObject[] obstaclePrefabs;

        private void Awake()
        {
            // Ensure time scale is set to 1 at start
            Time.timeScale = 1f;
            isPaused = false;
            
            // Find references if not assigned
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
            
            if (runnerManager == null)
                runnerManager = FindObjectOfType<EndlessRunnerManager>();
        }

        private void Start()
        {
            // Setup button listeners
            if (resumeButton != null)
                resumeButton.onClick.AddListener(TogglePause);
            
            if (returnToMainMenuButton != null)
                returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);

            // Force unpause at start
            SetPauseState(false);
            
            // Give a small delay before starting the game
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            // Make sure time is running
            Time.timeScale = 1f;
            
            // Wait a frame to let everything initialize
            yield return null;
            yield return new WaitForSeconds(0.2f);
            
            // Double check time scale
            if (Time.timeScale != 1f)
            {
                Debug.LogWarning("Time scale was not 1, resetting to 1");
                Time.timeScale = 1f;
            }
            
            if (runnerManager != null)
            {
                try
                {
                    // Call the StartGame method on the runner manager instance
                    runnerManager.StartGame();
                    // Use UpdateGameState instead of UpdateUI
                    runnerManager.UpdateGameState(GameState.Running);
                    
                    // Make sure we're not in paused state
                    SetPauseState(false);
                    
                    Debug.Log("Game successfully started via PauseManager");
                }
                catch (Exception e)
                {
                    Debug.LogError("Error during delayed start: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("RunnerManager not found! Game may start paused.");
            }
        }

        private void Update()
        {
            // Only allow pausing when the game is active
            if (runnerManager != null)
            {
                GameState currentState = runnerManager.CurrentGameState;
                // Check if the game is in Running state
                bool isRunning = (currentState == GameState.Running);
                
                if (isRunning)
                {
                    // Check for pause key press
                    if (Input.GetKeyDown(pauseKey))
                    {
                        TogglePause();
                    }
                }
            }
            
            // Safety check - if we're not supposed to be paused but timescale is 0, fix it
            if (!isPaused && Time.timeScale == 0f)
            {
                Debug.LogWarning("Time scale was 0 but game is not paused, resetting to 1");
                Time.timeScale = 1f;
            }
        }

        public void TogglePause()
        {
            SetPauseState(!isPaused);
        }

        public void SetPauseState(bool paused)
        {
            // Skip if state hasn't changed
            if (isPaused == paused)
                return;

            isPaused = paused;
            
            // Update time scale
            Time.timeScale = isPaused ? 0f : 1f;
            
            Debug.Log("Pause state changed to: " + isPaused + ", Time.timeScale = " + Time.timeScale);
            
            // Update UI
            if (uiManager != null)
            {
                if (isPaused)
                    uiManager.ShowPauseUI();
                else
                    uiManager.HidePauseUI();
            }
            
            // Update game state in runner manager
            if (runnerManager != null)
            {
                // Update game state
                if (isPaused)
                {
                    runnerManager.UpdateGameState(GameState.Paused);
                }
                else
                {
                    runnerManager.UpdateGameState(GameState.Running);
                }
            }
            
            // Trigger event for other systems
            OnPauseStateChanged?.Invoke(isPaused);
        }

        private void ReturnToMainMenu()
        {
            // Unpause the game first
            SetPauseState(false);
            
            // Tell the runner manager to return to main menu
            if (runnerManager != null)
            {
                // Use ReturnToMainScene method if ReturnToMainMenu doesn't exist
                if (runnerManager.GetType().GetMethod("ReturnToMainMenu") != null)
                {
                    runnerManager.ReturnToMainMenu();
                }
                else if (runnerManager.GetType().GetMethod("ReturnToMainScene") != null)
                {
                    runnerManager.ReturnToMainScene();
                }
            }
        }

        public bool IsPaused()
        {
            return isPaused;
        }
        
        private void OnDisable()
        {
            // Clean up button listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveAllListeners();
                
            if (returnToMainMenuButton != null)
                returnToMainMenuButton.onClick.RemoveAllListeners();
                
            // Make sure time scale is restored when disabled
            Time.timeScale = 1f;
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveAllListeners();
                
            if (returnToMainMenuButton != null)
                returnToMainMenuButton.onClick.RemoveAllListeners();
                
            // Make sure time scale is restored
            Time.timeScale = 1f;
                
            // Clear static event
            OnPauseStateChanged = null;
            
            // Clear references
            uiManager = null;
            runnerManager = null;
            
            // Stop all coroutines
            StopAllCoroutines();
        }
        
        private void OnApplicationQuit()
        {
            // Ensure time scale is reset on application quit
            Time.timeScale = 1f;
        }
    }
} 