using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class RunnerCameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
        
        [Header("Follow Settings")]
        [SerializeField] private float positionSmoothTime = 0.2f;
        [SerializeField] private float rotationSmoothTime = 0.3f;
        [SerializeField] private float lookAheadDistance = 3f;
        
        [Header("Turn Settings")]
        [SerializeField] private float reattachDuration = 1.0f; // How long to reattach after turn
        
        // Internal state
        private Vector3 currentVelocity;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        
        // Camera freeze state
        private bool isFrozen = false;
        private Vector3 frozenPosition;
        private Quaternion frozenRotation;
        private float reattachStartTime;
        private float reattachProgress = 0f;
        
        private void Start()
        {
            // Find player if target is not assigned
            if (target == null)
            {
                RunnerController runner = FindObjectOfType<RunnerController>();
                if (runner != null)
                {
                    target = runner.transform;
                    Debug.Log("Camera target automatically assigned to RunnerController");
                }
                else
                {
                    Debug.LogError("No target assigned to RunnerCameraController and no RunnerController found in scene");
                }
            }
            
            // Initialize camera position
            if (target != null)
            {
                transform.position = CalculateTargetPosition();
                transform.rotation = CalculateTargetRotation();
            }
        }
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            // Calculate desired target position and rotation
            targetPosition = CalculateTargetPosition();
            targetRotation = CalculateTargetRotation();
            
            if (isFrozen)
            {
                // Camera is frozen, check if reattach has started
                if (reattachProgress > 0f)
                {
                    // Reattaching in progress
                    reattachProgress = Mathf.Min(1.0f, (Time.time - reattachStartTime) / reattachDuration);
                    
                    // Smoothly interpolate from frozen position/rotation to target
                    transform.position = Vector3.Lerp(frozenPosition, targetPosition, reattachProgress);
                    transform.rotation = Quaternion.Slerp(frozenRotation, targetRotation, reattachProgress);
                    
                    // If reattach is complete, return to normal following
                    if (reattachProgress >= 1.0f)
                    {
                        isFrozen = false;
                        reattachProgress = 0f;
                        Debug.Log("Camera reattach complete");
                    }
                }
                // Otherwise stay frozen
            }
            else
            {
                // Normal camera following
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
            }
        }
        
        private Vector3 CalculateTargetPosition()
        {
            // Calculate offset relative to target's forward direction
            Vector3 relativeOffset = target.right * offset.x + 
                                   Vector3.up * offset.y + 
                                   target.forward * offset.z;
            
            // Position camera behind player with the proper offset
            Vector3 desiredPosition = target.position + relativeOffset;
            
            return desiredPosition;
        }
        
        private Quaternion CalculateTargetRotation()
        {
            // Calculate look position (slightly ahead of player)
            Vector3 lookPosition = target.position + target.forward * lookAheadDistance;
            
            // Create rotation to look at that position
            Vector3 lookDirection = lookPosition - transform.position;
            if (lookDirection != Vector3.zero)
            {
                return Quaternion.LookRotation(lookDirection);
            }
            
            return transform.rotation;
        }
        
        // Called when player enters a turn
        public void FreezeCamera()
        {
            if (isFrozen) return; // Already frozen
            
            isFrozen = true;
            frozenPosition = transform.position;
            frozenRotation = transform.rotation;
            reattachProgress = 0f;
            
            Debug.Log("Camera position frozen during turn");
        }
        
        // Called when player completes a turn
        public void StartReattach()
        {
            if (!isFrozen) return; // Not currently frozen
            
            reattachStartTime = Time.time;
            reattachProgress = 0.001f; // Small value > 0 to start reattachment
            
            Debug.Log("Camera reattachment started");
        }
    }
} 