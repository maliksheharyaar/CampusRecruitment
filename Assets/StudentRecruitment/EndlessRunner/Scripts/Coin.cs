using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private int coinValue = 1;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private float despawnDelay = 2f; // Delay before despawning after collection
        
        private bool isCollected = false;
        private Collider coinCollider;
        private Renderer coinRenderer;

        // Property to access the coin value
        public int CoinValue => coinValue;
        
        private void Awake()
        {
            // Cache components
            coinCollider = GetComponent<Collider>();
            coinRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            // Only rotate if not collected
            if (!isCollected)
            {
                // Rotate the coin
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
        }

        // Method to call when the coin is collected
        public void Collect()
        {
            // Skip if already collected
            if (isCollected) return;
            
            isCollected = true;
            
            // Play collection effect if assigned
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            // Play sound if assigned
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            // Disable the collider immediately to prevent multiple collections
            if (coinCollider != null)
            {
                coinCollider.enabled = false;
            }
            
            // Make the coin invisible but keep it active for the delay period
            if (coinRenderer != null)
            {
                coinRenderer.enabled = false;
            }
            
            // Start delayed despawn
            StartCoroutine(DelayedDespawn());
        }
        
        private IEnumerator DelayedDespawn()
        {
            // Wait for the specified delay period
            yield return new WaitForSeconds(despawnDelay);
            
            // Deactivate the object
            gameObject.SetActive(false);
            
            // Optional: Destroy the object completely if needed
            // Destroy(gameObject);
        }
    }
} 