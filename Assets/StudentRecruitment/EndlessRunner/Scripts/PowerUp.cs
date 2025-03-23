using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpType powerUpType;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float bobSpeed = 1f;
        [SerializeField] private float bobHeight = 0.5f;
        [SerializeField] private ParticleSystem collectEffect;
        [SerializeField] private float despawnDelay = 2f; // Consistent despawn delay

        private Vector3 startPosition;
        private bool collected = false;
        private Collider powerUpCollider;

        public PowerUpType Type => powerUpType;

        private void Start()
        {
            startPosition = transform.position;
            powerUpCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (collected) return;

            // Rotate around Y axis
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        public void Collect()
        {
            if (collected) return;
            collected = true;

            // Disable collider immediately to prevent multiple collections
            if (powerUpCollider != null)
            {
                powerUpCollider.enabled = false;
            }

            // Hide the model immediately
            foreach (Transform child in transform)
            {
                if (child.GetComponent<ParticleSystem>() == null)
                {
                    child.gameObject.SetActive(false);
                }
            }

            // Play particle effect if available
            if (collectEffect != null)
            {
                collectEffect.Play();
            }

            // Use consistent despawn delay
            StartCoroutine(DelayedDespawn());
        }
        
        private IEnumerator DelayedDespawn()
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(despawnDelay);
            
            // Deactivate the game object
            gameObject.SetActive(false);
        }
    }
} 