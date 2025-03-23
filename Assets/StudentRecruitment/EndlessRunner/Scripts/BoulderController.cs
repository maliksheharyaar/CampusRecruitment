using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    [RequireComponent(typeof(SphereCollider))]
    public class BoulderController : MonoBehaviour
    {
        [Header("Boulder Settings")]
        [SerializeField] private float distanceBehindPlayer = 15f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private Transform boulderModel;
        [SerializeField] private float chaseSpeed = 8f; // Speed at which boulder chases player
        [SerializeField] private float maxSpeed = 15f; // Maximum boulder speed
        [SerializeField] private float acceleration = 0.1f; // How quickly boulder accelerates

        [Header("Effects")]
        [SerializeField] private ParticleSystem dustEffect;
        [SerializeField] private AudioClip rollingSound;

        // Components
        private SphereCollider boulderCollider;
        private AudioSource audioSource;

        // State
        private bool isChasing = false;
        private Transform playerTransform;
        private float currentSpeed;

        private void Awake()
        {
            // Get components
            boulderCollider = GetComponent<SphereCollider>();
            audioSource = GetComponent<AudioSource>();

            // Set up audio source if not present
            if (audioSource == null && rollingSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = rollingSound;
                audioSource.loop = true;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.volume = 0.7f;
            }

            // Find player
            playerTransform = FindObjectOfType<RunnerController>()?.transform;

            // Set initial position
            if (playerTransform != null)
            {
                Vector3 startPos = playerTransform.position - Vector3.forward * distanceBehindPlayer;
                startPos.y = transform.position.y; // Keep the original y position
                transform.position = startPos;
            }

            // Initialize speed
            currentSpeed = chaseSpeed;
        }

        // Method to start boulder chasing
        public void StartChasing()
        {
            isChasing = true;
            currentSpeed = chaseSpeed;

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
        }

        // Method to stop boulder chasing
        public void StopChasing()
        {
            isChasing = false;

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
            if (!isChasing || playerTransform == null) return;

            // Get player reference
            RunnerController playerController = playerTransform.GetComponent<RunnerController>();
            
            // Update speed to follow player but stay behind
            if (playerController != null && !playerController.isFinished)
            {
                // Calculate desired position (behind the player)
                Vector3 targetPosition = playerTransform.position - Vector3.forward * distanceBehindPlayer;
                targetPosition.y = transform.position.y; // Keep the same height
                targetPosition.x = 0; // Stay in the center lane
                
                // Calculate distance to target position
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                
                // Adjust speed based on distance
                if (distanceToTarget > distanceBehindPlayer * 1.2f)
                {
                    // Boulder is too far behind, speed up
                    currentSpeed = Mathf.Min(currentSpeed + acceleration * 2 * Time.deltaTime, maxSpeed);
                }
                else if (distanceToTarget < distanceBehindPlayer * 0.8f)
                {
                    // Boulder is too close, slow down
                    currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, chaseSpeed * 0.5f);
                }
                else
                {
                    // Maintain a steady pace
                    currentSpeed = Mathf.Lerp(currentSpeed, chaseSpeed, Time.deltaTime);
                }
                
                // Move boulder toward target position
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    targetPosition, 
                    currentSpeed * Time.deltaTime
                );
            }
            
            // Rotate boulder to simulate rolling
            if (boulderModel != null)
            {
                boulderModel.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // If we hit the player, it's game over
            if (other.CompareTag("Player") && isChasing)
            {
                RunnerController runner = other.GetComponent<RunnerController>();
                if (runner != null)
                {
                    // Kill the player by reducing their lives to zero
                    EndlessRunnerManager.Instance.OnPlayerDeath();
                }
            }
        }
    }
} 