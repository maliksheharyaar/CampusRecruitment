using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 3, -7);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float lookAtOffset = 2f; // Look ahead of player
        
        [Header("Death Camera")]
        [SerializeField] private Vector3 deathCameraOffset = new Vector3(0, 3, -5);
        [SerializeField] private float deathCameraHeight = 2f;
        
        private Vector3 velocity = Vector3.zero;
        private bool isPlayerDead = false;
        
        private void Start()
        {
            // If no target assigned, try to find the player
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    Debug.LogWarning("Follow Camera: No target assigned and no player found!");
                }
            }
        }
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            // Handle camera position
            Vector3 targetPosition;
            
            if (isPlayerDead)
            {
                // Position for when player is dead - show from a side angle
                targetPosition = target.position + deathCameraOffset;
                targetPosition.y += deathCameraHeight;
            }
            else
            {
                // Normal follow position
                targetPosition = target.position + offset;
            }
            
            // Smoothly move to target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1 / smoothSpeed);
            
            // Look at target (slightly ahead)
            Vector3 lookTarget = target.position;
            lookTarget.y = transform.position.y; // Keep camera level
            lookTarget.z += lookAtOffset; // Look ahead of player
            transform.LookAt(lookTarget);
        }
        
        public void SetDeathCameraMode(bool isDead)
        {
            isPlayerDead = isDead;
        }
    }
} 