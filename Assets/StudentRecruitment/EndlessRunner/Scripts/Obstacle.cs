using UnityEngine;
using System.Collections;

namespace StudentRecruitment.EndlessRunner
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private GameObject breakEffect;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private bool useBoxCollider = true;
        [SerializeField] private float colliderDisableTime = 1.5f; // Time to disable collider after hit
        
        private Collider obstacleCollider;
        private bool isDisabled = false;
        
        private void Start()
        {
            // Ensure the collider is properly set up
            if (useBoxCollider)
            {
                obstacleCollider = GetComponent<BoxCollider>();
                if (obstacleCollider == null)
                {
                    obstacleCollider = gameObject.AddComponent<BoxCollider>();
                    // Set default size if this is a new collider
                    BoxCollider boxCollider = obstacleCollider as BoxCollider;
                    boxCollider.size = new Vector3(1f, 1f, 1f);
                    boxCollider.center = new Vector3(0f, 0.5f, 0f);
                }
                
                // Make sure it's a trigger for OnTriggerEnter to work
                obstacleCollider.isTrigger = true;
            }
            else
            {
                // Get any existing collider
                obstacleCollider = GetComponent<Collider>();
            }
            
            // Ensure obstacle has the correct tag
            if (!CompareTag("Obstacle"))
            {
                gameObject.tag = "Obstacle";
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isDisabled) return; // Skip if already disabled
            
            if (other.CompareTag("Player"))
            {
                // Get the RunnerController
                RunnerController playerController = other.GetComponent<RunnerController>();
                if (playerController != null)
                {
                    // Check if player is invincible
                    if (!playerController.IsInvincible)
                    {
                        // Apply damage to player using TakeHit (compatibility method)
                        playerController.TakeHit();
                        
                        // Play collision effects
                        if (hitEffect != null)
                        {
                            Instantiate(hitEffect, transform.position, Quaternion.identity);
                        }
                        
                        // Play sound
                        if (hitSound != null)
                        {
                            AudioSource.PlayClipAtPoint(hitSound, transform.position);
                        }
                        
                        // Temporarily disable collider
                        StartCoroutine(TemporarilyDisableCollider());
                    }
                    else
                    {
                        // Player is invincible, break the obstacle
                        BreakObstacle();
                    }
                }
            }
        }
        
        private IEnumerator TemporarilyDisableCollider()
        {
            // Set the flag
            isDisabled = true;
            
            // Disable the collider
            if (obstacleCollider != null)
            {
                obstacleCollider.enabled = false;
                
                // Visual feedback that collision is disabled - slightly fade out
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color originalColor = renderer.material.color;
                    Color fadedColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
                    renderer.material.color = fadedColor;
                }
                
                // Wait for the disable time
                yield return new WaitForSeconds(colliderDisableTime);
                
                // Re-enable the collider if the object is still active
                if (gameObject.activeInHierarchy)
                {
                    obstacleCollider.enabled = true;
                    
                    // Restore original color
                    if (renderer != null)
                    {
                        Color originalColor = renderer.material.color;
                        renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                    }
                }
            }
            
            // Reset the flag
            isDisabled = false;
        }
        
        private void BreakObstacle()
        {
            // Play break effect if assigned
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }
            
            // Play sound
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySound("ObstacleBreak");
            }
            
            // Deactivate the obstacle
            gameObject.SetActive(false);
        }
    }
} 