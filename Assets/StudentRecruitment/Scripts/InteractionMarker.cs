using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class InteractionMarker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float hoverHeight = 2f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private float markerScale = 0.01f;
    [SerializeField] private float visibilityDistance = 5f; // Distance at which marker becomes visible
    [SerializeField] private float interactionDistance = 3f; // Distance at which E key works
    [SerializeField] private string targetSceneName = "CanvasTestScene";
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("UI")]
    [SerializeField] private Canvas markerCanvas;
    [SerializeField] private Button markerButton;
    [SerializeField] private Text promptText;
    
    [Header("Prompt Settings")]
    [SerializeField] private string promptMessage = "Press E to interact";
    [SerializeField] private float promptOffset = 50f; // Offset below the marker in UI space
    [SerializeField] private Font promptFont; // Custom font asset
    [SerializeField] private int promptFontSize = 14;
    [SerializeField] private Color promptColor = Color.white;
    [SerializeField] private FontStyle promptFontStyle = FontStyle.Normal;
    [SerializeField] private TextAnchor promptAlignment = TextAnchor.MiddleCenter;
    [SerializeField] private bool useOutline;
    [SerializeField] private Color outlineColor = new Color(0, 0, 0, 1);
    [SerializeField] private float outlineThickness = 0.2f;
    [SerializeField] private Vector2 promptSize = new Vector2(200, 30);
    
    // Performance optimization - cache squared distances to avoid sqrt operations
    [Header("Performance")]
    [SerializeField] private bool useSquaredDistanceCheck = true;
    [SerializeField] private bool webGLOptimized = true; // Enable special optimizations for WebGL
    
    private Transform playerTransform;
    private Vector3 basePosition;
    private Camera mainCamera;
    private float initialY;
    private bool isTransitioning = false;
    private bool isInRange = false;
    private bool isInInteractionRange = false;
    private UnityEvent onClick;
    
    // Cached values for performance
    private float visibilityDistanceSqr;
    private float interactionDistanceSqr;
    private Vector3 playerPositionCache;
    private float distanceSqrCache;
    private float updateInterval = 0.1f; // Check distance every 100ms instead of every frame
    private float lastUpdateTime;
    private ColorBlock buttonColors;
    private Vector2 anchoredPosition = Vector2.zero;
    private Vector3 lookPosition = Vector3.zero;
    private GameObject promptGameObject;
    private RectTransform promptRectTransform;

    private void Awake()
    {
        onClick = new UnityEvent();
        
        // Pre-calculate squared distances
        visibilityDistanceSqr = visibilityDistance * visibilityDistance;
        interactionDistanceSqr = interactionDistance * interactionDistance;
        
        // Cache the button colors
        if (markerButton != null)
        {
            buttonColors = markerButton.colors;
        }
        
        anchoredPosition.y = -promptOffset;
    }

    private void Start()
    {
        FindPlayer();
        mainCamera = Camera.main;
        
        SetupCanvas();
        SetupPosition();
        SetupButton();
        SetupPromptText();
        SetMarkerVisibility(false);
    }

    private void SetupPromptText()
    {
        // Check if markerCanvas exists first
        if (markerCanvas == null)
        {
            Debug.LogError("MarkerCanvas is not assigned! Cannot setup prompt text.");
            return;
        }
        
        // Create or get the prompt text object
        if (promptText == null)
        {
            // Create a simple text object as a child of the marker canvas
            promptGameObject = new GameObject("PromptText");
            promptGameObject.transform.SetParent(markerCanvas.transform, false);
            
            // Add text component
            promptText = promptGameObject.AddComponent<Text>();
            
            // Add rect transform
            promptRectTransform = promptText.GetComponent<RectTransform>();
        }
        else
        {
            promptGameObject = promptText.gameObject;
            promptRectTransform = promptText.GetComponent<RectTransform>();
        }
        
        // Ensure we have a valid promptGameObject and promptText
        if (promptGameObject == null || promptText == null)
        {
            Debug.LogError("Failed to create or find prompt text components!");
            return;
        }
        
        // Setup text component
        promptText.text = promptMessage;
        promptText.fontSize = promptFontSize;
        promptText.alignment = promptAlignment;
        promptText.color = promptColor;
        promptText.fontStyle = promptFontStyle;
        promptText.horizontalOverflow = HorizontalWrapMode.Overflow;
        promptText.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Set custom font if assigned
        if (promptFont != null)
        {
            promptText.font = promptFont;
        }
        
        // Position below the marker
        if (promptRectTransform != null)
        {
            promptRectTransform.anchoredPosition = anchoredPosition;
            promptRectTransform.sizeDelta = promptSize;
        }
        
        // For WebGL, use a simpler approach with a fixed local position
        if (webGLOptimized && promptGameObject != null)
        {
            // Make text more readable in WebGL
            promptText.fontSize = Mathf.Max(promptFontSize, 16); // Ensure minimum size
            
            // Make text more visible by adding a shadow instead of outline or background
            Shadow textShadow = promptText.GetComponent<Shadow>();
            if (textShadow == null && useOutline)
            {
                textShadow = promptText.gameObject.AddComponent<Shadow>();
                textShadow.effectColor = outlineColor;
                textShadow.effectDistance = new Vector2(1, -1);
            }
            
            // Remove any existing background objects
            Transform existingBg = promptGameObject.transform.Find("Background");
            if (existingBg != null)
            {
                Destroy(existingBg.gameObject);
            }
            
            // Remove any existing outline
            Outline outline = promptText.GetComponent<Outline>();
            if (outline != null)
            {
                Destroy(outline);
            }
        }
        else if (useOutline && !webGLOptimized)
        {
            // Setup outline if enabled and not in WebGL optimized mode
            Outline outline = promptText.GetComponent<Outline>();
            if (outline == null)
            {
                outline = promptText.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(outlineThickness, outlineThickness);
            
            // Remove any shadow
            Shadow textShadow = promptText.GetComponent<Shadow>();
            if (textShadow != null)
            {
                Destroy(textShadow);
            }
        }
        else
        {
            // Remove both outline and shadow if not needed
            Outline outline = promptText.GetComponent<Outline>();
            if (outline != null)
            {
                Destroy(outline);
            }
            
            Shadow textShadow = promptText.GetComponent<Shadow>();
            if (textShadow != null)
            {
                Destroy(textShadow);
            }
        }
        
        // Ensure prompt starts hidden
        if (promptGameObject != null)
        {
            promptGameObject.SetActive(false);
        }
    }

    private void SetupButton()
    {
        if (markerButton != null)
        {
            // Setup the button to trigger our custom click event
            onClick.AddListener(OnInteractionTriggered);
            
            // Direct click handler for the button
            markerButton.onClick.AddListener(InvokeClick);
            markerButton.interactable = true;
        }
    }

    // Avoid creating garbage with lambda
    private void InvokeClick()
    {
        onClick.Invoke();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure it has the 'Player' tag.");
        }
    }

    private void SetupCanvas()
    {
        if (markerCanvas != null)
        {
            markerCanvas.renderMode = RenderMode.WorldSpace;
            markerCanvas.transform.localScale = Vector3.one * markerScale;
        }
        else
        {
            Debug.LogError("MarkerCanvas is not assigned! Please assign a Canvas in the inspector.");
        }
    }

    private void SetupPosition()
    {
        basePosition = transform.position + Vector3.up * hoverHeight;
        initialY = basePosition.y;
        transform.position = basePosition;
    }

    private void Update()
    {
        if (playerTransform == null || isTransitioning) return;

        // Only update distance checks at intervals to save performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateInteractionState();
            lastUpdateTime = Time.time;
        }
        
        if (isInRange)
        {
            UpdateVisuals();
        }
        
        if (isInInteractionRange)
        {
            CheckInput();
        }
    }

    private void UpdateInteractionState()
    {
        if (playerTransform == null) return;
        
        playerPositionCache = playerTransform.position;
        
        if (useSquaredDistanceCheck)
        {
            // Use squared distance to avoid expensive sqrt operations
            distanceSqrCache = (transform.position - playerPositionCache).sqrMagnitude;
            isInRange = distanceSqrCache <= visibilityDistanceSqr;
            isInInteractionRange = distanceSqrCache <= interactionDistanceSqr;
        }
        else
        {
            // Fallback to regular distance check
            float distance = Vector3.Distance(transform.position, playerPositionCache);
            isInRange = distance <= visibilityDistance;
            isInInteractionRange = distance <= interactionDistance;
        }

        SetMarkerVisibility(isInRange);
        
        // Update prompt visibility - only when state changes
        if (promptGameObject != null && promptGameObject.activeSelf != isInInteractionRange)
        {
            promptGameObject.SetActive(isInInteractionRange);
        }
        
        // Update button state - only when state changes
        if (markerButton != null && markerButton.enabled != isInInteractionRange)
        {
            markerButton.enabled = isInInteractionRange;
            
            // Update button color based on interaction range
            if (isInInteractionRange)
            {
                var colors = markerButton.colors;
                colors.normalColor = buttonColors.highlightedColor;
                markerButton.colors = colors;
            }
            else
            {
                markerButton.colors = buttonColors;
            }
        }
    }

    private void UpdateVisuals()
    {
        if (markerCanvas == null || !markerCanvas.gameObject.activeSelf) return;

        // Update position with bobbing motion - optimize sin calculation
        basePosition.y = initialY + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = basePosition;

        // Make marker face camera
        if (mainCamera != null)
        {
            // Optimize rotation calculation - only update when needed
            lookPosition = mainCamera.transform.position;
            lookPosition.y = transform.position.y; // Keep y level consistent
            transform.LookAt(lookPosition);
        }
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            // Directly click the button to trigger sound effects
            if (markerButton != null)
            {
                markerButton.onClick.Invoke();
            }
            else
            {
                onClick.Invoke();
            }
        }
    }

    private void OnInteractionTriggered()
    {
        if (!isTransitioning && isInInteractionRange)
        {
            isTransitioning = true;
            StartCoroutine(LoadSceneAsync());
        }
    }

    private System.Collections.IEnumerator LoadSceneAsync()
    {
        // Show loading canvas before scene transition
        LoadingCanvasManager.Instance.ShowLoadingCanvas();

        if (playerTransform != null)
        {
            // Freeze player before making persistent
            var playerController = playerTransform.GetComponent<StudentRecruitment.FinalCharacterController.PlayerController>();
            if (playerController != null)
            {
                playerController.FreezePlayer();
            }

            // Make player persistent before loading scene
            var playerPersistence = playerTransform.GetComponent<StudentRecruitment.PlayerPersistence>();
            if (playerPersistence != null)
            {
                playerPersistence.MakePersistent();
            }
            
            PlayerPositionManager.StorePosition(playerTransform.position);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is ready
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        asyncLoad.allowSceneActivation = true;
        isTransitioning = false;
    }

    public void SetMarkerVisibility(bool visible)
    {
        if (markerCanvas != null && markerCanvas.gameObject.activeSelf != visible)
        {
            markerCanvas.gameObject.SetActive(visible);
            if (!visible)
            {
                if (promptGameObject != null)
                {
                    promptGameObject.SetActive(false);
                }
                if (markerButton != null)
                {
                    markerButton.enabled = false;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw visibility range in editor
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
        Gizmos.DrawWireSphere(transform.position, visibilityDistance);
        
        // Draw interaction range in editor
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Semi-transparent green
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // Draw marker position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * hoverHeight, 0.2f);
    }
#endif

    private void OnDestroy()
    {
        if (markerButton != null)
        {
            markerButton.onClick.RemoveListener(InvokeClick);
        }
        onClick.RemoveAllListeners();
    }
} 