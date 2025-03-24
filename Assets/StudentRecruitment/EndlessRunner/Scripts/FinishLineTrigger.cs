using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class FinishLineTrigger : MonoBehaviour
    {
        [SerializeField] private ParticleSystem finishParticles;
        [SerializeField] private AudioClip finishSound;
        [SerializeField, Range(0f, 1f), Tooltip("Volume of the finish line sound effect")] 
        private float soundVolume = 1.0f;

        private bool hasBeenTriggered = false;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && finishSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenTriggered) return;
            
            if (other.CompareTag("Player"))
            {
                hasBeenTriggered = true;
                
                // Play effects
                if (finishParticles != null)
                {
                    finishParticles.Play();
                }
                
                if (audioSource != null && finishSound != null)
                {
                    audioSource.clip = finishSound;
                    audioSource.volume = soundVolume;
                    audioSource.Play();
                }
                
                // Notify the EndlessRunnerManager
                if (EndlessRunnerManager.Instance != null)
                {
                    EndlessRunnerManager.Instance.OnPlayerReachFinish();
                }
            }
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