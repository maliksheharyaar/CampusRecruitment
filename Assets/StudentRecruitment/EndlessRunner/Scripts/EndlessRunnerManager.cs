using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StudentRecruitment.EndlessRunner;

namespace StudentRecruitment.EndlessRunner
{
    public class EndlessRunnerManager : MonoBehaviour
    {
        [Header("Track Generation")]
        [SerializeField] private GameObject[] trackSegmentPrefabs;
        [SerializeField] private GameObject[] obstaclePrefabs;
        [SerializeField] private GameObject finishLinePrefab;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject[] powerUpPrefabs; // Power-up prefabs array
        [SerializeField] private int trackSegmentsToSpawn = 10;
        [SerializeField] private float segmentLength = 20f;
        [SerializeField] private Transform trackParent;
        [SerializeField] private float laneDistance = 3f; // Should match RunnerController

        [Header("Turn Settings")]
        [SerializeField] private bool enableTurns = true; // Allow turning this feature on/off easily
        [SerializeField] private float turnChance = 0.3f; // 30% chance for a turn after min segments
        [SerializeField] private int minSegmentsBeforeTurn = 10; // Minimum straight segments before a turn
        [SerializeField] private GameObject leftTurnPrefab; // Left turn track segment
        [SerializeField] private GameObject rightTurnPrefab; // Right turn track segment

        [Header("Player Settings")]
        [SerializeField] private float initialSpeed = 10f;
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float acceleration = 0.1f;
        [SerializeField] private float speedBoostMultiplier = 1.5f;
        [SerializeField] private float speedBoostDuration = 5f;

        [Header("Spawn Settings")]
        [SerializeField] private float obstacleSpawnChance = 0.7f;
        [SerializeField] private float coinSpawnChance = 0.8f;
        [SerializeField] private float powerUpSpawnChance = 0.3f; // Chance to spawn power-ups on a segment
        [SerializeField] private int maxCoinsPerSegment = 3;
        [SerializeField] private int maxPowerUpsPerSegment = 1; // Maximum power-ups per segment

        [Header("References")]
        [SerializeField] private RunnerController playerController;
        [SerializeField] private GameObject boulder;

        [Header("Game Settings")]
        [SerializeField] private float powerUpDuration = 5f;
        [SerializeField] private int coinsPerCompletion = 10;
        [SerializeField] private int baseCoinsPerScore = 5; // Base number of coins awarded per score point
        [SerializeField] private float scoreMultiplier = 0.5f; // Multiplier for score to coins conversion
        [SerializeField] private string mainSceneName = "MainScene";

        [Header("Object Culling")]
        [SerializeField] private float visibilityDistance = 50f; // How far ahead objects should be visible
        [SerializeField] private float cullingDistance = 20f; // How far behind objects should be destroyed

        // Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<int> OnScoreChanged;

        // Properties
        public GameState CurrentGameState { get; private set; } = GameState.Running;
        public int CurrentScore { get; private set; } = 0;
        public bool HasCollectedPage { get; private set; } = false;
        public float PowerUpDuration => powerUpDuration;
        public int CoinsPerCompletion => coinsPerCompletion;

        // Game state
        private bool gameInProgress = false;
        private List<GameObject> spawnedSegments = new List<GameObject>();
        private float totalTrackLength;
        private float currentSpeed;
        private bool isSpeedBoosted = false;
        private Coroutine speedBoostCoroutine;

        // Track generation state
        private int segmentsSinceLastTurn = 0; // Counter for segments since last turn
        private Vector3 currentTrackDirection = Vector3.forward; // Starts going forward (Z-axis)
        private Vector3 currentTrackPosition = Vector3.zero; // Current position for track placement
        private float currentSegmentLength = 0f; // Track current segment length

        // Track the last turn direction to prevent consecutive turns in the same direction
        private bool? lastTurnDirection = null; // null = no turn yet, true = left, false = right

        // Public static reference for access
        public static EndlessRunnerManager Instance { get; private set; }

        private Vector3 gameStartPosition;
        private Transform playerTransform;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Create track parent if not assigned
            if (trackParent == null)
            {
                trackParent = new GameObject("TrackParent").transform;
            }

            // Initialize speed
            currentSpeed = initialSpeed;
        }

