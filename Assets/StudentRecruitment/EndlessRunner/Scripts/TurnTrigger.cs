using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public class TurnTrigger : MonoBehaviour
    {
        [SerializeField] private bool isLeftTurn = true;
        [SerializeField] private float turnAngle = 90f; // Default to 90 degrees

        // Visual indicator for debugging in editor
        [SerializeField] private bool showDebugVisuals = true;
        [SerializeField] private Color leftTurnColor = Color.blue;
        [SerializeField] private Color rightTurnColor = Color.red;
        [SerializeField] private float debugArrowLength = 2f;
        
        // Reference to the camera controller
        private RunnerCameraController cameraController;

        // Constructor and initialization
        private void Awake()
        {
            // Create visual indicators to help debug turn direction
            //CreateRuntimeVisualIndicator();
            
            // Find the camera controller in the scene
            cameraController = FindObjectOfType<RunnerCameraController>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if this is the player entering the turn trigger
            RunnerController player = other.GetComponent<RunnerController>();
            
            if (player != null)
            {
                Debug.Log($"Player entered turn trigger: {gameObject.name}");
                
                // Disable player side-to-side movement when entering turn
                player.ForceMiddleLane();
                
                // Freeze the camera when the actual turn happens
                if (cameraController != null)
                {
                    cameraController.FreezeCamera();
                }
            }
        }

        // Add OnTriggerExit to start camera reattachment when player exits the turn
        private void OnTriggerExit(Collider other)
        {
            // Check if this is the player exiting the turn trigger
            RunnerController player = other.GetComponent<RunnerController>();
            
            if (player != null)
            {
                Debug.Log($"Player exited turn trigger: {gameObject.name}");
                
                // Start camera reattachment after the turn is complete
                if (cameraController != null)
                {
                    cameraController.StartReattach();
                }
            }
        }

        // Create visual indicators for debugging turn direction
        private void CreateRuntimeVisualIndicator()
        {
            // Create a visual indicator GameObject
            GameObject indicator = new GameObject("TurnDirectionIndicator");
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 3f, 0); // Position above the turn trigger

            // Add text label
            TextMesh textMesh = indicator.AddComponent<TextMesh>();
            textMesh.text = isLeftTurn ? "<<< LEFT TURN" : "RIGHT TURN >>>";
            textMesh.color = isLeftTurn ? leftTurnColor : rightTurnColor;
            textMesh.fontSize = 48;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            
            // Make it face the camera
            indicator.AddComponent<Billboard>();
        }

        // Draw debug visualization in the editor
        private void OnDrawGizmos()
        {
            if (showDebugVisuals)
            {
                // Set color based on turn direction
                Gizmos.color = isLeftTurn ? leftTurnColor : rightTurnColor;
                
                // Draw a box showing the trigger area
                Gizmos.DrawWireCube(transform.position, new Vector3(4f, 3f, 2f));
                
                // Draw turn direction arrow
                Vector3 startPos = transform.position + new Vector3(0, 1.5f, 0);
                Vector3 worldUp = Vector3.up;
                
                // Current forward direction
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                
                // Draw the forward direction
                Gizmos.color = Color.green;
                Gizmos.DrawRay(startPos, forward * debugArrowLength);
                
                // Draw the turn direction
                Gizmos.color = isLeftTurn ? leftTurnColor : rightTurnColor;
                
                if (isLeftTurn)
                {
                    // Left turn - draw arrow pointing left
                    Gizmos.DrawRay(startPos, -right * debugArrowLength);
                    
                    // Draw turn path - arc from forward to left
                    DrawTurnArc(startPos, forward, -right, 10, debugArrowLength, isLeftTurn);
                }
                else
                {
                    // Right turn - draw arrow pointing right
                    Gizmos.DrawRay(startPos, right * debugArrowLength);
                    
                    // Draw turn path - arc from forward to right
                    DrawTurnArc(startPos, forward, right, 10, debugArrowLength, isLeftTurn);
                }
                
                // Draw text label
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(startPos + Vector3.up * 1f, 
                    isLeftTurn ? "LEFT TURN (-90°)" : "RIGHT TURN (+90°)", 
                    new GUIStyle() { 
                        normal = { textColor = isLeftTurn ? leftTurnColor : rightTurnColor }, 
                        fontSize = 20, 
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    });
                #endif
            }
        }
        
        // Helper method to draw a turn arc in the editor
        private void DrawTurnArc(Vector3 center, Vector3 startDir, Vector3 endDir, int segments, float radius, bool isLeftTurn)
        {
            float angle = isLeftTurn ? -90f : 90f;
            
            for (int i = 0; i < segments; i++)
            {
                float startAngle = (i / (float)segments) * angle;
                float endAngle = ((i + 1) / (float)segments) * angle;
                
                Vector3 startPoint = Quaternion.Euler(0, startAngle, 0) * startDir * radius + center;
                Vector3 endPoint = Quaternion.Euler(0, endAngle, 0) * startDir * radius + center;
                
                Gizmos.DrawLine(startPoint, endPoint);
            }
        }

        // Public access methods
        public bool IsLeftTurn()
        {
            return isLeftTurn;
        }

        public float GetTurnAngle()
        {
            return isLeftTurn ? -turnAngle : turnAngle;
        }
        
        // Method to get exit direction based on current forward direction
        public Vector3 GetExitDirection()
        {
            // For simplicity, we just return a direction 90 degrees from forward
            float angle = isLeftTurn ? -90f : 90f;
            return Quaternion.Euler(0, angle, 0) * transform.forward;
        }
    }

    // Billboard component to make text face camera
    public class Billboard : MonoBehaviour 
    {
        void Update() 
        {
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                // Keep it upright
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            }
        }
    }
} 