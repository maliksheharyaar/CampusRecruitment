using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    // Scene-specific settings
    [Header("Scene Settings")]
    [SerializeField] private string currentSceneName;
    [SerializeField] private bool lockCursorInThisScene = true;
    [SerializeField] private bool startWithCursorLocked = true;
    
    // Track whether a dialog/conversation is active
    private bool isInDialog = false;
    
    // Expose instance for scene-specific access
    public static CursorManager Instance { get; private set; }
    
    private void Awake()
    {
        // Set as scene's cursor manager
        Instance = this;
        
        // Get current scene name if not set
        if (string.IsNullOrEmpty(currentSceneName))
        {
            currentSceneName = SceneManager.GetActiveScene().name;
        }
        
        // Apply initial cursor state
        if (startWithCursorLocked && lockCursorInThisScene)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        // Don't process input during dialogs
        if (isInDialog) return;
        
        // Only handle cursor in scenes where it should be locked
        if (lockCursorInThisScene)
        {
            // If Escape is pressed, toggle cursor lock state
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    UnlockCursor();
                }
                else
                {
                    LockCursor();
                }
            }

            // If mouse is clicked and cursor is unlocked, lock it again
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }
    }

    public void SetDialogMode(bool inDialog)
    {
        isInDialog = inDialog;
        
        if (inDialog)
        {
            UnlockCursor();
        }
        else if (lockCursorInThisScene)
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
} 