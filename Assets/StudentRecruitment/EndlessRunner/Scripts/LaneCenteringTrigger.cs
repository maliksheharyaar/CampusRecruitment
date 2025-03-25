using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class LaneCenteringTrigger : MonoBehaviour
    {
        [SerializeField] private bool showDebugVisuals = true;
        [SerializeField] private Color debugColor = Color.yellow;
        [SerializeField] private float debugArrowLength = 2f;
        
        // Reference to the associated turn trigger to get its direction
        private TurnTrigger associatedTurnTrigger;
        // Reference to the camera controller
        private RunnerCameraController cameraController;

        private void Awake()
        {
            // Create visual indicators to help debug
            if (showDebugVisuals)
            {
                //CreateRuntimeVisualIndicator();
            }
            
            // Find the associated turn trigger in the same parent object
            FindAssociatedTurnTrigger();
            
            // Find the camera controller in the scene
            cameraController = FindObjectOfType<RunnerCameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("Could not find RunnerCameraController in scene for lane centering trigger.");
            }
        }
        
        private void FindAssociatedTurnTrigger()
        {
            // Find turn trigger in siblings (we're both children of the same turn segment)
            if (transform.parent != null)
            {
                // Look for a TurnTrigger component in other children of our parent
                foreach (Transform child in transform.parent)
                {
                    if (child == transform) continue; // Skip ourselves
                    
                    // See if this child has a TurnTrigger component
                    TurnTrigger turnTrigger = child.GetComponentInChildren<TurnTrigger>();
                    if (turnTrigger != null)
                    {
                        associatedTurnTrigger = turnTrigger;
                        Debug.Log($"LaneCenteringTrigger found associated TurnTrigger: {turnTrigger.gameObject.name}");
                        return;
                    }
                }
            }
            
            Debug.LogWarning("LaneCenteringTrigger could not find associated TurnTrigger in parent transform.");
        }

        private void CreateRuntimeVisualIndicator()
        {
            // Create a visual indicator GameObject
            GameObject indicator = new GameObject("CenteringIndicator");
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 3f, 0); // Position above the trigger

            // Add text label
            TextMesh textMesh = indicator.AddComponent<TextMesh>();
            textMesh.text = "CENTER LANE";
            textMesh.color = debugColor;
            textMesh.fontSize = 32;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            
            // Make it face the camera
            indicator.AddComponent<Billboard>();
        }

        private void OnDrawGizmos()
        {
            if (showDebugVisuals)
            {
                // Set color for centering trigger
                Gizmos.color = debugColor;
                
                // Draw a box showing the trigger area
                Collider collider = GetComponent<Collider>();
                if (collider != null && collider is BoxCollider boxCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                }
                else
                {
                    Gizmos.DrawWireCube(transform.position, new Vector3(10f, 5f, 8f));
                }
                
                // Draw arrows pointing to center
                Vector3 startPos = transform.position + new Vector3(0, 1.5f, 0);
                
                // Draw arrows pointing to center from each lane
                Gizmos.DrawLine(startPos + new Vector3(-3f, 0, 0), startPos);
                Gizmos.DrawLine(startPos + new Vector3(3f, 0, 0), startPos);
                
                // Draw an "X" at the center
                float size = 1f;
                Gizmos.DrawLine(startPos + new Vector3(-size, 0, -size), startPos + new Vector3(size, 0, size));
                Gizmos.DrawLine(startPos + new Vector3(-size, 0, size), startPos + new Vector3(size, 0, -size));
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(startPos + Vector3.up * 1f, 
                    "CENTER LANE TRIGGER", 
                    new GUIStyle() { 
                        normal = { textColor = debugColor }, 
                        fontSize = 16, 
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    });
                #endif
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if this is the player
            RunnerController player = other.GetComponent<RunnerController>();
            
            if (player != null)
            {
                Debug.Log("Lane centering trigger activated - forcing player to center lane");
                
                // Force player to middle lane
                player.ForceMiddleLane();
                
                // No camera pre-rotation anymore, camera will freeze during the actual turn
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            // Check if this is the player
            RunnerController player = other.GetComponent<RunnerController>();
            
            if (player != null)
            {
                Debug.Log("Player exited lane centering zone");
                
                // If the player exits the lane centering zone without triggering a turn,
                // we need to re-enable lane movement
                player.EnableLaneMovement();
                
                // No need to end camera pre-turn here, as the turn trigger will handle that
                // when the actual turn happens.
            }
        }
    }
} 