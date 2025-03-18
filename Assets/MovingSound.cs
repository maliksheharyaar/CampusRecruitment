using System.Collections;
using System.Collections.Generic;
using StudentRecruitment.FinalCharacterController;
using UnityEngine;

public class MovingSound : MonoBehaviour
{
    // Add an audio source component to the game object and then a PlaySound method
    public AudioSource audioSource;

    public void PlaySound()
    {
        // audioSource.Stop();
        audioSource.Play();
    }
        public void PlaySound2()
    {
        // Find the player's input component
        var playerInput = FindObjectOfType<PlayerLocomotionInput>();
        
        // Check if X input value is between -0.5 and 0.5
        if (playerInput.MovementInput.x == 1f || playerInput.MovementInput.x == -1f)
        {
            audioSource.Play();
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
