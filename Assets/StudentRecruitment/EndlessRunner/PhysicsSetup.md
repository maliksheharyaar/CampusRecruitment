# Physics Setup Guide for Endless Runner

This guide provides detailed instructions for setting up the physics in your endless runner game to ensure stable gameplay mechanics.

## Player Physics Setup

1. Select your Player GameObject in the Hierarchy
2. Configure the Character Controller component:

### Character Controller Component
- **Height**: 2
- **Radius**: 0.5
- **Y Center**: 1
- **Step Offset**: 0.1 (keep very low to prevent climbing obstacles)
- **Skin Width**: 0.08 (default)
- **Min Move Distance**: 0.001
- **Slope Limit**: 45
- **Center**: (0, 1, 0)

### Ground Check
- Create an empty child GameObject named "GroundCheck"
- Position it at the bottom of the player (0, 0.1, 0)
- Assign this transform to the "Ground Check" field in RunnerController

### RunnerController Component
- **Ground Layer**: Create and assign a layer named "Ground"
- **Ground Distance**: 0.4
- **Lane Distance**: 3
- **Jump Height**: 2
- **Jump Time**: 0.5
- **Slide Height**: 0.5
- **Slide Time**: 0.5
- **Lane Change Speed**: 5

## Track and Ground Setup

1. For all track segments:
   - Add Box Colliders to all ground surfaces
   - Set them to non-trigger
   - Assign all ground objects to the "Ground" layer
   - Ensure colliders are properly sized with no gaps between segments

2. Adjust the RunnerController script to use the Ground layer for Ground Check:
   ```csharp
   [SerializeField] private LayerMask groundMask;
   
   // In Update or FixedUpdate
   isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
   ```

3. Create a Physics Material for ground objects (optional):
   - Create a new Physics Material named "TrackMaterial"
   - Set Dynamic Friction: 0.6
   - Set Static Friction: 0.6
   - Set Friction Combine: Average
   - Set Bounce Combine: Minimum
   - Apply to all ground colliders

## Obstacle Setup

1. For each obstacle:
   - Add appropriate Colliders (Box Collider for most obstacles)
   - Set them as Triggers (check "Is Trigger")
   - Tag them as "Obstacle"
   - Size them appropriately based on the type:
     - Wall obstacles: full player height
     - Jump obstacles: lower height
     - Slide obstacles: positioned higher

2. Make sure obstacle detection uses OnTriggerEnter:
   ```csharp
   private void OnTriggerEnter(Collider other)
   {
       if (other.CompareTag("Player") && !other.GetComponent<RunnerController>().IsInvincible)
       {
           // Handle collision
           other.GetComponent<RunnerController>().TakeHit();
       }
   }
   ```

## Boulder Setup

1. Select your Boulder GameObject
2. Configure components:

### Sphere Collider Component
- **Radius**: 2
- **Is Trigger**: Checked
- **Center**: (0, 0, 0)

### BoulderController Component
- **Distance Behind Player**: 15
- **Rotation Speed**: 180
- Ensure the boulder model is assigned
- Set up dust effects if applicable

3. Ensure the boulder uses trigger detection:
   ```csharp
   private void OnTriggerEnter(Collider other)
   {
       if (other.CompareTag("Player") && isChasing)
       {
           // Game over when boulder catches player
           EndlessRunnerManager.Instance.OnPlayerDeath();
       }
   }
   ```

## Movement System

Since we're using a Character Controller instead of Rigidbody physics, movement will be script-controlled:

1. For jumps, use interpolation between heights:
   ```csharp
   // In JumpCoroutine
   float jumpStartTime = Time.time;
   float jumpDuration = jumpTime / 2;
   float startHeight = transform.position.y;
   float endHeight = startHeight + jumpHeight;
   
   while (Time.time < jumpStartTime + jumpDuration)
   {
       float t = (Time.time - jumpStartTime) / jumpDuration;
       float height = Mathf.Lerp(startHeight, endHeight, t);
       
       // Apply jump height
       controller.enabled = false;
       transform.position = new Vector3(transform.position.x, height, transform.position.z);
       controller.enabled = true;
       
       yield return null;
   }
   ```

2. For lane changes, use lerping between positions:
   ```csharp
   // In HandleLaneMovement
   float targetX = (targetLane - 1) * laneDistance;
   
   // Smoothly move toward target lane
   currentLanePosition = Mathf.Lerp(currentLanePosition, targetX, Time.deltaTime * laneChangeSpeed);
   
   // Apply the horizontal movement directly to transform
   controller.enabled = false;
   transform.position = new Vector3(currentLanePosition, transform.position.y, transform.position.z);
   controller.enabled = true;
   ```

## Testing

1. Enter Play mode and verify:
   - Player stays at the correct height
   - Jumping and sliding have smooth transitions
   - Lane changes work correctly
   - Obstacle collisions are detected properly
   - The character doesn't float above the ground

2. Debug common issues:
   - If the player floats, adjust the ground check distance
   - If collisions aren't detected, check tags and trigger settings
   - If movement is jerky, adjust the lerp speeds

## Advanced Configurations

### Performance Optimization
- Use simple colliders (Box, Sphere) rather than Mesh colliders
- Disable colliders for purely visual elements
- Put obstacles and collectibles on separate layers for efficient collision checks

### Visual Feedback
- Add animations that match the physical movement:
  - Jump animation should play during the jump coroutine
  - Slide animation should play during the slide coroutine
  - Lane change should trigger a turning animation

### Additional Tips
- Temporarily visualize the ground check area using Debug.DrawSphere
- Log state changes when testing to confirm correct behavior
- Use editor gizmos to visualize lanes and obstacle positions 