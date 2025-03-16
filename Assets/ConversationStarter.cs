using System.Collections;
using System.Collections.Generic;
using DialogueEditor;
using UnityEngine;
using StudentRecruitment.FinalCharacterController;

[RequireComponent(typeof(Collider))]
public class ConversationStarter : MonoBehaviour
{
    [Header("Conversation Settings")]
    [SerializeField] private NPCConversation _conversation;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Visual Cue (Optional)")]
    [SerializeField] private GameObject interactionPrompt;
    
    private PlayerController _playerController;
    private GameObject _playerObject;
    private bool _isInConversation = false;
    private CursorManager _cursorManager;

    private void Start()
    {
        // Find cursor manager in current scene
        _cursorManager = FindObjectOfType<CursorManager>();
        
        // Hide prompt initially if assigned
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
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
        
        // Start the conversation
        ConversationManager.Instance.StartConversation(_conversation);
        
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
    
    private void HandleConversationEnd()
    {
        if (!_isInConversation) return;
        
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