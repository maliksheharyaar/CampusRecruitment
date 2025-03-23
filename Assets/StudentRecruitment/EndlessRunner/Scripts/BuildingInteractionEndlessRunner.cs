using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class BuildingInteractionEndlessRunner : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 5f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private ProgramBuildingUI programUI;
        
        private bool playerInRange = false;
        private Transform playerTransform;
        private GameObject promptInstance;
        
        private void Start()
        {
            // Initialize the UI as hidden
            if (interactionPromptUI != null)
                interactionPromptUI.SetActive(false);
                
            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        private void Update()
        {
            CheckPlayerDistance();
            
            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                Interact();
            }
        }
        
        private void CheckPlayerDistance()
        {
            if (playerTransform == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool inRange = distanceToPlayer <= interactionDistance;
            
            if (inRange != playerInRange)
            {
                playerInRange = inRange;
                
                // Show/hide interaction prompt
                if (interactionPromptUI != null)
                    interactionPromptUI.SetActive(playerInRange);
            }
        }
        
        private void Interact()
        {
            // Unlock cursor for UI interaction
            CursorManager cursorManager = CursorManager.Instance;
            if (cursorManager != null)
                cursorManager.UnlockCursor();
                
            // Show the program UI
            if (programUI != null)
                programUI.ShowProgramUI();
            
            // Hide interaction prompt while UI is open
            if (interactionPromptUI != null)
                interactionPromptUI.SetActive(false);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Visualize the interaction range in the editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
} 