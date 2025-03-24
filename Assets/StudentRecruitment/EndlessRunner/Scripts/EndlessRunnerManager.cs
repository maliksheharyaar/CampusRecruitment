using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            float currentZPosition = 0f;

            // Spawn regular track segments
            for (int i = 0; i < trackSegmentsToSpawn; i++)
            {
                // Select a random track segment prefab
                GameObject segmentPrefab = trackSegmentPrefabs[UnityEngine.Random.Range(0, trackSegmentPrefabs.Length)];
                
                // Instantiate the segment
                GameObject segment = Instantiate(segmentPrefab, new Vector3(0, 0, currentZPosition), Quaternion.identity, trackParent);
                spawnedSegments.Add(segment);
                
                // Randomly add obstacle spawners near the end of each segment (except the last one)
                if (i < trackSegmentsToSpawn - 1 && UnityEngine.Random.Range(0f, 1f) < obstacleSpawnChance)
                {
                    // Place obstacles at random positions along the segment
                    float obstacleZ = currentZPosition + UnityEngine.Random.Range(segmentLength * 0.4f, segmentLength * 0.9f);
                    SpawnObstacles(obstacleZ);
                }
                
                // Spawn coins along the segment
                if (coinPrefab != null && UnityEngine.Random.Range(0f, 1f) < coinSpawnChance)
                {
                    SpawnCoins(currentZPosition, segmentLength);
                }
                
                // Spawn power-ups along the segment (more rare than coins)
                if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && UnityEngine.Random.Range(0f, 1f) < powerUpSpawnChance)
                {
                    SpawnPowerUps(currentZPosition, segmentLength);
                }
                
                // Move to next position
                currentZPosition += segmentLength;
            }

            // Spawn finish line at the end
            if (finishLinePrefab != null)
            {
                GameObject finishLine = Instantiate(finishLinePrefab, new Vector3(0, 0, currentZPosition - 15), Quaternion.identity, trackParent);
                spawnedSegments.Add(finishLine);
                
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

            // Calculate total track length
            totalTrackLength = currentZPosition;
        }

        // Method to spawn obstacles
        private void SpawnObstacles(float zPosition)
        {
            // Skip if no obstacle prefabs are assigned
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            {
                Debug.LogWarning("No obstacle prefabs assigned");
                return;
            }

            // Choose a random lane (0, 1, 2)
            int lane = UnityEngine.Random.Range(0, 3);
            
            // Calculate position based on lane
            float xPosition = (lane - 1) * laneDistance;
            Vector3 obstaclePosition = new Vector3(xPosition, 0, zPosition);
            
            // Choose a random obstacle prefab
            GameObject obstaclePrefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
            
            // Instantiate with prefab's original rotation instead of Quaternion.identity
            GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, obstaclePrefab.transform.rotation);
            
            // Add to spawned segments list to track
            spawnedSegments.Add(obstacle);
        }

        // New method to spawn coins with collision detection
        private void SpawnCoins(float segmentStart, float segmentLength)
        {
            if (coinPrefab == null) return;
            
            // Determine number of coins to spawn
            int coinCount = UnityEngine.Random.Range(1, maxCoinsPerSegment + 1);
            int spawnedCoins = 0;
            int maxAttempts = coinCount * 5; // Allow multiple attempts per coin
            int attempts = 0;
            
            while (spawnedCoins < coinCount && attempts < maxAttempts)
            {
                attempts++;
                
                // Random lane
                int lane = UnityEngine.Random.Range(-1, 2);
                
                // Random z position within segment (avoid very start and very end)
                float zPos = segmentStart + UnityEngine.Random.Range(segmentLength * 0.1f, segmentLength * 0.9f);
                
                // Check if position is clear before spawning
                if (IsPositionClear(lane, zPos))
                {
                    // Calculate position
                    float xPos = lane * laneDistance;
                    float yPos = 1.0f; // Height above ground
                    
                    // Instantiate coin
                    GameObject coin = Instantiate(coinPrefab, new Vector3(xPos, yPos, zPos), Quaternion.identity, trackParent);
                    
                    // Ensure it has the right tag
                    coin.tag = "Coin";
                    
                    // Add to spawned objects
                    spawnedSegments.Add(coin);
                    
                    spawnedCoins++;
                }
            }
            
            if (spawnedCoins < coinCount)
            {
                Debug.Log($"Could only spawn {spawnedCoins}/{coinCount} coins due to space constraints");
            }
        }

        private void SpawnObstacle(int lane, float zPosition)
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
            
            // Select random obstacle prefab
            GameObject obstaclePrefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
            
            // Calculate lane position
            float xPosition = lane * laneDistance;
            
            // Randomly offset the position slightly to avoid perfect alignment
            float xOffset = UnityEngine.Random.Range(-0.2f, 0.2f);
            float zOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
            
            // Make a final check if the position is clear with offsets
            if (!IsPositionClear(xPosition + xOffset, zPosition + zOffset, 1.5f)) // Larger radius for final check
            {
                Debug.Log($"Position ({xPosition}, {zPosition}) no longer clear with offsets, aborting spawn");
                return;
            }
            
            // Instantiate obstacle - use prefab's original rotation instead of Quaternion.identity
            GameObject obstacle = Instantiate(obstaclePrefab, 
                new Vector3(xPosition + xOffset, 0, zPosition + zOffset), 
                obstaclePrefab.transform.rotation, trackParent);
            
            // Ensure it has the right tag
            obstacle.tag = "Obstacle";
            
            // Add to spawned objects for later cleanup
            spawnedSegments.Add(obstacle);
        }
        
        // Helper method to check if a position is clear (no obstacles, coins or power-ups)
        private bool IsPositionClear(int lane, float zPosition)
        {
            float xPosition = lane * laneDistance;
            return IsPositionClear(xPosition, zPosition, 2.0f); // Default check radius
        }
        
        private bool IsPositionClear(float xPosition, float zPosition, float checkRadius = 2.0f)
        {
            // Position to check (y position is 1.0 to check for hovering items)
            Vector3 checkPosition = new Vector3(xPosition, 1.0f, zPosition);
            
            // Check for any colliders at this position
            Collider[] hitColliders = Physics.OverlapSphere(checkPosition, checkRadius);
            
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
            // Find all track segments
            GameObject[] trackSegments = GameObject.FindGameObjectsWithTag("Track");
            
            // If no track segments found or if we never generated a track, create a new one
            if (trackSegments.Length == 0 || spawnedSegments.Count == 0)
            {
                Debug.Log("No track segments found, generating new track");
                GenerateTrack();
                return;
            }
            
            Debug.Log($"Regenerating gameplay objects on {trackSegments.Length} existing track segments");
            
            // Sort track segments by Z position to maintain order
            System.Array.Sort(trackSegments, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));
            
            // Spawn gameplay objects on each segment
            for (int i = 0; i < trackSegments.Length; i++)
            {
                float segmentStart = trackSegments[i].transform.position.z;
                
                // Randomly add obstacles (except on the last segment)
                if (i < trackSegments.Length - 1 && UnityEngine.Random.Range(0f, 1f) < obstacleSpawnChance)
                {
                    float obstacleZ = segmentStart + UnityEngine.Random.Range(segmentLength * 0.4f, segmentLength * 0.9f);
                    SpawnObstacles(obstacleZ);
                }
                
                // Spawn coins
                if (coinPrefab != null && UnityEngine.Random.Range(0f, 1f) < coinSpawnChance)
                {
                    SpawnCoins(segmentStart, segmentLength);
                }
                
                // Spawn power-ups (more rare)
                if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && UnityEngine.Random.Range(0f, 1f) < powerUpSpawnChance)
                {
                    SpawnPowerUps(segmentStart, segmentLength);
                }
            }
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
                    // Get current player z position to determine culling
                    float playerZ = playerTransform.position.z;
                    
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
                        float objDistance = obj.transform.position.z - playerZ;
                        
                        // Check if object is behind the player beyond the culling distance
                        if (objDistance < -cullingDistance)
                        {
                            // Remove and destroy object
                            spawnedSegments.RemoveAt(i);
                            Destroy(obj);
                        }
                        else
                        {
                            // Apply LOD (Level of Detail) based on distance
                            ApplyLevelOfDetail(obj, objDistance);
                        }
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
            if (obj.CompareTag("Player") || obj.CompareTag("Finish")) return;
            
            // Get all renderers and LOD groups
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            LODGroup lodGroup = obj.GetComponent<LODGroup>();
            
            // If the object has a LOD group, Unity will handle LOD automatically
            if (lodGroup != null) return;
            
            // Distance checks - define visibility zones
            bool isNearZone = Mathf.Abs(distance) < 30f;                   // Full detail zone (<30 units)
            bool isMidZone = distance >= 30f && distance < 60f;            // Mid detail zone (30-60 units)
            bool isFarZone = distance >= 60f && distance < visibilityDistance; // Far detail zone (60-visibility distance)
            bool isOutOfRange = Mathf.Abs(distance) >= visibilityDistance;    // Out of range
            
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
    }
} 