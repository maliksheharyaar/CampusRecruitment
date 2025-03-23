using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }
        
        [SerializeField] private bool lockCursorAtStart = true;
        [SerializeField] private CursorLockMode defaultLockMode = CursorLockMode.Locked;
        [SerializeField] private bool defaultCursorVisibility = false;
        
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
        }
        
        private void Start()
        {
            if (lockCursorAtStart)
            {
                LockCursor();
            }
            else
            {
                UnlockCursor();
            }
        }
        
        public void LockCursor()
        {
            Cursor.lockState = defaultLockMode;
            Cursor.visible = defaultCursorVisibility;
        }
        
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                LockCursor();
            }
            else
            {
                UnlockCursor();
            }
        }
    }
} 