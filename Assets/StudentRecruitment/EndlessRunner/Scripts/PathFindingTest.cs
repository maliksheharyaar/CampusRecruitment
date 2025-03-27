using UnityEngine;

public class PathFindingTest : MonoBehaviour
{
    [Header("Path Finding Settings")]
    [SerializeField] private PathFindingVisualizer pathVisualizer;
    [SerializeField] private KeyCode startPathKey = KeyCode.P;
    [SerializeField] private KeyCode stopPathKey = KeyCode.O;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Validate references
        if (pathVisualizer == null)
        {
            Debug.LogError("PathFindingTest: PathVisualizer reference is missing! Please assign it in the inspector.");
            enabled = false;
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"PathFindingTest: Controls:\n" +
                     $"- Press {startPathKey} to start pathfinding\n" +
                     $"- Press {stopPathKey} to stop pathfinding");
        }
    }

    private void Update()
    {
        // Start pathfinding
        if (Input.GetKeyDown(startPathKey))
        {
            try
            {
                pathVisualizer.StartPathFinding();
                if (showDebugLogs)
                {
                    Debug.Log("PathFindingTest: Started pathfinding");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("PathFindingTest: Error starting pathfinding: " + e.Message);
            }
        }

        // Stop pathfinding
        if (Input.GetKeyDown(stopPathKey))
        {
            try
            {
                pathVisualizer.StopPathFinding();
                if (showDebugLogs)
                {
                    Debug.Log("PathFindingTest: Stopped pathfinding");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("PathFindingTest: Error stopping pathfinding: " + e.Message);
            }
        }
    }

    // Public method to start pathfinding from other scripts
    public void StartPathFinding()
    {
        if (pathVisualizer != null)
        {
            pathVisualizer.StartPathFinding();
        }
    }

    // Public method to stop pathfinding from other scripts
    public void StopPathFinding()
    {
        if (pathVisualizer != null)
        {
            pathVisualizer.StopPathFinding();
        }
    }
} 