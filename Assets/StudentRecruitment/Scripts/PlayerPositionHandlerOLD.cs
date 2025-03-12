using UnityEngine;

public class PlayerPositionHandler : MonoBehaviour
{
    private void Start()
    {
        // Check if we have a stored position
        if (PlayerPositionManager.HasStoredPosition())
        {
            // Restore the player's position
            transform.position = PlayerPositionManager.GetLastPosition();
            
            // Clear the stored position after using it
            PlayerPositionManager.ClearStoredPosition();
        }
    }
} 