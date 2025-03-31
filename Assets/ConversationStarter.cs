using System.Collections;
using System.Collections.Generic;
using DialogueEditor;
using UnityEngine;
using StudentRecruitment.FinalCharacterController;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ConversationStarter : MonoBehaviour
{
    [Header("Conversation Settings")]
    [SerializeField] private NPCConversation _conversation;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Visual Cue (Optional)")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Header("NPC Animation")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string talkingParameterName = "IsTalking";
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation in degrees per second
    
    private PlayerController _playerController;
    private GameObject _playerObject;
    private bool _isInConversation = false;
    private CursorManager _cursorManager;
    private Quaternion _initialRotation; // Store the initial rotation
    private Coroutine _rotationCoroutine; // Store the current rotation coroutine

    private void Start()
    {
        // Store initial rotation
        _initialRotation = transform.rotation;
        
        // Find cursor manager in current scene
        _cursorManager = FindObjectOfType<CursorManager>();
        
        // Hide prompt initially if assigned
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // If no animator is assigned, try to get it from this gameObject
        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += HandleConversationEnd;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= HandleConversationEnd;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_isInConversation)
        {
            // Show interaction prompt if assigned
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide interaction prompt if assigned
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && !_isInConversation)
        {
            if(Input.GetKeyDown(interactionKey))
            {
                _playerObject = other.gameObject;
                _playerController = other.GetComponent<PlayerController>();
                
                StartConversation();
            }
        }
    }
    
    private void StartConversation()
    {
        _isInConversation = true;
        
        // Check if _conversation is assigned
        if (_conversation == null)
        {
            Debug.LogError("[ConversationStarter] No conversation assigned to this trigger! Please assign an NPCConversation in the inspector.", this);
            _isInConversation = false;
            return;
        }
        
        // Check if ConversationManager is available
        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[ConversationStarter] ConversationManager.Instance is null. Make sure DialogueEditor is properly initialized.", this);
            _isInConversation = false;
            return;
        }

        // Make NPC face the player smoothly
        if (_playerObject != null)
        {
            // Calculate direction to player
            Vector3 directionToPlayer = _playerObject.transform.position - transform.position;
            directionToPlayer.y = 0; // Keep the rotation only on the Y axis
            
            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            
            // Start smooth rotation
            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
            }
            _rotationCoroutine = StartCoroutine(SmoothRotate(targetRotation));
        }
        
        // Start the conversation
        ConversationManager.Instance.StartConversation(_conversation);
        
        // Start NPC talking animation
        SetNPCTalking(true);
        
        // Disable camera movement and player controller
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }
        
        // Hide the player
        if (_playerObject != null)
        {
            // Disable renderer components to make player invisible
            Renderer[] renderers = _playerObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
        
        // Hide interaction prompt if assigned
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Use scene's CursorManager for cursor control if available
        if (_cursorManager != null)
        {
            _cursorManager.SetDialogMode(true);
        }
        else
        {
            // Fallback if CursorManager not available
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    /// <summary>
    /// Set the NPC talking animation state
    /// </summary>
    /// <param name="isTalking">Whether the NPC should be in talking state</param>
    public void SetNPCTalking(bool isTalking)
    {
        if (npcAnimator != null)
        {
            npcAnimator.SetBool(talkingParameterName, isTalking);
        }
    }
    
    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    
    private void HandleConversationEnd()
    {
        if (!_isInConversation) return;
        
        // Stop NPC talking animation
        SetNPCTalking(false);
        
        // Return to initial rotation smoothly
        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
        }
        _rotationCoroutine = StartCoroutine(SmoothRotate(_initialRotation));
        
        // Re-enable player control
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }
        
        // Show the player
        if (_playerObject != null)
        {
            // Re-enable renderer components
            Renderer[] renderers = _playerObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
        
        // Use scene's CursorManager for cursor control if available
        if (_cursorManager != null)
        {
            _cursorManager.SetDialogMode(false);
        }
        else
        {
            // Fallback if CursorManager not available
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Reset conversation state
        _isInConversation = false;
    }
}