        private void Start()
        {
            // Find player controller if not assigned
            if (playerController == null)
            {
                playerController = FindObjectOfType<RunnerController>();
            }
            
            // Generate the track
            GenerateTrack();

            // Start the game (this initializes speed)
            StartGame();

            // Update UI
            UpdateUI(GameState.Running);
            
            // Double-ensure the player is moving
            StartCoroutine(EnsurePlayerMoving());

            // Store the start position for culling reference
            gameStartPosition = Vector3.zero;
            
            // Find the player transform for distance calculations
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
            else
            {
                // Try to find the player if not directly referenced
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            
            // Start the culling coroutine
            StartCoroutine(CullDistantObjects());
        }
        
        private IEnumerator EnsurePlayerMoving()
        {
            // Wait a frame to let everything initialize
            yield return null;
            
            // Make sure player has speed set
            if (playerController != null)
            {
                playerController.SetForwardSpeed(initialSpeed);
            }
            else
            {
                Debug.LogError("Still no player controller after initialization!");
            }
        }

        void Update()
        {
            if (!gameInProgress) return;

            // Check if player controller exists
            if (playerController == null)
            {
                Debug.LogError("Player controller not assigned in the inspector!");
                return;
            }

            // Accelerate speed over time
            if (!isSpeedBoosted && !playerController.isFinished)
            {
                currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
                // Set forward speed on player controller
                playerController.SetForwardSpeed(currentSpeed);
            }
        }

        private void GenerateTrack()
        {
            if (trackSegmentPrefabs.Length == 0)
            {
                Debug.LogError("No track segment prefabs assigned!");
                return;
            }

            // Reset track generation variables
            currentTrackPosition = Vector3.zero;
            currentTrackDirection = Vector3.forward;
            segmentsSinceLastTurn = 0;
            lastTurnDirection = null; // Reset the last turn direction

            // Spawn regular track segments
            for (int i = 0; i < trackSegmentsToSpawn; i++)
            {
                // Determine if this segment should be a turn - never allow the last segment to be a turn
                bool shouldGenerateTurn = enableTurns && 
                                          segmentsSinceLastTurn >= minSegmentsBeforeTurn && 
                                          UnityEngine.Random.Range(0f, 1f) < turnChance &&
                                          i < trackSegmentsToSpawn - 1; // Ensure last segment is not a turn
                
                GameObject segment;
                
                if (shouldGenerateTurn)
                {
                    // Generate a turn segment (left or right)
                    segment = GenerateTurnSegment();
                    segmentsSinceLastTurn = 0; // Reset counter after turn
                }
                else
                {
                    // Generate a straight segment
                    segment = GenerateStraightSegment(i);
                    segmentsSinceLastTurn++; // Increment counter for straight segments
                }
                
                spawnedSegments.Add(segment);
            }

            // Spawn finish line at the end
            SpawnFinishLine();

            // Calculate total track length (approximate)
            totalTrackLength = trackSegmentsToSpawn * segmentLength;
        }

        private GameObject GenerateStraightSegment(int segmentIndex)
        {
            // Select a random track segment prefab
            GameObject segmentPrefab = trackSegmentPrefabs[UnityEngine.Random.Range(0, trackSegmentPrefabs.Length)];
            currentSegmentLength = segmentLength; // Store for later use
            
            // Calculate rotation based on current track direction
            Quaternion segmentRotation = Quaternion.LookRotation(currentTrackDirection);
            
            // Instantiate the segment at the current position with proper rotation
            GameObject segment = Instantiate(segmentPrefab, currentTrackPosition, segmentRotation, trackParent);
            
            // Ensure track segment has the "Track" tag
            if (!segment.CompareTag("Track"))
            {
                segment.tag = "Track";
            }
            
            // Calculate center of the segment for obstacle and item placement
            Vector3 segmentCenter = currentTrackPosition + (currentTrackDirection * segmentLength * 0.5f);
            
            // Randomly add obstacles (except the last segment)
            if (segmentIndex < trackSegmentsToSpawn - 1 && UnityEngine.Random.Range(0f, 1f) < obstacleSpawnChance)
            {
                // Random position along the segment
                float obstacleDistanceFromCenter = UnityEngine.Random.Range(-segmentLength * 0.3f, segmentLength * 0.3f);
                Vector3 obstaclePosition = segmentCenter + (currentTrackDirection * obstacleDistanceFromCenter);
                SpawnObstaclesAlongDirection(obstaclePosition);
            }
            
            // Spawn coins along the segment
            if (coinPrefab != null && UnityEngine.Random.Range(0f, 1f) < coinSpawnChance)
            {
                SpawnCoinsAlongDirection(segmentCenter, segmentLength, currentTrackDirection);
            }
            
            // Spawn power-ups along the segment
            if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && UnityEngine.Random.Range(0f, 1f) < powerUpSpawnChance)
            {
                SpawnPowerUpsAlongDirection(segmentCenter, segmentLength, currentTrackDirection);
            }
            
            // Move to next position for the next segment
            currentTrackPosition += currentTrackDirection * segmentLength;
            
            return segment;
        }

