using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private string targetSceneName = "CanvasTestScene";
    
    private bool isInRange = false;
    private Transform playerTransform;
    private Vector3 interactionPoint;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Subscribe to scene loading event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loading event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindPlayer();
        SetupInteractionPoint();
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

    private void SetupInteractionPoint()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            interactionPoint = renderer.bounds.center;
        }
        else
        {
            interactionPoint = transform.position;
            Debug.LogWarning("No Renderer found on building, using transform position as interaction point.");
        }
    }

    private void Update()
    {
        if (playerTransform == null || isTransitioning || PlayerPositionManager.IsTransitionInProgress())
        {
            return;
        }

        float distance = Vector3.Distance(interactionPoint, playerTransform.position);
        isInRange = distance <= interactionDistance;

        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            AttemptTransition();
        }
    }

    private void AttemptTransition()
    {
        if (playerTransform != null)
        {
            Vector3 currentPosition = playerTransform.position;
            Debug.Log($"Attempting transition. Current position: {currentPosition}");
            
            isTransitioning = true;
            PlayerPositionManager.StorePosition(currentPosition);
            
            // Double-check that position was stored successfully
            if (PlayerPositionManager.HasStoredPosition())
            {
                StartCoroutine(LoadSceneAsync());
            }
            else
            {
                Debug.LogError("Failed to store position! Aborting transition.");
                isTransitioning = false;
            }
        }
    }

    private System.Collections.IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait a frame to ensure position is properly stored
        yield return new WaitForEndOfFrame();

        // Double check position is stored before allowing scene transition
        if (PlayerPositionManager.HasStoredPosition())
        {
            Debug.Log("Position verified, proceeding with scene transition");
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            Debug.LogError("Position verification failed before scene transition!");
            isTransitioning = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset transition state when any scene is loaded
        isTransitioning = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Renderer renderer = GetComponent<Renderer>();
            Vector3 gizmoPosition = renderer != null ? renderer.bounds.center : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gizmoPosition, 0.5f);
        }

        Gizmos.color = isInRange ? Color.green : Color.red;
        Vector3 gizmoCenter = Application.isPlaying ? interactionPoint : (GetComponent<Renderer>()?.bounds.center ?? transform.position);
        Gizmos.DrawWireSphere(gizmoCenter, interactionDistance);
    }

    private void HandleError(string message)
    {
        Debug.LogError($"[BuildingInteraction] {message}");
        #if UNITY_WEBGL && !UNITY_EDITOR
            // Handle errors gracefully in WebGL
            // Maybe show a UI message instead of just logging
        #endif
    }
}