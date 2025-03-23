using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class CoinPickup : MonoBehaviour
    {
        [SerializeField] private int coinValue = 1;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private GameObject visualEffect;
        [SerializeField] private string pickupSoundName = "CoinPickup";
        
        private bool collected = false;
        
        private void Update()
        {
            // Rotate the coin
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (collected) return;
            
            if (other.CompareTag("Player"))
            {
                CollectCoin();
            }
        }
        
        private void CollectCoin()
        {
            collected = true;
            
            // Add to player's coin count
            GameProgress.AddCoins(coinValue);
            
            // Play sound effect
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySound(pickupSoundName);
            }
            
            // Show visual effect if assigned
            if (visualEffect != null)
            {
                Instantiate(visualEffect, transform.position, Quaternion.identity);
            }
            
            // Hide the coin
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            
            // Destroy after a short delay (to allow sound to play)
            Destroy(gameObject, 1f);
        }
    }
} 