        private GameObject GenerateTurnSegment()
        {
            // Determine turn direction (left or right)
            // If we had a previous turn, alternate the direction
            bool isLeftTurn;
            
            if (lastTurnDirection.HasValue)
            {
                // Force alternating turns - if last turn was left, this one must be right and vice versa
                isLeftTurn = !lastTurnDirection.Value;
                Debug.Log($"Forcing alternating turn: Previous turn was {(lastTurnDirection.Value ? "left" : "right")}, this turn is {(isLeftTurn ? "left" : "right")}");
            }
            else
            {
                // First turn, randomly choose direction
                isLeftTurn = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                Debug.Log($"First turn, randomly selected {(isLeftTurn ? "left" : "right")}");
            }
            
            // Store this turn direction for next time
            lastTurnDirection = isLeftTurn;
            
            GameObject turnPrefab = isLeftTurn ? leftTurnPrefab : rightTurnPrefab;
            
            // Use regular track prefab if specific turn prefabs aren't assigned
            if (turnPrefab == null)
            {
                turnPrefab = trackSegmentPrefabs[0];
                Debug.LogWarning("Turn prefab not assigned. Using regular track segment.");
            }
            
            // Calculate rotation for the turn
            float turnAngle = isLeftTurn ? -90f : 90f;
            
            // Current rotation before turn
            Quaternion currentRotation = Quaternion.LookRotation(currentTrackDirection);
            
            // Instantiate the turn segment
            GameObject turnSegment = Instantiate(turnPrefab, currentTrackPosition, currentRotation, trackParent);
            
            // Ensure track segment has the "Track" tag
            if (!turnSegment.CompareTag("Track"))
            {
                turnSegment.tag = "Track";
            }
            
            // ===== Create a larger centering trigger before the turn =====
            GameObject centeringTrigger = new GameObject("LaneCenteringTrigger");
            centeringTrigger.transform.SetParent(turnSegment.transform);
            // Position it before the turn trigger (farther back from the start of the segment)
            centeringTrigger.transform.localPosition = new Vector3(0, 0, -10f);
            centeringTrigger.transform.localRotation = Quaternion.identity;
            centeringTrigger.tag = "CenterLane"; // Special tag for lane centering
            
            // Add a box collider as trigger
            BoxCollider centeringCollider = centeringTrigger.AddComponent<BoxCollider>();
            centeringCollider.isTrigger = true;
            centeringCollider.size = new Vector3(laneDistance * 6, 5f, 8f); // Much wider and longer than turn trigger
            centeringCollider.center = new Vector3(0, 1.5f, 0); // Centered at player height
            
            // Add LaneCenteringTrigger component
            LaneCenteringTrigger centeringComponent = centeringTrigger.AddComponent<LaneCenteringTrigger>();
            
            // ===== Create the regular turn trigger =====
            GameObject triggerZone = new GameObject("TurnTriggerZone");
            triggerZone.transform.SetParent(turnSegment.transform);
            triggerZone.transform.localPosition = Vector3.zero; // Start of the segment
            triggerZone.transform.localRotation = Quaternion.identity;
            triggerZone.tag = "Track"; // Important: must have the same tag
            
            // Add a box collider as trigger
            BoxCollider triggerCollider = triggerZone.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(laneDistance * 3, 3f, 1f); // Wide enough for all lanes, tall enough for jumping player
            triggerCollider.center = new Vector3(0, 1.5f, 0); // Centered at player height
            
            // Add TurnTrigger component to the trigger zone
            TurnTrigger turnTrigger = triggerZone.AddComponent<TurnTrigger>();
            
            // SIMPLIFY: We only need to set the isLeftTurn field, no need for complex exit direction calculation
            typeof(TurnTrigger).GetField("isLeftTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(turnTrigger, isLeftTurn);
            typeof(TurnTrigger).GetField("turnAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(turnTrigger, 90f); // Always use 90 degrees
            
            // Store a reference to mark the turn trigger for debugging
            turnTrigger.gameObject.name = isLeftTurn ? "LeftTurnTrigger" : "RightTurnTrigger";
            
            // Calculate new direction after the turn - this is still needed for track generation
            currentTrackDirection = Quaternion.Euler(0, isLeftTurn ? -90 : 90, 0) * currentTrackDirection;
            
            // Update position for the next segment
            currentTrackPosition += currentTrackDirection * segmentLength;
            
            Debug.Log($"Generated {(isLeftTurn ? "left" : "right")} turn at {turnSegment.transform.position}. Turn angle: {(isLeftTurn ? -90 : 90)}, New direction: {currentTrackDirection}");
            
            return turnSegment;
        }

        private void SpawnFinishLine()
        {
            if (finishLinePrefab != null && spawnedSegments.Count >= 1)
            {
                // Get the last segment's position
                GameObject lastSegment = spawnedSegments[spawnedSegments.Count - 1];
                Vector3 finishPosition = lastSegment.transform.position;
                Debug.Log($"Last segment position: {finishPosition}");
                
                Quaternion finishRotation = Quaternion.LookRotation(currentTrackDirection, Vector3.up);
                
                GameObject finishLine = Instantiate(finishLinePrefab, finishPosition, finishRotation, trackParent);
                spawnedSegments.Add(finishLine);
                
                Debug.Log($"Finish line spawned at position: {finishLine.transform.position}");
                
                // Make sure the finish line has a trigger collider and the "Finish" tag
                if (!finishLine.CompareTag("Finish"))
                {
                    finishLine.tag = "Finish";
                }
                
                Collider finishCollider = finishLine.GetComponent<Collider>();
                if (finishCollider != null)
                {
                    finishCollider.isTrigger = true;
                }
            }
        }

        // Modified to spawn obstacles along the current track direction
        private void SpawnObstaclesAlongDirection(Vector3 position)
        {
            // Skip if no obstacle prefabs are assigned
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            {
                Debug.LogWarning("No obstacle prefabs assigned");
                return;
            }

            // Choose a random lane (0, 1, 2)
            int lane = UnityEngine.Random.Range(0, 3);
            
            // Calculate position based on lane - perpendicular to track direction
            Vector3 rightVector = Vector3.Cross(Vector3.up, currentTrackDirection).normalized;
            float xOffset = (lane - 1) * laneDistance; // Convert lane to offset
            Vector3 obstaclePosition = position + (rightVector * xOffset);
            
            // Choose a random obstacle prefab
            GameObject obstaclePrefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
            
            // Rotation aligned with track
            Quaternion trackRotation = Quaternion.LookRotation(currentTrackDirection, Vector3.up);
            
            // Instantiate with proper rotation
            GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, trackRotation);
            
            // Add to spawned segments list to track
            spawnedSegments.Add(obstacle);
        }

        // New method to spawn coins along a direction
        private void SpawnCoinsAlongDirection(Vector3 centerPosition, float segmentLength, Vector3 direction)
        {
            // Skip if coin prefab is null
            if (coinPrefab == null) return;
            
            // Calculate right vector perpendicular to the track direction
            Vector3 rightVector = Vector3.Cross(Vector3.up, direction).normalized;
            
            // Calculate how many coins to spawn
            int coinsToSpawn = UnityEngine.Random.Range(1, maxCoinsPerSegment + 1);
            
            for (int i = 0; i < coinsToSpawn; i++)
            {
                // Choose a random lane
                int lane = UnityEngine.Random.Range(0, 3);
                
                // Random position along segment
                float zOffset = UnityEngine.Random.Range(-segmentLength * 0.4f, segmentLength * 0.4f);
                
                // Calculate position based on lane - perpendicular to track direction
                float xOffset = (lane - 1) * laneDistance; // Convert lane to offset
                Vector3 coinPosition = centerPosition + (direction * zOffset) + (rightVector * xOffset);
                coinPosition.y += 1f; // Raise coins slightly above the ground
                
                // Rotation aligned with track
                Quaternion coinRotation = Quaternion.LookRotation(direction, Vector3.up);
                
                // Check if position is clear
                if (IsPositionClear(coinPosition, 1.0f))
                {
                    GameObject coin = Instantiate(coinPrefab, coinPosition, coinRotation);
                    spawnedSegments.Add(coin);
                }
            }
        }

        // New method to spawn power-ups along a direction
        private void SpawnPowerUpsAlongDirection(Vector3 centerPosition, float segmentLength, Vector3 direction)
        {
            // Skip if no power-up prefabs
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            
            // Calculate right vector perpendicular to the track direction
            Vector3 rightVector = Vector3.Cross(Vector3.up, direction).normalized;
            
            // Calculate how many power-ups to spawn
            int powerUpsToSpawn = Mathf.Min(UnityEngine.Random.Range(0, maxPowerUpsPerSegment + 1), powerUpPrefabs.Length);
            
            for (int i = 0; i < powerUpsToSpawn; i++)
            {
                // Choose a random lane
                int lane = UnityEngine.Random.Range(0, 3);
                
                // Random position along segment
                float zOffset = UnityEngine.Random.Range(-segmentLength * 0.4f, segmentLength * 0.4f);
                
                // Calculate position based on lane - perpendicular to track direction
                float xOffset = (lane - 1) * laneDistance; // Convert lane to offset
                Vector3 powerUpPosition = centerPosition + (direction * zOffset) + (rightVector * xOffset);
                powerUpPosition.y += 1f; // Raise power-ups slightly above the ground
                
                // Rotation aligned with track
                Quaternion powerUpRotation = Quaternion.LookRotation(direction, Vector3.up);
                
                // Check if position is clear
                if (IsPositionClear(powerUpPosition, 1.5f))
                {
                    int powerUpIndex = UnityEngine.Random.Range(0, powerUpPrefabs.Length);
                    GameObject powerUp = Instantiate(powerUpPrefabs[powerUpIndex], powerUpPosition, powerUpRotation);
                    spawnedSegments.Add(powerUp);
                    
                    // Start animation coroutine for the power-up
                    StartCoroutine(AnimatePowerUp(powerUp.transform));
                }
            }
        }

        public void StartGame()
        {
            Debug.Log("StartGame called");
            
            // Check if player controller exists
            if (playerController == null)
            {
                Debug.LogError("Player controller not assigned in the inspector!");
                playerController = FindObjectOfType<RunnerController>();
                
                if (playerController == null)
                {
                    Debug.LogError("Could not find RunnerController in scene!");
                    return;
                }
                else
                {
                    Debug.Log("Found player controller in scene");
                }
            }

            // Reset player and score
            playerController.ResetPlayer();
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);

            // Reset speed and ensure player is moving
            currentSpeed = initialSpeed;
            playerController.SetForwardSpeed(currentSpeed);
            Debug.Log("Start game setting player speed to: " + currentSpeed);
            
            // Manually trigger player movement
            if (playerController != null)
            {
                playerController.Jump(); // Just to test input system
            }

            // Start boulder chase if boulder exists
            if (boulder != null)
            {
                // Position boulder behind first track segment
                boulder.transform.position = new Vector3(0, boulder.transform.position.y, -10f);
                
                // Activate boulder
                boulder.SetActive(true);
                
                // Start chasing behavior if it has BoulderController
                BoulderController boulderController = boulder.GetComponent<BoulderController>();
                if (boulderController != null)
                {
                    boulderController.StartChasing();
                }
            }

            gameInProgress = true;
        }

