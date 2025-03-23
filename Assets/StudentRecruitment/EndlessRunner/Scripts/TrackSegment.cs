using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class TrackSegment : MonoBehaviour
    {
        [Header("Segment Settings")]
        [SerializeField] private float segmentLength = 20f;
        [Tooltip("Set to true if this is a finish line segment")]
        [SerializeField] private bool isFinishSegment = false;
        
        [Header("Obstacle Spawning")]
        [SerializeField] private Transform[] lanePositions; // Positions to spawn obstacles in each lane
        [SerializeField] private GameObject[] possibleObstacles; // Prefabs of possible obstacles
        [SerializeField] private GameObject[] possiblePowerUps; // Prefabs of possible power-ups
        [SerializeField] private float powerUpProbability = 0.2f; // 20% chance for power-up
        
        [Header("Obstacle Configuration")]
        [Range(0, 3)]
        [SerializeField] private int minObstacles = 1;
        [Range(0, 5)]
        [SerializeField] private int maxObstacles = 3;
        
        private void Start()
        {
            // Skip obstacle generation for finish segments
            if (isFinishSegment) return;
            
            // Only generate obstacles if we have valid lane positions and obstacle prefabs
            if (lanePositions != null && lanePositions.Length > 0 && 
                possibleObstacles != null && possibleObstacles.Length > 0)
            {
                GenerateObstacles();
            }
        }
        
        private void GenerateObstacles()
        {
            // Determine number of obstacles for this segment
            int obstacleCount = Random.Range(minObstacles, maxObstacles + 1);
            
            // Keep track of which lane positions are used (to avoid overlaps)
            bool[] laneUsed = new bool[lanePositions.Length];
            
            for (int i = 0; i < obstacleCount; i++)
            {
                // Find an unused lane position
                int attemptLimit = 10; // Prevent infinite loop
                int laneIndex = -1;
                
                while (attemptLimit > 0)
                {
                    laneIndex = Random.Range(0, lanePositions.Length);
                    if (!laneUsed[laneIndex])
                    {
                        laneUsed[laneIndex] = true;
                        break;
                    }
                    attemptLimit--;
                }
                
                if (laneIndex >= 0)
                {
                    // Decide if we spawn a power-up instead of an obstacle
                    bool spawnPowerUp = Random.value < powerUpProbability && 
                                        possiblePowerUps != null && 
                                        possiblePowerUps.Length > 0;
                    
                    if (spawnPowerUp)
                    {
                        // Select a random power-up
                        GameObject powerUpPrefab = possiblePowerUps[Random.Range(0, possiblePowerUps.Length)];
                        Instantiate(powerUpPrefab, lanePositions[laneIndex].position, Quaternion.identity, transform);
                    }
                    else
                    {
                        // Select a random obstacle
                        GameObject obstaclePrefab = possibleObstacles[Random.Range(0, possibleObstacles.Length)];
                        Instantiate(obstaclePrefab, lanePositions[laneIndex].position, Quaternion.identity, transform);
                    }
                }
            }
        }
        
        public float GetLength()
        {
            return segmentLength;
        }
        
        public bool IsFinishSegment()
        {
            return isFinishSegment;
        }
    }
} 