using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerPositionDebugger : MonoBehaviour
{
    [SerializeField, ReadOnly] private Vector3 currentPlayerPosition;
    [SerializeField, ReadOnly] private Vector3 lastSavedPosition;
    [SerializeField, ReadOnly] private bool hasStoredPosition;
    [SerializeField, ReadOnly] private bool isInTransition;

    private Transform playerTransform;

    private void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        // Update current position
        if (playerTransform != null)
        {
            currentPlayerPosition = playerTransform.position;
        }

        // Update saved position from manager
        lastSavedPosition = PlayerPositionManager.GetLastPosition();
        hasStoredPosition = PlayerPositionManager.HasStoredPosition();
        isInTransition = PlayerPositionManager.IsTransitionInProgress();
    }
}

// Custom attribute to make fields read-only in the inspector
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif 