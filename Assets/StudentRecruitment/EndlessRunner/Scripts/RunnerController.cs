using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using StudentRecruitment.EndlessRunner;

namespace StudentRecruitment.EndlessRunner
{
    [RequireComponent(typeof(CharacterController))]
    public class RunnerController : MonoBehaviour
    {
        [Header("Player Movement")]
        [SerializeField] private float laneDistance = 3f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpTime = 0.5f;
        [SerializeField] private float laneChangeSpeed = 5f;
        
        [Header("Forward Movement")]
        [SerializeField] private float forwardSpeed = 10f;
        [SerializeField] private float maxForwardSpeed = 20f;
        [SerializeField] private bool autoStart = true; // Auto-start forward movement
        [SerializeField] private bool useDirectControls = true; // Enable direct keyboard controls

        [Header("Character Model")]
        [SerializeField] private GameObject characterModel; // Reference to the 3D character model
        [SerializeField] private Transform modelTransform; // Reference to model transform for rotation
        [SerializeField] private float rotationSpeed = 10f; // How fast the character rotates when changing lanes

        [Header("Player State")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private GameObject shieldVFX;
        [SerializeField] private float bounceBackDistance = 1.0f; // How far to bounce back when hitting obstacle

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        [Header("Audio")]
        [SerializeField] private AudioClip runningSound;
        [SerializeField, Range(0f, 1f), Tooltip("Volume of the running sound effect")] 
        private float runningSoundVolume = 0.5f;
        [SerializeField, Range(0.5f, 2f), Tooltip("Pitch/rate of the running sound effect")] 
        private float runningSoundPitch = 1.0f;
        [SerializeField] private bool enableRunningSound = true;

        // Components
        private CharacterController controller;
        private Animator animator;
        private PlayerInputActions inputActions;
        private IDisposable inputBindings;
        private AudioSource audioSource;

        // Movement state
        private int targetLane = 1; // 0: left, 1: center, 2: right
        private float currentLanePosition = 0f;
        private Vector3 originalPosition;
        private float originalHeight;
        private float originalCenterY;
        private bool isBouncing = false;
        private float bounceTime = 0f;

        // Player state
        private bool isJumping = false;
        private int lives;
        private bool isInvincible = false;
        private bool isGrounded = false;
        private Coroutine powerUpCoroutine;
        public bool isFinished = false;
        private bool canMove = true; // Flag to control movement during bounce back

        // Events
        public static event Action<int> OnLivesChanged;
        public static event Action<PowerUpType> OnPowerUpCollected;
        public static event Action<int, int> OnHealthChanged;
        public static event Action<bool, float, float> OnInvincibilityChanged;

        // Properties
        public int CurrentLives => lives;
        public int TargetLane => targetLane;
        public bool IsInvincible => isInvincible;
        public int Health { get { return lives; } private set { lives = value; } }
        public int MaxHealth => maxLives;
        
        // Add new property for tracking death state
        public bool isDead = false;

        private void Awake()
        {
            // Get components
            controller = GetComponent<CharacterController>();
            
            // Get animator from character model if available
            if (characterModel != null)
            {
                animator = characterModel.GetComponent<Animator>();
                
                // If no animator on the model itself, try to find it in children
                if (animator == null)
                {
                    animator = characterModel.GetComponentInChildren<Animator>();
                }
            }
            else
            {
                // Fallback to getting animator from children
                animator = GetComponentInChildren<Animator>();
            }
            
            // Log warning if animator is still null
            if (animator == null)
            {
                Debug.LogWarning("No Animator component found on the character model.");
            }
            
            // Initialize
            lives = maxLives;
            OnLivesChanged?.Invoke(lives);
            OnHealthChanged?.Invoke(lives, maxLives);
            
            // Store original position and height
            originalPosition = transform.position;
            originalHeight = controller.height;
            originalCenterY = controller.center.y;
            
            // Set up input actions
            inputActions = new PlayerInputActions();
            
            // Get or add AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = true;
                audioSource.spatialBlend = 0.0f; // 2D sound for running
                audioSource.volume = runningSoundVolume;
            }
        }

        private void Start()
        {
            // If autoStart is enabled, set an initial forward speed
            if (autoStart)
            {
                forwardSpeed = 10f; // Default starting speed
                
                // Set running animation state
                if (animator != null)
                {
                    animator.SetBool("IsRunning", true);
                }
            }
            
            // Configure audio
            if (audioSource != null && runningSound != null)
            {
                audioSource.clip = runningSound;
                audioSource.volume = runningSoundVolume;
                audioSource.pitch = runningSoundPitch;
            }
        }

