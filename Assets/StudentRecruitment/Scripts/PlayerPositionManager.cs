using UnityEngine;

public static class PlayerPositionManager
{
    private static Vector3 lastPosition = Vector3.zero;
    private static bool hasStoredPosition = false;
    private static bool isTransitionInProgress = false;
    private static int positionUpdateCount = 0;

    public static void StorePosition(Vector3 position)
    {
        // Validate the position before storing
        if (IsValidPosition(position))
        {
            lastPosition = position;
            hasStoredPosition = true;
            isTransitionInProgress = true;
            positionUpdateCount++;
            Debug.Log($"[PlayerPositionManager] Stored player position ({positionUpdateCount}): {position}");
        }
        else
        {
            Debug.LogWarning($"[PlayerPositionManager] Attempted to store invalid position: {position}. Current stored position: {lastPosition}");
        }
    }

    public static Vector3 GetLastPosition()
    {
        if (!hasStoredPosition)
        {
            Debug.LogWarning("[PlayerPositionManager] No stored position found, returning Vector3.zero");
            return Vector3.zero;
        }
        
        if (!IsValidPosition(lastPosition))
        {
            Debug.LogError($"[PlayerPositionManager] Stored position is invalid: {lastPosition}");
            return Vector3.zero;
        }

        return lastPosition;
    }

    public static bool HasStoredPosition()
    {
        bool isValid = hasStoredPosition && IsValidPosition(lastPosition);
        if (!isValid && hasStoredPosition)
        {
            Debug.LogWarning("[PlayerPositionManager] Has stored position but position is invalid!");
        }
        return isValid;
    }

    public static void ClearStoredPosition()
    {
        Debug.Log($"[PlayerPositionManager] Clearing stored position. Last position was: {lastPosition}");
        lastPosition = Vector3.zero;
        hasStoredPosition = false;
        isTransitionInProgress = false;
        positionUpdateCount = 0;
    }

    public static bool IsTransitionInProgress()
    {
        return isTransitionInProgress;
    }

    private static bool IsValidPosition(Vector3 position)
    {
        bool isValid = !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
                      !float.IsInfinity(position.x) && !float.IsInfinity(position.y) && !float.IsInfinity(position.z) &&
                      position.magnitude < 10000f;

        if (!isValid)
        {
            Debug.LogError($"[PlayerPositionManager] Invalid position detected: {position}");
        }

        return isValid;
    }
    public static void ClearAllData()
    {
        lastPosition = Vector3.zero;
        hasStoredPosition = false;
        isTransitionInProgress = false;
        positionUpdateCount = 0;
    }
}