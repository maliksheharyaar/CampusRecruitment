using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Collections;

public class PlayerSpawnHandler : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private float maxGroundAngle = 45f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float spawnElevation = 0.1f; // Small elevation when spawning
    [SerializeField] private float teleportDelay = 0.01f; // Delay for teleportation
    [SerializeField] private float loadingScreenDuration = 0.1f; // How long to show the loading screen

    [Header("UI References")]
    [SerializeField] private GameObject loadingCanvas;

    private bool isRestoringPosition = false;

    private void Awake()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only handle position in MainScene
        if (scene.name == "MainScene")
        {
            // Start position restoration after a short delay to ensure player is spawned
            StartCoroutine(RestorePlayerPositionWithDelay());
        }
    }

    private System.Collections.IEnumerator RestorePlayerPositionWithDelay()
    {
        // Show loading canvas if it exists
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
        }

        // Wait for a short time to ensure player is spawned at origin
        yield return new WaitForSeconds(teleportDelay);

        // Prevent multiple simultaneous restorations
        if (isRestoringPosition)
        {
            Debug.LogWarning("[PlayerSpawnHandler] Position restoration already in progress, skipping");
            yield break;
        }

        isRestoringPosition = true;
        RestorePlayerPosition();
        isRestoringPosition = false;

        // Keep the loading screen visible for the specified duration
        yield return new WaitForSeconds(loadingScreenDuration);

        // Hide loading canvas after the duration
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
        }
    }

    private void RestorePlayerPosition()
    {
        // Find the player in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[PlayerSpawnHandler] Player not found in scene!");
            return;
        }

        // Get the player controller to unfreeze later
        var playerController = player.GetComponent<StudentRecruitment.FinalCharacterController.PlayerController>();

        // Check if we have a stored position
        if (PlayerPositionManager.HasStoredPosition())
        {
            Vector3 savedPosition = PlayerPositionManager.GetLastPosition();
            Debug.Log($"[PlayerSpawnHandler] Restoring player to position: {savedPosition}");

            // Get required components
            CharacterController characterController = player.GetComponent<CharacterController>();
            NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
            Rigidbody rb = player.GetComponent<Rigidbody>();

            // Temporarily disable components
            if (characterController != null) characterController.enabled = false;
            if (agent != null) agent.enabled = false;
            if (rb != null) rb.isKinematic = true;

            // Find valid ground position and add elevation
            Vector3 groundPosition = FindValidGroundPosition(savedPosition);
            if (groundPosition != Vector3.zero)
            {
                // Add elevation to the ground position
                groundPosition.y += spawnElevation;

                // Set position
                player.transform.position = groundPosition;

                // Re-enable components
                if (characterController != null)
                {
                    characterController.enabled = true;
                    // Force character controller to update its position
                    characterController.Move(Vector3.zero);
                }

                if (agent != null)
                {
                    agent.enabled = true;
                    // Force NavMeshAgent to update its position
                    agent.Warp(groundPosition);
                }

                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"[PlayerSpawnHandler] Successfully restored player to elevated position: {groundPosition}");
            }
            else
            {
                // If no valid ground position found, add elevation to saved position
                savedPosition.y += spawnElevation;
                Debug.LogWarning("[PlayerSpawnHandler] Could not find valid ground position, using elevated original position");
                player.transform.position = savedPosition;
            }

            // Clear the stored position after successful restoration
            PlayerPositionManager.ClearStoredPosition();
        }
        else
        {
            Debug.Log("[PlayerSpawnHandler] No stored position found, player will spawn at default position");
        }

        // Unfreeze the player after position restoration
        if (playerController != null)
        {
            playerController.UnfreezePlayer();
        }
    }

    private Vector3 FindValidGroundPosition(Vector3 position)
    {
        // First try to find a valid NavMesh position
        if (NavMesh.SamplePosition(position, out NavMeshHit navHit, groundCheckDistance, NavMesh.AllAreas))
        {
            // Check if the ground is too steep
            if (Physics.Raycast(navHit.position + Vector3.up, Vector3.down, out RaycastHit groundHit, groundCheckDistance, groundLayer))
            {
                float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
                if (groundAngle <= maxGroundAngle)
                {
                    return navHit.position;
                }
            }
        }

        // If NavMesh position is invalid, try to find a valid ground position using raycast
        if (Physics.Raycast(position + Vector3.up * groundCheckDistance, Vector3.down, out RaycastHit hit, groundCheckDistance * 2, groundLayer))
        {
            float groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (groundAngle <= maxGroundAngle)
            {
                // Check if this position is on the NavMesh
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit sampleHit, 1f, NavMesh.AllAreas))
                {
                    return sampleHit.position;
                }
            }
        }

        return Vector3.zero; // Return zero if no valid position found
    }
} 