        public void OnPlayerDeath()
        {
            gameInProgress = false;

            // Stop player movement completely using the Die method
            if (playerController != null)
            {
                playerController.Die();
            }

            // Stop boulder
            if (boulder != null)
            {
                BoulderController boulderController = boulder.GetComponent<BoulderController>();
                if (boulderController != null)
                {
                    boulderController.StopChasing();
                }
            }
            
            // Stop all game coroutines to ensure no lingering effects
            StopAllCoroutines();

            // Update UI to game over state
            UpdateUI(GameState.GameOver);
        }

        public void OnPlayerReachFinish()
        {
            // Stop the game progress
            gameInProgress = false;
            
            // Stop the player
            if (playerController != null)
            {
                playerController.isFinished = true;
                playerController.SetForwardSpeed(0);
                
                // Make sure the player freezes in place
                Rigidbody playerRigidbody = playerController.GetComponent<Rigidbody>();
                if (playerRigidbody != null)
                {
                    playerRigidbody.velocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                    playerRigidbody.isKinematic = true;
                }
            }
            
            // Set the game state to win
            UpdateGameState(GameState.Win);
            
            // Call OnPlayerWin
            OnPlayerWin();
        }

        public void OnPlayerWin()
        {
            // Award coins to the player based on score
            int coinsAwarded = CalculateCoinsReward();
        }

