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
        [SerializeField] private AudioClip collectSound;
        [SerializeField, Range(0f, 1f), Tooltip("Volume of the collect sound effect")] 
        private float soundVolume = 1.0f;

        private Vector3 startPosition;
        private bool collected = false;
        private Collider powerUpCollider;
        private AudioSource audioSource;

        public PowerUpType Type => powerUpType;

        private void Start()
        {
            startPosition = transform.position;
            powerUpCollider = GetComponent<Collider>();
            
            // Get or add AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && collectSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 20.0f;
            }
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

            // Play sound effect if available but detach it so it continues playing
            if (collectSound != null)
            {
                PlayDetachedSound(collectSound, transform.position, soundVolume);
            }

            // Destroy the object immediately
            Destroy(gameObject);
        }
        
        // Play a sound effect at a position, detached from the original object
        private void PlayDetachedSound(AudioClip clip, Vector3 position, float volume)
        {
            // Create a temporary game object to play the sound
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = position;
            
            // Add an audio source component and configure it
            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 1.0f; // 3D sound
            source.Play();
            
            // Destroy the temporary object after the sound is done playing
            Destroy(tempAudio, clip.length + 0.1f);
        }
        
        // Public method to adjust volume at runtime
        public void SetSoundVolume(float volume)
        {
            soundVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
            {
                audioSource.volume = soundVolume;
            }
        }
    }
} 