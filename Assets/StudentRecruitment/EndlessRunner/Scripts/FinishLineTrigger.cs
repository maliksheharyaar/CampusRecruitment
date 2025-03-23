using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class FinishLineTrigger : MonoBehaviour
    {
        [SerializeField] private ParticleSystem finishParticles;
        [SerializeField] private AudioClip finishSound;

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
                    audioSource.Play();
                }
                
                // Notify the EndlessRunnerManager
                if (EndlessRunnerManager.Instance != null)
                {
                    EndlessRunnerManager.Instance.OnPlayerReachFinish();
                }
            }
        }
    }
} 