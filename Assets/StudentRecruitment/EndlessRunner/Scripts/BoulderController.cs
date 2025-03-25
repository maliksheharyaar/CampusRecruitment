using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    [RequireComponent(typeof(SphereCollider))]
    public class BoulderController : MonoBehaviour
    {
        [Header("Boulder Settings")]
        [SerializeField] private float moveSpeed = 8f; // Constant forward speed
        [SerializeField] private float rotationSpeed = 180f; // Rolling rotation speed
        [SerializeField] private Transform boulderModel;
        [SerializeField] private float turnDuration = 1.0f; // How long a turn takes
        [SerializeField] private float followDistance = 30f; // How far behind the player
        [SerializeField] private float turnCooldown = 2.0f; // Minimum time between turns

        [Header("Catch-up Mechanic")]
        [SerializeField] private float maxFollowDistance = 50f; // If boulder falls beyond this distance, speed up
        [SerializeField] private float catchUpSpeedMultiplier = 1.5f; // Speed multiplier when catching up
        [SerializeField] private float targetCatchUpDistance = 15f; // Slow down once boulder is this close

        [Header("Effects")]
        [SerializeField] private ParticleSystem dustEffect;
        [SerializeField] private AudioClip rollingSound;

        // Components
        private SphereCollider boulderCollider;
        private AudioSource audioSource;
        private Transform playerTransform;
        private GameObject turnDetector;

        // State
        private bool isMoving = false;
        private Vector3 moveDirection = Vector3.forward; // Current movement direction
        private bool isTurning = false;
        private Coroutine currentTurnCoroutine = null;
        private float lastTurnTime = -999f; // Time of the last turn

        private void Awake()
        {
            // Get components
            boulderCollider = GetComponent<SphereCollider>();
            audioSource = GetComponent<AudioSource>();
            playerTransform = FindObjectOfType<RunnerController>()?.transform;

            // Set up collider
            boulderCollider.isTrigger = false; // Main collider for physics

            // Create a dedicated turn detector GameObject with trigger
            CreateTurnDetector();

            // Set up audio source if not present
            if (audioSource == null && rollingSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = rollingSound;
                audioSource.loop = true;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.volume = 0.7f;
            }

            // Initialize
            moveDirection = Vector3.forward;
        }

        // Create a dedicated object to detect turn triggers
        private void CreateTurnDetector()
        {
            turnDetector = new GameObject("BoulderTurnDetector");
            turnDetector.transform.SetParent(transform);
            turnDetector.transform.localPosition = Vector3.zero;
            turnDetector.transform.localRotation = Quaternion.identity;
            
            // Add a BoxCollider as trigger with specified values
            BoxCollider detectorCollider = turnDetector.AddComponent<BoxCollider>();
            detectorCollider.isTrigger = true;
            detectorCollider.size = new Vector3(5f, 1f, 1f); // Use the specified size
            detectorCollider.center = new Vector3(0f, -6f, -6.5f); // Updated z-coordinate from -3 to -6.5
            
            // Add a Rigidbody to ensure trigger detection (set to kinematic so it doesn't affect physics)
            Rigidbody rb = turnDetector.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            // Set the layer (optional: you could create a dedicated layer for turn detection)
            turnDetector.layer = LayerMask.NameToLayer("Default");
            
            // Add the TurnDetector component
            TurnDetector detector = turnDetector.AddComponent<TurnDetector>();
            detector.Initialize(this);
            
            Debug.Log("Created turn detector with collider size: " + detectorCollider.size + ", center: " + detectorCollider.center);
        }

        // Method to start boulder moving
        public void StartChasing()
        {
            isMoving = true;

            // Start dust effect
            if (dustEffect != null)
            {
                dustEffect.Play();
            }

            // Start sound
            if (audioSource != null && rollingSound != null)
            {
                audioSource.Play();
            }
            
            // Position behind player at start
            if (playerTransform != null)
            {
                // Set position behind player
                Vector3 newPosition = playerTransform.position - (playerTransform.forward * followDistance);
                newPosition.y = transform.position.y; // Keep current height
                transform.position = newPosition;
                
                // Match player's direction
                moveDirection = playerTransform.forward;
                transform.forward = moveDirection;
            }
        }

        // Method to stop boulder
        public void StopChasing()
        {
            isMoving = false;

            // Stop any active turn coroutines
            if (currentTurnCoroutine != null)
            {
                StopCoroutine(currentTurnCoroutine);
                currentTurnCoroutine = null;
            }

            // Stop dust effect
            if (dustEffect != null)
            {
                dustEffect.Stop();
            }

            // Stop sound
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void Update()
        {
            if (!isMoving) return;
            
            // Calculate actual speed based on distance to player
            float currentSpeed = moveSpeed;
            
            // Check if player exists
            if (playerTransform != null)
            {
                // Project the distance to player along our movement direction
                Vector3 vectorToPlayer = playerTransform.position - transform.position;
                float distanceToPlayer = Vector3.Dot(vectorToPlayer, moveDirection);
                
                // Debug.Log($"Distance to player: {distanceToPlayer}");
                
                // If we're too far behind, activate catch-up speed
                if (distanceToPlayer > maxFollowDistance)
                {
                    currentSpeed = moveSpeed * catchUpSpeedMultiplier;
                    if (dustEffect != null)
                    {
                        // Increase dust effect size/rate for visual feedback
                        var main = dustEffect.main;
                        main.startSizeMultiplier = 1.5f;
                        var emission = dustEffect.emission;
                        emission.rateOverTimeMultiplier = 1.5f;
                    }
                    
                    if (audioSource != null)
                    {
                        // Increase pitch for audio feedback
                        audioSource.pitch = 1.2f;
                    }
                    
                    Debug.Log($"Boulder catch-up activated! Distance: {distanceToPlayer:F1}, Speed: {currentSpeed:F1}");
                }
                // If we've caught up to the target distance, resume normal speed
                else if (distanceToPlayer <= targetCatchUpDistance)
                {
                    currentSpeed = moveSpeed;
                    if (dustEffect != null)
                    {
                        // Reset dust effect
                        var main = dustEffect.main;
                        main.startSizeMultiplier = 1.0f;
                        var emission = dustEffect.emission;
                        emission.rateOverTimeMultiplier = 1.0f;
                    }
                    
                    if (audioSource != null)
                    {
                        // Reset pitch
                        audioSource.pitch = 1.0f;
                    }
                }
                // Otherwise, if we're in the catch-up zone, maintain catch-up speed until target is reached
                else if (distanceToPlayer > targetCatchUpDistance && currentSpeed > moveSpeed)
                {
                    // Keep the catch-up speed
                    currentSpeed = moveSpeed * catchUpSpeedMultiplier;
                }
            }
            
            // Simply move forward in current direction at calculated speed
            if (!isTurning)
            {
                transform.position += moveDirection * currentSpeed * Time.deltaTime;
                
                // Keep boulder facing its movement direction
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            
            // Rotate boulder model to simulate rolling
            if (boulderModel != null)
            {
                boulderModel.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
            }
        }
        
        // Called from TurnDetector when a turn trigger is detected
        public void HandleTurnTrigger(bool isLeftTurn)
        {
            // Check if enough time has passed since the last turn (cooldown)
            if (!isTurning && Time.time > lastTurnTime + turnCooldown)
            {
                StartTurn(isLeftTurn);
                lastTurnTime = Time.time; // Record the time of this turn
                Debug.Log($"Turn initiated, next turn available in {turnCooldown} seconds");
            }
            else if (Time.time <= lastTurnTime + turnCooldown)
            {
                Debug.Log($"Turn ignored - cooldown active. {lastTurnTime + turnCooldown - Time.time:F2} seconds remaining");
            }
        }
        
        // Detect player collision
        private void OnTriggerEnter(Collider other)
        {
            // If we hit the player, it's game over
            if (other.CompareTag("Player") && isMoving)
            {
                RunnerController runner = other.GetComponent<RunnerController>();
                if (runner != null)
                {
                    // Kill the player by triggering game over
                    EndlessRunnerManager.Instance.OnPlayerDeath();
                }
            }
        }
        
        // Start a turning maneuver for the boulder
        private void StartTurn(bool isLeftTurn)
        {
            // Don't start another turn if already turning
            if (isTurning) return;
            
            Debug.Log($"Boulder turning: {(isLeftTurn ? "LEFT" : "RIGHT")}");
            
            // Start turn coroutine
            if (currentTurnCoroutine != null)
            {
                StopCoroutine(currentTurnCoroutine);
            }
            
            currentTurnCoroutine = StartCoroutine(TurnCoroutine(isLeftTurn));
        }
        
        // Turn coroutine - similar to player's turn logic
        private IEnumerator TurnCoroutine(bool isLeftTurn)
        {
            // Set turning state
            isTurning = true;
            
            // Calculate new direction based on turn direction
            float turnAngle = isLeftTurn ? -90f : 90f;
            Vector3 newMoveDirection = Quaternion.Euler(0, turnAngle, 0) * moveDirection;
            
            // Store current rotation and position
            Quaternion fromRotation = transform.rotation;
            Quaternion toRotation = Quaternion.LookRotation(newMoveDirection);
            Vector3 turnCenterPosition = transform.position;
            
            // Calculate turn radius and center point
            float turnRadius = 5f; // Similar to a track segment width
            Vector3 turnCenter = turnCenterPosition + (isLeftTurn ? transform.right : -transform.right) * turnRadius;
            
            // Track turn progress
            float turnStartTime = Time.time;
            float elapsedTime = 0f;
            
            // Perform the turn over time
            while (elapsedTime < turnDuration)
            {
                elapsedTime = Time.time - turnStartTime;
                float t = elapsedTime / turnDuration; 
                
                // Use smoothstep for easing
                float smoothT = t * t * (3f - 2f * t);
                
                // Rotate the boulder
                transform.rotation = Quaternion.Slerp(fromRotation, toRotation, smoothT);
                
                // Move in an arc during turn
                float angle = isLeftTurn ? -smoothT * 90f : smoothT * 90f;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * (turnCenterPosition - turnCenter);
                transform.position = turnCenter + offset;
                
                yield return null;
            }
            
            // Ensure final rotation and direction are exact
            transform.rotation = toRotation;
            moveDirection = newMoveDirection;
            
            // Exit turning state
            isTurning = false;
            currentTurnCoroutine = null;
            
            Debug.Log($"Boulder turn complete. New direction: {moveDirection}");
        }

        private void OnDisable()
        {
            // Stop any active coroutines when disabled
            if (currentTurnCoroutine != null)
            {
                StopCoroutine(currentTurnCoroutine);
                currentTurnCoroutine = null;
            }
        }
    }

    // Separate component to handle turn trigger detection
    public class TurnDetector : MonoBehaviour
    {
        private BoulderController boulderController;

        public void Initialize(BoulderController controller)
        {
            boulderController = controller;
            Debug.Log("TurnDetector initialized");
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"TurnDetector triggered by: {other.gameObject.name}, tag: {other.tag}");
            
            // Check for turn triggers with broader criteria
            if (other.CompareTag("Track") || other.name.Contains("TurnTrigger") || other.name.Contains("Turn"))
            {
                Debug.Log($"Checking object for TurnTrigger: {other.gameObject.name}");
                
                // Try multiple ways to find the TurnTrigger
                TurnTrigger turnTrigger = null;
                
                // Check on the object itself
                turnTrigger = other.GetComponent<TurnTrigger>();
                if (turnTrigger != null)
                    Debug.Log("Found TurnTrigger on the object itself");
                
                // If not found, check parents
                if (turnTrigger == null)
                {
                    turnTrigger = other.GetComponentInParent<TurnTrigger>();
                    if (turnTrigger != null)
                        Debug.Log("Found TurnTrigger in parent");
                }
                
                // If still not found, check children
                if (turnTrigger == null)
                {
                    turnTrigger = other.GetComponentInChildren<TurnTrigger>();
                    if (turnTrigger != null)
                        Debug.Log("Found TurnTrigger in children");
                }
                
                // As a last resort, check a specific child called TurnTriggerZone
                if (turnTrigger == null)
                {
                    Transform triggerZone = other.transform.Find("TurnTriggerZone");
                    if (triggerZone != null)
                    {
                        turnTrigger = triggerZone.GetComponent<TurnTrigger>();
                        if (turnTrigger != null)
                            Debug.Log("Found TurnTrigger in TurnTriggerZone child");
                    }
                }
                
                // If we found a TurnTrigger component, trigger the turn
                if (turnTrigger != null)
                {
                    bool isLeftTurn = turnTrigger.IsLeftTurn();
                    Debug.Log($"Turn trigger found: {(isLeftTurn ? "LEFT" : "RIGHT")}");
                    boulderController.HandleTurnTrigger(isLeftTurn);
                }
                else
                {
                    // If no TurnTrigger is found but name contains 'left' or 'right', guess the direction
                    string objName = other.gameObject.name.ToLower();
                    if (objName.Contains("left"))
                    {
                        Debug.Log("No TurnTrigger found, but name contains 'left' - assuming LEFT turn");
                        boulderController.HandleTurnTrigger(true); // Left turn
                    }
                    else if (objName.Contains("right"))
                    {
                        Debug.Log("No TurnTrigger found, but name contains 'right' - assuming RIGHT turn");
                        boulderController.HandleTurnTrigger(false); // Right turn
                    }
                    else
                    {
                        Debug.LogWarning($"Track object {other.gameObject.name} doesn't have TurnTrigger component!");
                    }
                }
            }
        }
        
        // Add OnTriggerStay in case OnTriggerEnter is missed
        private void OnTriggerStay(Collider other)
        {
            // Only check occasionally to avoid spamming
            if (Time.frameCount % 10 == 0)
            {
                OnTriggerEnter(other);
            }
        }
    }
} 