        private void OnEnable()
        {
            inputActions?.Enable();
            inputBindings = inputActions?.BindPlayerControls(this);
        }

        private void OnDisable()
        {
            // Stop all coroutines when disabled
            StopAllCoroutines();
            
            // Clean up input systems completely
            if (inputActions != null)
            {
                inputActions.Disable();
            }
            
            if (inputBindings != null)
            {
                inputBindings.Dispose();
                inputBindings = null;
            }
        }

        private void Update()
        {
            // Skip all movement and input processing if player is dead or finished
            if (isDead || isFinished) return;

            // Check if grounded
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            // Process direct keyboard controls
            if (useDirectControls)
            {
                HandleDirectControls();
            }

            // Update lane position
            HandleLaneMovement();
            
            // Handle bounce back effect
            HandleBounce();
            
            // Add forward movement
            MoveForward();
            
            // Handle running sound
            UpdateRunningSound();
            
            // Log position occasionally for debugging
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Player position: {transform.position}, Speed: {forwardSpeed}, Grounded: {isGrounded}, Lane: {targetLane}");
            }
        }
        
        // Handle direct keyboard input
        private void HandleDirectControls()
        {
            // Jump
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                OnJumpInput();
            }
            
            // Move left
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnMoveLeftInput();
            }
            
            // Move right
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnMoveRightInput();
            }
            
            // Pause
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                OnPauseInput();
            }
        }
        
        // Handle bounce back effect after hitting obstacle
        private void HandleBounce()
        {
            if (isBouncing)
            {
                // Bounce effect lasts 0.5 seconds
                bounceTime -= Time.deltaTime;
                
                if (bounceTime <= 0)
                {
                    isBouncing = false;
                }
            }
        }

        // Method to set forward speed from EndlessRunnerManager
        public void SetForwardSpeed(float speed)
        {
            // Don't allow speed changes if player has finished
            if (isFinished) 
            {
                forwardSpeed = 0;
                return;
            }
            
            forwardSpeed = Mathf.Clamp(speed, 0, maxForwardSpeed);
        }
        
        // Add forward movement method
        private void MoveForward()
        {
            // Don't move if finished, bouncing, or canMove is false
            if (isFinished || isBouncing || !canMove) return;
            
            // Create movement vector
            Vector3 moveDirection = Vector3.forward * forwardSpeed * Time.deltaTime;
            
            // Add gravity if not grounded
            if (!isGrounded)
            {
                moveDirection.y = Physics.gravity.y * Time.deltaTime;
            }
            
            try
            {
                // Use ONLY character controller for movement (not transform.Translate)
                if (controller != null && controller.enabled)
                {
                    controller.Move(moveDirection);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error moving player: " + e.Message);
            }
        }

        // Handle lane movement and rotate the character model
        private void HandleLaneMovement()
        {
            // Don't change lanes if finished or bouncing
            if (isFinished || isBouncing) return;
            
            // Calculate target x position
            float targetX = (targetLane - 1) * laneDistance;
            
            // Smoothly move toward target lane
            float previousLanePosition = currentLanePosition;
            currentLanePosition = Mathf.Lerp(currentLanePosition, targetX, Time.deltaTime * laneChangeSpeed);
            
            try
            {
                // Apply the horizontal movement directly to the controller
                Vector3 moveDirection = new Vector3(currentLanePosition - transform.position.x, 0, 0);
                controller.Move(moveDirection);
                
                // Rotate the character model when changing lanes
                if (characterModel != null || modelTransform != null)
                {
                    Transform modelToRotate = (modelTransform != null) ? modelTransform : characterModel.transform;
                    
                    // Calculate the direction vector
                    float horizontalMovement = currentLanePosition - previousLanePosition;
                    
                    // Only rotate if there's significant horizontal movement
                    if (Mathf.Abs(horizontalMovement) > 0.01f)
                    {
                        // Calculate target rotation (lean slightly into direction of movement)
                        float targetYRotation = 0f;
                        if (horizontalMovement < 0)
                        {
                            // Moving left - rotate slightly left
                            targetYRotation = -30f;
                        }
                        else if (horizontalMovement > 0)
                        {
                            // Moving right - rotate slightly right
                            targetYRotation = 30f;
                        }
                        
                        // Smoothly rotate towards the target rotation
                        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);
                        modelToRotate.rotation = Quaternion.Lerp(modelToRotate.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                    }
                    else
                    {
                        // Return to forward rotation when not moving horizontally
                        Quaternion forwardRotation = Quaternion.Euler(0, 0, 0);
                        modelToRotate.rotation = Quaternion.Lerp(modelToRotate.rotation, forwardRotation, Time.deltaTime * rotationSpeed);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in lane movement: " + e.Message);
            }
        }

        // Input callback methods
        public void OnJumpInput()
        {
            if (!isJumping && isGrounded && !isFinished)
            {
                StartCoroutine(JumpCoroutine());
            }
        }

        public void OnMoveLeftInput()
        {
            if (targetLane > 0 && !isFinished && !isBouncing)
            {
                targetLane--;
                if (animator != null)
                {
                    animator.SetBool("IsRunning", true);
                }
            }
        }

        public void OnMoveRightInput()
        {
            if (targetLane < 2 && !isFinished && !isBouncing)
            {
                targetLane++;
                if (animator != null)
                {
                    animator.SetBool("IsRunning", true);
                }
            }
        }

        public void OnPauseInput()
        {
            if (EndlessRunnerManager.Instance != null)
            {
                EndlessRunnerManager.Instance.UpdateGameState(GameState.Paused);
            }
        }

        private IEnumerator JumpCoroutine()
        {
            isJumping = true;
            
            // Pause running sound when jumping
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            
            // Start jump animation
            if (animator != null)
            {
                animator.SetBool("IsJumping", true);
            }
            
            // Jump upward phase
            float jumpStartTime = Time.time;
            float jumpDuration = jumpTime / 2;
            float startHeight = transform.position.y;
            float endHeight = startHeight + jumpHeight;
            
            while (Time.time < jumpStartTime + jumpDuration)
            {
                float t = (Time.time - jumpStartTime) / jumpDuration;
                float height = Mathf.Lerp(startHeight, endHeight, t);
                
                // Apply jump height while preserving horizontal and forward movement
                Vector3 moveDirection = new Vector3(0, height - transform.position.y, 0);
                controller.Move(moveDirection);
                
                yield return null;
            }
            
            // Fall downward phase
            float fallStartTime = Time.time;
            float fallDuration = jumpTime / 2;
            
            while (Time.time < fallStartTime + fallDuration)
            {
                float t = (Time.time - fallStartTime) / fallDuration;
                float height = Mathf.Lerp(endHeight, startHeight, t);
                
                // Apply fall height while preserving horizontal and forward movement
                Vector3 moveDirection = new Vector3(0, height - transform.position.y, 0);
                controller.Move(moveDirection);
                
                yield return null;
            }
            
            // Ensure we land at the right height
            Vector3 finalMove = new Vector3(0, startHeight - transform.position.y, 0);
            controller.Move(finalMove);
            
            // Set animations back to running
            if (animator != null)
            {
                animator.SetBool("IsJumping", false);
            }
            
            isJumping = false;
            
            // Resume running sound if appropriate
            UpdateRunningSound();
        }

        // Collision handling
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Skip collision handling if the player is finished or invincible
            if (isFinished || isInvincible) return;

            // Handle obstacle collision
            if (hit.gameObject.CompareTag("Obstacle"))
            {
                // Only handle obstacle hit if not already bouncing
                if (!isBouncing)
                {
                    HandleObstacleHit();
                    // StartBounceBack is called inside HandleObstacleHit
                }
            }
            // Handle finish line
            else if (hit.gameObject.CompareTag("Finish"))
            {
                HandleFinish();
            }
        }

        // Start bounce back effect
        private void StartBounceBack()
        {
            if (isBouncing) return;
            
            // Start bounce effect
            isBouncing = true;
            bounceTime = 0.5f;
            
            // Apply bounce-back movement
            Vector3 bounceDirection = Vector3.back * bounceBackDistance;
            controller.Move(bounceDirection);
        }

        // Trigger handling
        private void OnTriggerEnter(Collider other)
        {
            // Handle coin collection
            if (other.CompareTag("Coin"))
            {
                Coin coin = other.GetComponent<Coin>();
                if (coin != null)
                {
                    EndlessRunnerManager.Instance.AddScore(coin.CoinValue);
                    coin.Collect();
                }
            }
            // Handle power-up collection
            else if (other.CompareTag("PowerUp"))
            {
                PowerUp powerUp = other.GetComponent<PowerUp>();
                if (powerUp != null)
                {
                    ApplyPowerUp(powerUp.Type);
                    powerUp.Collect();
                }
            }
            // Handle finish line (also as trigger)
            else if (other.CompareTag("Finish"))
            {
                HandleFinish();
            }
            // Handle obstacle (also as trigger)
            else if (other.CompareTag("Obstacle") && !isInvincible)
            {
                // Only handle obstacle hit if not already bouncing
                if (!isBouncing)
                {
                    HandleObstacleHit();
                    // StartBounceBack is called inside HandleObstacleHit
                }
            }
        }

        // Handle hitting an obstacle
        private void HandleObstacleHit()
        {
            if (isInvincible || isDead || isFinished || isBouncing) return;
            
            // Start bounce back effect first
            StartBounceBack();
            
            // Take damage
            lives--;
            
            // Trigger events
            OnLivesChanged?.Invoke(lives);
            OnHealthChanged?.Invoke(lives, maxLives);
            
            // Check if dead
            if (lives <= 0)
            {
                Die();
                return;
            }
        }

        // Handle reaching the finish line
        private void HandleFinish()
        {
            if (isFinished) return;

            // Set finished state immediately
            isFinished = true;
            
            // Stop all movement
            forwardSpeed = 0;
            
            // Freeze player in place - disable controller if it exists
            if (controller != null)
            {
                // Save current position
                Vector3 finalPosition = transform.position;
                
                // Temporarily disable controller to stop physics interactions
                controller.enabled = false;
                
                // Set exact position (to prevent gravity/sliding)
                transform.position = finalPosition;
            }

            // Play finish animation
            if (animator != null)
            {
                animator.SetTrigger("Victory");
            }

            // Notify game manager
            EndlessRunnerManager.Instance.OnPlayerReachFinish();
        }

        // Apply power-up
        private void ApplyPowerUp(PowerUpType type)
        {
            // Track previous power-up type
            PowerUpType? previousType = null;
            
            // Cancel any running power-up
            if (powerUpCoroutine != null)
            {
                // Store the previous power-up type before canceling
                previousType = type;
                
                // Stop the current power-up coroutine
                StopCoroutine(powerUpCoroutine);
                powerUpCoroutine = null;
                
                // If we had invincibility running and we're switching to a different power-up,
                // ensure invincibility is properly turned off
                if (isInvincible && type != PowerUpType.Invincibility)
                {
                    isInvincible = false;
                    
                    // Hide shield VFX
                    if (shieldVFX != null)
                    {
                        shieldVFX.SetActive(false);
                    }
                    
                    Debug.Log("Invincibility canceled by new power-up");
                    
                    // Notify about invincibility end
                    OnInvincibilityChanged?.Invoke(false, 0, EndlessRunnerManager.Instance.PowerUpDuration);
                }
            }

            // Apply new power-up
            switch (type)
            {
                case PowerUpType.Invincibility:
                    powerUpCoroutine = StartCoroutine(InvincibilityCoroutine());
                    break;
                case PowerUpType.SpeedBoost:
                    // Notify game manager about speed boost
                    EndlessRunnerManager.Instance.ActivateSpeedBoost();
                    break;
                case PowerUpType.ExtraLife:
                    // Add an extra life, up to the maximum
                    if (lives < maxLives)
                    {
                        lives++;
                        OnLivesChanged?.Invoke(lives);
                    }
                    break;
            }

            // Trigger power-up event
            OnPowerUpCollected?.Invoke(type);
        }

        // Invincibility power-up coroutine
        private IEnumerator InvincibilityCoroutine()
        {
            // Enable invincibility
            isInvincible = true;

            // Show shield VFX
            if (shieldVFX != null)
            {
                shieldVFX.SetActive(true);
            }

            // Wait for power-up duration
            float duration = EndlessRunnerManager.Instance.PowerUpDuration;

            // Notify about invincibility change
            OnInvincibilityChanged?.Invoke(true, duration, duration);

            // Count down remaining time
            float remainingTime = duration;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                OnInvincibilityChanged?.Invoke(true, remainingTime, duration);
                yield return null;
            }

            // Disable invincibility
            isInvincible = false;

            // Hide shield VFX
            if (shieldVFX != null)
            {
                shieldVFX.SetActive(false);
            }

            // Notify about invincibility end
            OnInvincibilityChanged?.Invoke(false, 0, duration);

            powerUpCoroutine = null;
        }

        // Reset player
        public void ResetPlayer()
        {
            isFinished = false;
            isJumping = false;
            isBouncing = false;
            isDead = false;
            canMove = true;
            targetLane = 1;
            currentLanePosition = 0f;

            // Reset lives
            lives = maxLives;
            if (OnLivesChanged != null) OnLivesChanged.Invoke(lives);

            // Reset position
            transform.position = originalPosition;

            // Reset controller height
            controller.height = originalHeight;
            controller.center = new Vector3(0, originalCenterY, 0);

            // Reset character model rotation if available
            if (characterModel != null)
            {
                characterModel.transform.rotation = Quaternion.identity;
            }
            else if (modelTransform != null)
            {
                modelTransform.rotation = Quaternion.identity;
            }

            // Reset animations
            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsJumping", false);
            }

            // Turn off shield effect
            if (shieldVFX != null)
            {
                shieldVFX.SetActive(false);
            }

            // Reset invincibility
            isInvincible = false;
        }
        
        // Also add a manual jump method for testing
        public void Jump()
        {
            OnJumpInput();
        }

        // Add a public method for EndlessRunnerManager to call
        public void TriggerDeath()
        {
            if (!isDead)
            {
                Die();
            }
        }
        
        // Change back to public so EndlessRunnerManager can access it
        public void Die()
        {
            // If already dead, don't process again
            if (isDead) return;
            
            // Set death flag
            isDead = true;
            
            // Disable movement
            canMove = false;
            
            // Set velocity to zero
            if (controller != null && controller.enabled)
            {
                // Make one last move to ensure player is on the ground
                Vector3 finalMove = Vector3.down * 0.1f;
                controller.Move(finalMove);
            }
            
            // Stop any running coroutines
            StopAllCoroutines();
            
            // Disable input actions - just call Disable() directly
            if (inputActions != null)
            {
                inputActions.Disable();
            }
            
            // Set speed to zero
            SetForwardSpeed(0);
            
            // Trigger death animation if available
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            
            // Use coroutine instead of Invoke to show game over panel with delay
            StartCoroutine(ShowGameOverPanelDelayed(0.1f));
        }
        
        // Coroutine to show game over panel with delay - replaces Invoke
        private IEnumerator ShowGameOverPanelDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowGameOverPanel();
        }
        
        // Method to show the game over panel
        private void ShowGameOverPanel()
        {
            if (EndlessRunnerManager.Instance != null)
            {
                EndlessRunnerManager.Instance.OnPlayerDeath();
            }
        }

        // Further enhanced OnDestroy for better memory cleanup
        private void OnDestroy()
        {
            // Stop all coroutines to prevent memory leaks
            StopAllCoroutines();
            
            // Unsubscribe from all events to prevent memory leaks
            OnLivesChanged = null;
            OnPowerUpCollected = null;
            OnHealthChanged = null;
            OnInvincibilityChanged = null;
            
            // Unsubscribe from all input bindings
            if (inputBindings != null)
            {
                inputBindings.Dispose();
                inputBindings = null;
            }
            
            // Disable and null input actions
            if (inputActions != null)
            {
                inputActions.Disable();
                inputActions = null;
            }
            
            // Make sure shield VFX is disabled
            if (shieldVFX != null)
            {
                shieldVFX.SetActive(false);
            }
            
            // Clean up audio
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            // Force GC collection to clean up any lingering allocations
            GC.Collect();
        }

        // Add a properly implemented TakeHit method since it's referenced in Obstacle.cs
        public void TakeHit()
        {
            if (isInvincible || isDead || isFinished || isBouncing) return;

            HandleObstacleHit();
        }

        // Add method to handle animation events
        public void PlaySound(string soundName)
        {
            // This receiver will catch the PlaySound animation event
            // You can add actual sound playing logic here if needed
            // For now this empty method will prevent the error
        }

        private void UpdateRunningSound()
        {
            if (audioSource == null || runningSound == null || !enableRunningSound) return;
            
            // Should be playing running sound when:
            // 1. Player is grounded (not jumping)
            // 2. Player is moving (speed > 0)
            // 3. Player is not dead or finished
            // 4. Player is not bouncing back from an obstacle
            bool shouldPlayRunningSound = isGrounded && forwardSpeed > 0 && !isDead && !isFinished && !isBouncing;
            
            if (shouldPlayRunningSound && !audioSource.isPlaying)
            {
                audioSource.pitch = runningSoundPitch;
                audioSource.Play();
            }
            else if (!shouldPlayRunningSound && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        public void SetRunningSoundVolume(float volume)
        {
            runningSoundVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
            {
                audioSource.volume = runningSoundVolume;
            }
        }

        public void SetRunningSoundPitch(float pitch)
        {
            // Clamp pitch between 0.5 (half speed) and 2.0 (double speed)
            runningSoundPitch = Mathf.Clamp(pitch, 0.5f, 2f);
            if (audioSource != null)
            {
                audioSource.pitch = runningSoundPitch;
            }
        }

        public void EnableRunningSound(bool enable)
        {
            enableRunningSound = enable;
            
            if (!enableRunningSound && audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            else if (enableRunningSound)
            {
                UpdateRunningSound();
            }
        }
    }
} 