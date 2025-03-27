using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PathFindingVisualizer : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private GameObject playerObject; // The player object to follow
    [SerializeField] private GameObject targetObject; // The destination object
    [SerializeField] private Material pathMaterial; // Material for the path line
    [SerializeField] private float lineWidth = 0.1f; // Width of the path line
    [SerializeField] private float updateRate = 0.1f; // How often to update the path
    [SerializeField] private float pathHeightOffset = 0.5f; // Height above the ground
    [SerializeField] private float maxPathDistance = 100f; // Maximum distance to generate path
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private float distanceThreshold = 0.5f;

    private LineRenderer pathLine;
    private NavMeshPath currentPath;
    private bool isPathActive = false;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // Validate required components
            if (pathMaterial == null)
            {
                Debug.LogError("PathFindingVisualizer: Path material is missing! Please assign it in the inspector.");
                enabled = false;
                return;
            }

            // Create and setup the LineRenderer
            pathLine = gameObject.AddComponent<LineRenderer>();
            pathLine.material = pathMaterial;
            pathLine.startWidth = lineWidth;
            pathLine.endWidth = lineWidth;
            pathLine.positionCount = 0;
            pathLine.useWorldSpace = true;
            pathLine.enabled = false;
            pathLine.sortingOrder = 1; // Ensure it renders above other objects
            pathLine.sortingLayerName = "Default";
            pathLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            pathLine.receiveShadows = false;

            // Initialize the NavMeshPath
            currentPath = new NavMeshPath();

            // Check if NavMesh exists
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 100f, NavMesh.AllAreas))
            {
                Debug.LogWarning("PathFindingVisualizer: No NavMesh found in the scene! Please bake the NavMesh.");
            }

            isInitialized = true;
            if (showDebugLogs)
            {
                Debug.Log("PathFindingVisualizer: Successfully initialized");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("PathFindingVisualizer: Error during initialization: " + e.Message);
            enabled = false;
        }
    }

    public void StartPathFinding()
    {
        if (!isInitialized)
        {
            Debug.LogError("PathFindingVisualizer: Not properly initialized!");
            return;
        }

        // Validate player and target references
        if (playerObject == null)
        {
            Debug.LogError("PathFindingVisualizer: Player object not assigned! Please assign it in the inspector.");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("PathFindingVisualizer: Target object not assigned! Please assign it in the inspector.");
            return;
        }

        // Check if target is within max distance
        float distanceToTarget = Vector3.Distance(playerObject.transform.position, targetObject.transform.position);
        if (distanceToTarget > maxPathDistance)
        {
            Debug.LogWarning($"PathFindingVisualizer: Target is too far! Distance: {distanceToTarget:F1}m, Max: {maxPathDistance}m");
            return;
        }

        try
        {
            isPathActive = true;
            pathLine.enabled = true;
            CancelInvoke(nameof(UpdatePath));
            InvokeRepeating(nameof(UpdatePath), 0f, updateRate);

            if (showDebugLogs)
            {
                Debug.Log($"PathFindingVisualizer: Started pathfinding from {playerObject.name} to {targetObject.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("PathFindingVisualizer: Error starting pathfinding: " + e.Message);
            StopPathFinding();
        }
    }

    private void UpdatePath()
    {
        if (!isPathActive || !isInitialized) return;

        try
        {
            // Calculate path from player position to target
            NavMesh.CalculatePath(playerObject.transform.position, targetObject.transform.position, NavMesh.AllAreas, currentPath);

            // Check if path is valid
            if (currentPath.status == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning("PathFindingVisualizer: Invalid path! Target might be unreachable.");
                return;
            }

            // Update line renderer positions with height offset
            pathLine.positionCount = currentPath.corners.Length;
            Vector3[] positions = new Vector3[currentPath.corners.Length];
            for (int i = 0; i < currentPath.corners.Length; i++)
            {
                positions[i] = currentPath.corners[i] + Vector3.up * pathHeightOffset;
            }
            pathLine.SetPositions(positions);

            if (showDebugLogs && currentPath.corners.Length > 0)
            {
                Debug.Log($"PathFindingVisualizer: Path updated with {currentPath.corners.Length} points");
            }

            // Check if player is close enough to target
            float distanceToTarget = Vector3.Distance(playerObject.transform.position, targetObject.transform.position);
            if (distanceToTarget <= distanceThreshold)
            {
                if (showDebugLogs)
                {
                    Debug.Log("PathFindingVisualizer: Player reached target, stopping pathfinding");
                }
                StopPathFinding();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("PathFindingVisualizer: Error updating path: " + e.Message);
            StopPathFinding();
        }
    }

    public void StopPathFinding()
    {
        if (!isInitialized) return;

        isPathActive = false;
        pathLine.enabled = false;
        CancelInvoke(nameof(UpdatePath));

        if (showDebugLogs)
        {
            Debug.Log("PathFindingVisualizer: Stopped pathfinding");
        }
    }

    private void OnDestroy()
    {
        // Clean up
        if (pathLine != null)
        {
            Destroy(pathLine);
        }
    }
} 