        private void UpdateUI(GameState state)
        {
            CurrentGameState = state;
            
            // Trigger the game state changed event
            OnGameStateChanged?.Invoke(state);
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateGameState(state);
            }
        }

        // Public method to update game state
        public void UpdateGameState(GameState newState)
        {
            UpdateUI(newState);
        }

        // Add coins to the score
        public void AddScore(int points)
        {
            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);
        }
        
        // Calculate coins based on score and level progression
        public int CalculateCoinsReward()
        {
            // Base coins for completing the level
            int baseCoins = coinsPerCompletion;
            
            // Additional coins based on score and the base coins per score rate
            // Use baseCoinsPerScore to give a minimum number of coins per score point
            int scoreCoins = Mathf.Max(
                baseCoinsPerScore, 
                Mathf.RoundToInt(CurrentScore * scoreMultiplier)
            );
            
            // Bonus coins for high scores
            int bonusCoins = 0;
            
            // Add bonus tiers based on score thresholds
            if (CurrentScore >= 50) bonusCoins += 5;
            if (CurrentScore >= 100) bonusCoins += 10;
            if (CurrentScore >= 200) bonusCoins += 20;
            
            // Sum all coin rewards
            int totalCoins = baseCoins + scoreCoins + bonusCoins;
            
            Debug.Log($"Coin reward breakdown: Base={baseCoins}, Score-based={scoreCoins}, Bonus={bonusCoins}, Total={totalCoins}");
            
            return totalCoins;
        }
        
        // Activate speed boost
        public void ActivateSpeedBoost()
        {
            // Cancel existing speed boost if active
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine);
            }
            
            // Start new speed boost
            speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine());
        }
        
        private IEnumerator SpeedBoostCoroutine()
        {
            isSpeedBoosted = true;
            
            // Apply speed boost
            float originalSpeed = currentSpeed;
            float boostedSpeed = originalSpeed * speedBoostMultiplier;
            
            // Cap at max speed
            boostedSpeed = Mathf.Min(boostedSpeed, maxSpeed * 1.5f);
            
            // Apply to player
            playerController.SetForwardSpeed(boostedSpeed);
            
            // Wait for duration
            yield return new WaitForSeconds(speedBoostDuration);
            
            // Return to normal speed (but don't go backward in progression)
            float currentNormalSpeed = Mathf.Min(currentSpeed + acceleration * speedBoostDuration, maxSpeed);
            float endSpeed = Mathf.Max(currentNormalSpeed, originalSpeed);
            playerController.SetForwardSpeed(endSpeed);
            
            isSpeedBoosted = false;
            speedBoostCoroutine = null;
        }
        
        // For loading the next level
        public void LoadNextLevel()
        {
            // For now, just restart the current level
            RestartGame();
        }
        
        // Renamed from ReturnToMainScene to match expected method name
        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainSceneName);
        }
        
        // Keep for backward compatibility
        public void ReturnToMainScene()
        {
            ReturnToMainMenu();
        }

        public void RestartGame()
        {
            // Simply reload the current scene
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        
        // Modified to keep track segments but clear gameplay objects
        private void ClearTrack()
        {
            // Destroy all spawned segments and objects
            foreach (GameObject segment in spawnedSegments)
            {
                if (segment != null)
                {
                    Destroy(segment);
                }
            }
            
            // Clear the list
            spawnedSegments.Clear();
            
            // Additional cleanup: find any remaining objects that might have been missed
            ClearRemainingGameplayObjects();
        }
        
        // New method to clear only gameplay objects but keep track segments
        private void ClearGameplayObjects()
        {
            // Create a temporary list of objects to remove from spawnedSegments
            List<GameObject> objectsToRemove = new List<GameObject>();
            
            // Go through all spawned objects
            foreach (GameObject obj in spawnedSegments)
            {
                if (obj == null) continue;
                
                // Skip track segments and finish line - keep those
                if (obj.CompareTag("Track") || obj.CompareTag("Finish"))
                {
                    continue;
                }
                
                // If it's an obstacle, coin, or power-up, destroy it
                if (obj.CompareTag("Obstacle") || obj.CompareTag("Coin") || obj.CompareTag("PowerUp"))
                {
                    Destroy(obj);
                    objectsToRemove.Add(obj);
                }
            }
            
            // Remove the destroyed objects from our list
            foreach (GameObject obj in objectsToRemove)
            {
                spawnedSegments.Remove(obj);
            }
            
            // Additional cleanup for any objects we might have missed
            ClearRemainingGameplayObjects();
        }
        
        // Helper method to clear any remaining gameplay objects
        private void ClearRemainingGameplayObjects()
        {
            // Find and destroy all obstacles, coins, and power-ups
            GameObject[] leftoverObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (GameObject obstacle in leftoverObstacles)
            {
                Destroy(obstacle);
            }
            
            GameObject[] leftoverCoins = GameObject.FindGameObjectsWithTag("Coin");
            foreach (GameObject coin in leftoverCoins)
            {
                Destroy(coin);
            }
            
            GameObject[] leftoverPowerUps = GameObject.FindGameObjectsWithTag("PowerUp");
            foreach (GameObject powerUp in leftoverPowerUps)
            {
                Destroy(powerUp);
            }
        }
        
        // New method to regenerate gameplay objects on existing track
        private void RegenerateGameplayObjects()
        {
            // Reset track generation state
            segmentsSinceLastTurn = 0;
            currentTrackDirection = Vector3.forward;
            
            Debug.Log($"Regenerating gameplay objects on {spawnedSegments.Count} existing track segments");

            // Generate gameplay objects on existing track segments
            for (int i = 0; i < spawnedSegments.Count; i++)
            {
                Transform segment = spawnedSegments[i].transform;
                
                // Skip if not a track segment (might be a power-up or obstacle)
                if (!spawnedSegments[i].CompareTag("Track")) continue;
                
                // Get segment position and direction
                Vector3 segmentPosition = segment.position;
                currentTrackDirection = segment.forward;
                
                // Random chance for obstacles
                if (UnityEngine.Random.Range(0f, 1f) < obstacleSpawnChance)
                {
                    // Position obstacles randomly along the segment
                    Vector3 obstaclePosition = segmentPosition + currentTrackDirection * 
                        (segmentLength * UnityEngine.Random.Range(0.4f, 0.9f));
                    SpawnObstaclesAlongDirection(obstaclePosition);
                }
                
                // Random chance for coins
                if (coinPrefab != null && UnityEngine.Random.Range(0f, 1f) < coinSpawnChance)
                {
                    SpawnCoinsAlongDirection(segmentPosition + currentTrackDirection * (segmentLength * 0.5f), 
                                          segmentLength, currentTrackDirection);
                }
                
                // Random chance for power-ups
                if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && 
                    UnityEngine.Random.Range(0f, 1f) < powerUpSpawnChance)
                {
                    SpawnPowerUpsAlongDirection(segmentPosition + currentTrackDirection * (segmentLength * 0.5f), 
                                         segmentLength, currentTrackDirection);
                }
            }
            
            Debug.Log("Gameplay objects regenerated successfully");
        }

        // Method to spawn power-ups with collision detection
        private void SpawnPowerUps(float segmentStart, float segmentLength)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            
            // Determine number of power-ups to spawn (usually just 1 per segment maximum)
            int powerUpCount = UnityEngine.Random.Range(1, maxPowerUpsPerSegment + 1);
            int spawnedPowerUps = 0;
            int maxAttempts = powerUpCount * 5; // Allow multiple attempts per power-up
            int attempts = 0;
            
            while (spawnedPowerUps < powerUpCount && attempts < maxAttempts)
            {
                attempts++;
                
                // Random lane
                int lane = UnityEngine.Random.Range(-1, 2);
                
                // Random z position within segment (avoid very start and very end)
                float zPos = segmentStart + UnityEngine.Random.Range(segmentLength * 0.2f, segmentLength * 0.8f);
                
                // Make sure power-ups are spaced well away from obstacles by using a larger check radius
                if (IsPositionClear(lane, zPos, 3.0f))
                {
                    // Calculate position - make power-ups float higher than coins
                    float xPos = lane * laneDistance;
                    float yPos = 1.5f; // Height above ground (higher than coins)
                    
                    // Select a random power-up prefab
                    GameObject powerUpPrefab = powerUpPrefabs[UnityEngine.Random.Range(0, powerUpPrefabs.Length)];
                    
                    // Instantiate power-up
                    GameObject powerUp = Instantiate(powerUpPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity, trackParent);
                    
                    // Ensure it has the right tag
                    powerUp.tag = "PowerUp";
                    
                    // Add to spawned objects
                    spawnedSegments.Add(powerUp);
                    
                    spawnedPowerUps++;
                    
                    // Add a slight visual offset to the power-up to make it more noticeable
                    StartCoroutine(AnimatePowerUp(powerUp.transform));
                }
            }
            
            if (spawnedPowerUps < powerUpCount && attempts >= maxAttempts)
            {
                Debug.Log($"Could only spawn {spawnedPowerUps}/{powerUpCount} power-ups due to space constraints");
            }
        }
        
        // Make power-ups float up and down slightly to make them more visible
        private IEnumerator AnimatePowerUp(Transform powerUpTransform)
        {
            if (powerUpTransform == null) yield break;
            
            Vector3 startPos = powerUpTransform.position;
            float animSpeed = UnityEngine.Random.Range(0.5f, 1.5f); // Random speed for variety
            float animHeight = 0.3f; // How high it floats up and down
            
            // Add rotation as well
            powerUpTransform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            
            float time = 0;
            while (powerUpTransform != null && powerUpTransform.gameObject.activeInHierarchy)
            {
                time += Time.deltaTime * animSpeed;
                
                if (powerUpTransform != null)
                {
                    // Up and down floating motion
                    float yOffset = Mathf.Sin(time) * animHeight;
                    powerUpTransform.position = new Vector3(
                        startPos.x, 
                        startPos.y + yOffset, 
                        startPos.z);
                    
                    // Slow rotation
                    powerUpTransform.Rotate(Vector3.up, 50 * Time.deltaTime);
                }
                
                yield return null;
            }
        }

        private IEnumerator CullDistantObjects()
        {
            // Wait until we have a valid player reference
            while (playerTransform == null)
            {
                // Try to find player if it wasn't set initially
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                yield return new WaitForSeconds(0.5f);
            }
            
            while (true)
            {
                if (CurrentGameState == GameState.Running)
                {
                    // Get current player position
                    Vector3 playerPosition = playerTransform.position;
                    Vector3 playerForward = playerTransform.forward;
                    
                    // Loop through all spawned objects
                    for (int i = spawnedSegments.Count - 1; i >= 0; i--)
                    {
                        if (spawnedSegments[i] == null)
                        {
                            // Remove null entries from the list
                            spawnedSegments.RemoveAt(i);
                            continue;
                        }
                        
                        GameObject obj = spawnedSegments[i];
                        
                        // Never cull the finish line or turn triggers
                        if (obj.CompareTag("Finish") || obj.name.Contains("TurnTrigger"))
                        {
                            // Apply LOD but never cull these important objects
                            ApplyLevelOfDetail(obj, Vector3.Distance(obj.transform.position, playerPosition));
                            continue;
                        }
                        
                        // Calculate vector from player to object
                        Vector3 playerToObj = obj.transform.position - playerPosition;
                        
                        // Project this vector onto player's forward direction to see if it's behind
                        float projectionOnPlayerForward = Vector3.Dot(playerToObj, playerForward);
                        
                        // Check if object is behind the player beyond the culling distance
                        // Only remove objects that are definitively behind the player in the direction they're facing
                        if (projectionOnPlayerForward < -cullingDistance)
                        {
                            // Remove and destroy object
                            spawnedSegments.RemoveAt(i);
                            Destroy(obj);
                            continue;
                        }
                        
                        // Calculate actual distance for LOD
                        float distance = Vector3.Distance(playerPosition, obj.transform.position);
                        
                        // Apply LOD based on actual distance
                        ApplyLevelOfDetail(obj, distance);
                    }
                }
                
                // Check every 0.1 seconds for smoother transitions
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // Helper method to apply level of detail based on distance
        private void ApplyLevelOfDetail(GameObject obj, float distance)
        {
            // Skip null objects
            if (obj == null) return;
            
            // Skip objects tagged for special treatment
            if (obj.CompareTag("Player")) return;
            
            // Always keep finish line and turn triggers visible
            bool isImportantObject = obj.CompareTag("Finish") || obj.name.Contains("TurnTrigger");
            
            // Get all renderers and LOD groups
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            LODGroup lodGroup = obj.GetComponent<LODGroup>();
            
            // If the object has a LOD group, Unity will handle LOD automatically
            if (lodGroup != null && !isImportantObject) return;
            
            // Distance checks - define visibility zones
            bool isNearZone = distance < 30f;                   // Full detail zone (<30 units)
            bool isMidZone = distance >= 30f && distance < 60f; // Mid detail zone (30-60 units)
            bool isFarZone = distance >= 60f && distance < visibilityDistance; // Far detail zone (60-visibility distance)
            bool isOutOfRange = distance >= visibilityDistance && !isImportantObject; // Out of range (but always keep important objects)
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                
                // Apply render state based on distance zone
                if (isOutOfRange)
                {
                    // Out of range - disable renderer
                    renderer.enabled = false;
                }
                else if (isNearZone)
                {
                    // Near zone - enable full detail
                    renderer.enabled = true;
                    
                    // Enable shadows for near objects
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
                else if (isMidZone)
                {
                    // Mid zone - enable with simpler shadows
                    renderer.enabled = true;
                    
                    // Cast shadows but don't receive for mid-range objects
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = false;
                }
                else if (isFarZone)
                {
                    // Far zone - enable with no shadows
                    renderer.enabled = true;
                    
                    // No shadows for far objects
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }
            
            // If we have mesh renderers, adjust their properties for mid and far zones
            if (isMidZone || isFarZone)
            {
                // Simplify or disable particle effects for distant objects
                ParticleSystem[] particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in particleSystems)
                {
                    if (ps == null) continue;
                    
                    var main = ps.main;
                    if (isFarZone)
                    {
                        // Far zone - reduce max particles
                        main.maxParticles = Mathf.Max(10, main.maxParticles / 4);
                    }
                    else if (isMidZone)
                    {
                        // Mid zone - slightly reduce max particles
                        main.maxParticles = Mathf.Max(20, main.maxParticles / 2);
                    }
                }
            }
        }

        // OnDestroy method to clean up resources
        private void OnDestroy()
        {
            // Stop all coroutines
            StopAllCoroutines();
            
            // Clear delegates and event subscriptions
            OnScoreChanged = null;
            OnGameStateChanged = null;
            
            // Clear any remaining game objects to prevent leaks
            if (spawnedSegments != null)
            {
                spawnedSegments.Clear();
            }
            
            // Force garbage collection
            System.GC.Collect();
        }

        // OnDisable method to stop coroutines
        private void OnDisable()
        {
            // Stop all coroutines when disabled
            StopAllCoroutines();
        }

        // Helper method to check if a position is clear (no obstacles, coins or power-ups)
        private bool IsPositionClear(Vector3 position, float checkRadius = 2.0f)
        {
            // Check for any colliders at this position
            Collider[] hitColliders = Physics.OverlapSphere(position, checkRadius);
            
            foreach (Collider collider in hitColliders)
            {
                // Skip these tags if they exist, otherwise use a try-catch to avoid errors
                try
                {
                    // These are the tags we want to ignore (safe objects to overlap with)
                    string[] ignoreTags = new string[] { "Track", "Finish", "Ground", "Player" };
                    
                    bool shouldIgnore = false;
                    foreach (string tag in ignoreTags) 
                    {
                        // Try to safely check the tag - if the tag doesn't exist, this will just return false
                        if (collider.gameObject.CompareTag(tag))
                        {
                            shouldIgnore = true;
                            break;
                        }
                    }
                    
                    if (shouldIgnore)
                    {
                        continue;
                    }
                }
                catch (UnityException)
                {
                    // Tag not defined, just continue with collision check
                    // We'll check the layer instead as a fallback
                    if (collider.gameObject.layer == LayerMask.NameToLayer("Default") || 
                        collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        continue;
                    }
                }
                
                // If we found any other collider, position is not clear
                return false;
            }
            
            return true;
        }

        // Overload for IsPositionClear that takes lane, zPos, and checkRadius
        private bool IsPositionClear(int lane, float zPos, float checkRadius = 2.0f)
        {
            // Calculate position based on lane and z-position
            float xPos = lane * laneDistance;
            float yPos = 1f; // Height above ground
            
            // Calculate right vector perpendicular to the track direction
            Vector3 rightVector = Vector3.Cross(Vector3.up, currentTrackDirection).normalized;
            
            // Calculate the actual position with the current track direction
            Vector3 position = new Vector3(0, yPos, zPos) + (rightVector * xPos);
            
            // Use the existing method to check if the position is clear
            return IsPositionClear(position, checkRadius);
        }

        // Helper method to shuffle an array (Fisher-Yates algorithm)
        private void ShuffleArray<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                // Get random index from i to end
                int r = i + UnityEngine.Random.Range(0, n - i);
                
                // Swap elements
                T temp = array[i];
                array[i] = array[r];
                array[r] = temp;
            }
        }
    }
} 