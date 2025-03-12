using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : MonoBehaviour
{
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
            RestorePlayerPosition();
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

        // Check if we have a stored position
        if (PlayerPositionManager.HasStoredPosition())
        {
            Vector3 savedPosition = PlayerPositionManager.GetLastPosition();
            Debug.Log($"[PlayerSpawnHandler] Restoring player to position: {savedPosition}");

            // Force position update through transform
            player.transform.position = savedPosition;

            // If the player has a CharacterController, we need to handle it specially
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // Disable and re-enable the CharacterController to force position update
                characterController.enabled = false;
                player.transform.position = savedPosition;
                characterController.enabled = true;
            }

            // If the player has a Rigidbody, we need to handle it specially
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Reset velocity and move to position
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = savedPosition;
            }

            // Clear the stored position after successful restoration
            PlayerPositionManager.ClearStoredPosition();
        }
        else
        {
            Debug.Log("[PlayerSpawnHandler] No stored position found, player will spawn at default position");
        }
    }
} 