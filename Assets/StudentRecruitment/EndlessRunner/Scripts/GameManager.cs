using UnityEngine;
using System.Collections;

namespace StudentRecruitment.EndlessRunner
{
    public class GameManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private GameObject endlessRunnerManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject uiManagerPrefab;
        [SerializeField] private GameObject cursorManagerPrefab;
        [SerializeField] private GameObject pauseManagerPrefab;
        
        [Header("Settings")]
        [SerializeField] private float gameStartDelay = 2f;
        [SerializeField] private bool isDebugMode = false;
        
        private EndlessRunnerManager runnerManager;
        private AudioManager audioManager;
        private UIManager uiManager;
        private CursorManager cursorManager;
        private PauseManager pauseManager;
        
        private void Awake()
        {
            // Initialize all managers if they don't exist
            InitializeManagers();
            
            // Debug mode
            if (isDebugMode)
            {
                Debug.Log("Game Manager: Debug mode enabled");
            }
        }
        
        private void Start()
        {
            // Play background music
            if (audioManager != null)
            {
                audioManager.PlaySound("BackgroundMusic");
            }
            
            // Start the game after a delay
            StartCoroutine(DelayedGameStart());
        }
        
        private IEnumerator DelayedGameStart()
        {
            yield return new WaitForSeconds(gameStartDelay);
            
            // Start the game
            if (runnerManager != null)
            {
                Debug.Log("Game started!");
            }
        }
        
        private void InitializeManagers()
        {
            // Check for EndlessRunnerManager
            runnerManager = FindObjectOfType<EndlessRunnerManager>();
            if (runnerManager == null && endlessRunnerManagerPrefab != null)
            {
                GameObject managerObj = Instantiate(endlessRunnerManagerPrefab);
                managerObj.name = "EndlessRunnerManager";
                runnerManager = managerObj.GetComponent<EndlessRunnerManager>();
            }
            
            // Check for AudioManager
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null && audioManagerPrefab != null)
            {
                GameObject managerObj = Instantiate(audioManagerPrefab);
                managerObj.name = "AudioManager";
                audioManager = managerObj.GetComponent<AudioManager>();
            }
            
            // Check for UIManager
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null && uiManagerPrefab != null)
            {
                GameObject managerObj = Instantiate(uiManagerPrefab);
                managerObj.name = "UIManager";
                uiManager = managerObj.GetComponent<UIManager>();
            }
            
            // Check for CursorManager
            cursorManager = FindObjectOfType<CursorManager>();
            if (cursorManager == null && cursorManagerPrefab != null)
            {
                GameObject managerObj = Instantiate(cursorManagerPrefab);
                managerObj.name = "CursorManager";
                cursorManager = managerObj.GetComponent<CursorManager>();
            }
            
            // Check for PauseManager
            pauseManager = FindObjectOfType<PauseManager>();
            if (pauseManager == null && pauseManagerPrefab != null)
            {
                GameObject managerObj = Instantiate(pauseManagerPrefab);
                managerObj.name = "PauseManager";
                pauseManager = managerObj.GetComponent<PauseManager>();
            }
        }
        
        // Method to reset all progress (for testing)
        public void ResetAllProgress()
        {
            if (isDebugMode)
            {
                GameProgress.ResetAllProgress();
                Debug.Log("All progress has been reset!");
            }
        }
    }
} 