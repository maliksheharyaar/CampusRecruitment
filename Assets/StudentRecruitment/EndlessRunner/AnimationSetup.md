# Animation Setup Guide for Endless Runner

This guide will help you implement professional animations for your endless runner game, enhancing visual appeal and player experience.

## Character Animation Setup

1. Import Character Model and Animations:
   - Place your character model in `Assets/StudentRecruitment/EndlessRunner/Models/Characters/`
   - Import animation files (FBX or other formats) to `Assets/StudentRecruitment/EndlessRunner/Animations/Character/`
   - Required animations for base functionality:
     - Idle
     - Run
     - Jump
     - Slide
     - Hit/Damage
     - Death
   - Optional animations for polish:
     - Turn Left/Right (for lane changes)
     - Celebration (for reaching finish line)
     - Power-up activation poses

2. Configure Animation Import Settings:
   - Select each animation file in the Project view
   - In the Inspector:
     - Rig tab: Set Animation Type to "Humanoid" (if using humanoid character)
     - Animation tab:
       - Set Loop Time for continuous animations (Run, Idle)
       - Uncheck Loop Time for one-shot animations (Jump, Death)
       - Set appropriate Root Transform options

3. Create Animator Controller:
   - Right-click in Project view > Create > Animator Controller
   - Name it "PlayerAnimatorController"
   - Place it in `Assets/StudentRecruitment/EndlessRunner/Animations/`

4. Set Up Animation State Machine:
   - Double-click the controller to open the Animator window
   - Create the following states by right-clicking and selecting "Create State":
     - Idle
     - Run
     - Jump
     - Slide
     - Hit
     - Death
   - Set Run as the default state

5. Assign Animation Clips:
   - Select each state and assign the corresponding animation clip in the Inspector
   - Adjust transition times and settings for smooth blending

6. Create Transition Parameters:
   - In the Parameters tab, create the following parameters:
     - isRunning (bool)
     - isJumping (bool)
     - isSliding (bool)
     - isHit (trigger)
     - isDead (bool)
     - turnDirection (float) - for lane changes (-1 for left, 1 for right)

7. Set Up State Transitions:
   - Create transitions between states:
     - Idle → Run: When isRunning = true
     - Run → Jump: When isJumping = true
     - Run → Slide: When isSliding = true
     - Run → Hit: When isHit is triggered
     - Any State → Death: When isDead = true
     - Jump → Run: When isJumping = false
     - Slide → Run: When isSliding = false
     - Hit → Run: When exit time is reached

8. Configure Transition Settings:
   - Select each transition and in the Inspector:
     - Set Has Exit Time and Exit Time for appropriate transitions
     - Set Transition Duration (typically 0.1-0.3 seconds)
     - Configure transition interruptions if needed

9. Assign Animator Controller to Character:
   - Select your player GameObject in the Hierarchy
   - In the Inspector, add an Animator component if it doesn't exist
   - Drag the PlayerAnimatorController to the Controller field

## Animation Script Integration

1. Update RunnerController Script:
   ```csharp
   using UnityEngine;
   
   [RequireComponent(typeof(Animator))]
   public class RunnerController : MonoBehaviour
   {
       private Animator animator;
       
       // Animation parameter names (matching those in the Animator)
       private const string IS_RUNNING = "isRunning";
       private const string IS_JUMPING = "isJumping";
       private const string IS_SLIDING = "isSliding";
       private const string IS_HIT = "isHit";
       private const string IS_DEAD = "isDead";
       private const string TURN_DIRECTION = "turnDirection";
       
       private void Awake()
       {
           animator = GetComponent<Animator>();
       }
       
       private void Start()
       {
           // Start running animation when game begins
           animator.SetBool(IS_RUNNING, true);
       }
       
       // Call when player jumps
       private void Jump()
       {
           // Jump logic
           animator.SetBool(IS_JUMPING, true);
       }
       
       // Call when jump ends
       private void EndJump()
       {
           animator.SetBool(IS_JUMPING, false);
       }
       
       // Call when player slides
       private void Slide()
       {
           // Slide logic
           animator.SetBool(IS_SLIDING, true);
       }
       
       // Call when slide ends
       private void EndSlide()
       {
           animator.SetBool(IS_SLIDING, false);
       }
       
       // Call when changing lanes
       private void ChangeLane(int direction)
       {
           // Lane change logic
           animator.SetFloat(TURN_DIRECTION, direction);
       }
       
       // Call when player gets hit
       public void TakeDamage()
       {
           // Damage logic
           animator.SetTrigger(IS_HIT);
       }
       
       // Call when player dies
       public void Die()
       {
           // Death logic
           animator.SetBool(IS_RUNNING, false);
           animator.SetBool(IS_DEAD, true);
       }
       
       // Called when game is completed
       public void ReachFinishLine()
       {
           // Victory logic
           animator.SetBool(IS_RUNNING, false);
           animator.SetTrigger("celebrate"); // Optional celebration animation
       }
   }
   ```

## Obstacle and Environment Animations

1. Create Animation Clips for Obstacles:
   - Right-click in Project view > Create > Animation
   - Name appropriately (e.g., "ObstacleRotation", "ObstacleBobbing")
   - Place in `Assets/StudentRecruitment/EndlessRunner/Animations/Obstacles/`

2. Set Up Obstacle Animations:
   - Select an obstacle GameObject
   - Open Animation window (Window > Animation > Animation)
   - Create animation clips for different obstacle behaviors:
     - Spinning objects
     - Moving platforms
     - Swinging pendulums
     - Pulsing barriers

3. Create Simple Animation Script for Generic Obstacles:
   ```csharp
   using UnityEngine;
   
   public class ObstacleAnimator : MonoBehaviour
   {
       [Header("Movement Animation")]
       [SerializeField] private bool enableMovement = false;
       [SerializeField] private Vector3 movementDirection = Vector3.up;
       [SerializeField] private float movementAmount = 1f;
       [SerializeField] private float movementSpeed = 1f;
       
       [Header("Rotation Animation")]
       [SerializeField] private bool enableRotation = false;
       [SerializeField] private Vector3 rotationAxis = Vector3.up;
       [SerializeField] private float rotationSpeed = 90f;
       
       [Header("Scale Animation")]
       [SerializeField] private bool enablePulsing = false;
       [SerializeField] private float pulseAmount = 0.2f;
       [SerializeField] private float pulseSpeed = 1f;
       
       private Vector3 startPosition;
       private Vector3 originalScale;
       
       private void Start()
       {
           startPosition = transform.position;
           originalScale = transform.localScale;
       }
       
       private void Update()
       {
           if (enableMovement)
           {
               float movement = Mathf.Sin(Time.time * movementSpeed) * movementAmount;
               transform.position = startPosition + movementDirection.normalized * movement;
           }
           
           if (enableRotation)
           {
               transform.Rotate(rotationAxis.normalized * (rotationSpeed * Time.deltaTime));
           }
           
           if (enablePulsing)
           {
               float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
               transform.localScale = originalScale * pulse;
           }
       }
   }
   ```

## UI Animation Setup

1. Create UI Animation Controllers:
   - Create separate Animator Controllers for major UI elements:
     - ScorePopup
     - PowerUpIcon
     - GameOverPanel
     - VictoryPanel

2. Configure UI Animations:
   - Use the Animation window to create animations for:
     - Panel transitions (fade in/out, slide in/out)
     - Button hover/click effects
     - Score counters (counting up)
     - Power-up icons (pulsing, glowing)

3. Score Popup Animation:
   ```csharp
   using UnityEngine;
   using TMPro;
   
   public class ScorePopup : MonoBehaviour
   {
       [SerializeField] private float moveSpeed = 50f;
       [SerializeField] private float fadeSpeed = 1f;
       [SerializeField] private float scaleSpeed = 1f;
       [SerializeField] private TextMeshProUGUI scoreText;
       
       private RectTransform rectTransform;
       private Color textColor;
       private float lifeTime = 0f;
       
       private void Awake()
       {
           rectTransform = GetComponent<RectTransform>();
           textColor = scoreText.color;
       }
       
       public void Initialize(int score, Transform startPosition)
       {
           scoreText.text = "+" + score.ToString();
           lifeTime = 0f;
       }
       
       private void Update()
       {
           lifeTime += Time.deltaTime;
           
           // Move upward
           rectTransform.anchoredPosition += Vector2.up * (moveSpeed * Time.deltaTime);
           
           // Fade out
           textColor.a = Mathf.Lerp(1f, 0f, lifeTime * fadeSpeed);
           scoreText.color = textColor;
           
           // Scale up slightly
           float scale = Mathf.Lerp(1f, 1.5f, lifeTime * scaleSpeed);
           rectTransform.localScale = new Vector3(scale, scale, 1f);
           
           // Destroy when fully faded
           if (textColor.a <= 0.05f)
           {
               Destroy(gameObject);
           }
       }
   }
   ```

## Particle Effects Integration

1. Create Particle Systems for Key Events:
   - Player actions: 
     - Dust when landing from jump
     - Trail when running
     - Effect when sliding
   - Collectibles:
     - Coin collection sparkle
     - Power-up activation burst

2. Trigger Particle Effects in Code:
   ```csharp
   using UnityEngine;
   
   public class EffectsManager : MonoBehaviour
   {
       [SerializeField] private ParticleSystem runningDustEffect;
       [SerializeField] private ParticleSystem jumpLandingEffect;
       [SerializeField] private ParticleSystem slideEffect;
       [SerializeField] private ParticleSystem coinCollectEffect;
       [SerializeField] private ParticleSystem[] powerUpEffects; // Array for different power-up types
       
       public void PlayRunningEffect(bool isRunning)
       {
           if (isRunning)
               runningDustEffect.Play();
           else
               runningDustEffect.Stop();
       }
       
       public void PlayJumpLandingEffect()
       {
           jumpLandingEffect.Play();
       }
       
       public void PlaySlideEffect(bool isSliding)
       {
           if (isSliding)
               slideEffect.Play();
           else
               slideEffect.Stop();
       }
       
       public void PlayCoinCollectEffect(Vector3 position)
       {
           coinCollectEffect.transform.position = position;
           coinCollectEffect.Play();
       }
       
       public void PlayPowerUpEffect(int powerUpType, Vector3 position)
       {
           if (powerUpType >= 0 && powerUpType < powerUpEffects.Length)
           {
               powerUpEffects[powerUpType].transform.position = position;
               powerUpEffects[powerUpType].Play();
           }
       }
   }
   ```

## Animation Event System

1. Set Up Animation Events:
   - Open your character's animation clips
   - Use the Events track to add events at specific frames
   - Add events for:
     - Footsteps in run animation
     - Landing impact in jump animation
     - Slide start/end
     - Hit reaction sounds

2. Create Animation Event Handler:
   ```csharp
   using UnityEngine;
   
   public class AnimationEventHandler : MonoBehaviour
   {
       [SerializeField] private RunnerController playerController;
       [SerializeField] private EffectsManager effectsManager;
       [SerializeField] private AudioManager audioManager;
       
       // Called by animation event in run animation
       public void FootstepEvent()
       {
           audioManager.PlaySFX("Footstep");
           effectsManager.PlayRunningEffect(true);
       }
       
       // Called by animation event in jump animation when landing
       public void JumpLandEvent()
       {
           audioManager.PlaySFX("Landing");
           effectsManager.PlayJumpLandingEffect();
       }
       
       // Called by animation event in slide animation
       public void SlideStartEvent()
       {
           audioManager.PlaySFX("Slide");
       }
       
       // Called by animation event in slide animation
       public void SlideEndEvent()
       {
           playerController.EndSlide();
       }
       
       // Called by animation event in hit animation
       public void HitReactionComplete()
       {
           playerController.ResumeRunning();
       }
   }
   ```

## Camera Animation

1. Create Camera Animation Effects:
   - Camera shake for impacts
   - Smooth follow transitions

2. Implement Camera Effects Script:
   ```csharp
   using UnityEngine;
   using System.Collections;
   
   public class CameraEffects : MonoBehaviour
   {
       [SerializeField] private float shakeDuration = 0.5f;
       [SerializeField] private float shakeAmount = 0.7f;
       [SerializeField] private float decreaseFactor = 1.0f;
       
       private Vector3 originalPos;
       private float currentShakeDuration = 0f;
       
       private void Awake()
       {
           originalPos = transform.localPosition;
       }
       
       private void Update()
       {
           if (currentShakeDuration > 0)
           {
               transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
               currentShakeDuration -= Time.deltaTime * decreaseFactor;
           }
           else
           {
               currentShakeDuration = 0f;
               transform.localPosition = originalPos;
           }
       }
       
       public void ShakeCamera()
       {
           originalPos = transform.localPosition;
           currentShakeDuration = shakeDuration;
       }
       
       public void ShakeCamera(float duration, float amount)
       {
           originalPos = transform.localPosition;
           currentShakeDuration = duration;
           shakeAmount = amount;
       }
   }
   ```

## Animation Optimization

1. Performance Considerations:
   - Use animation culling for off-screen objects
   - Reduce animation sample rate for distant objects
   - Use LOD (Level of Detail) for character animations

2. Set Animation Culling Mode:
   - Select the Animator component on your character
   - Set Culling Mode to "Based On Renderers"
   - This disables animation updates when the character isn't visible

3. Use Animation LOD System:
   ```csharp
   using UnityEngine;
   
   public class AnimationLOD : MonoBehaviour
   {
       [SerializeField] private Animator animator;
       [SerializeField] private float highQualityDistance = 10f;
       [SerializeField] private float mediumQualityDistance = 20f;
       
       private Camera mainCamera;
       private float currentUpdateInterval = 0f;
       private float highQualityInterval = 0f; // Every frame
       private float mediumQualityInterval = 0.1f; // Every 10th frame
       private float lowQualityInterval = 0.3f; // Every 30th frame
       
       private void Start()
       {
           mainCamera = Camera.main;
           animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
       }
       
       private void Update()
       {
           float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
           
           if (distance <= highQualityDistance)
           {
               currentUpdateInterval = highQualityInterval;
               animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
           }
           else if (distance <= mediumQualityDistance)
           {
               currentUpdateInterval = mediumQualityInterval;
               animator.cullingMode = AnimatorCullingMode.BasedOnRenderers;
           }
           else
           {
               currentUpdateInterval = lowQualityInterval;
               animator.cullingMode = AnimatorCullingMode.BasedOnRenderers;
           }
           
           // Set animator update rate based on distance
           animator.updateMode = AnimatorUpdateMode.Normal;
           animator.cullingMode = AnimatorCullingMode.BasedOnRenderers;
       }
   }
   ```

## Testing and Quality Assurance

1. Animation Debugging:
   - Use the Animation Debug Window (Windows > Animation > Animation Debug)
   - Check for unintended blending or popping between animations
   - Verify animation transition timing

2. Common Animation Issues:
   - Foot sliding: Adjust root motion or add foot IK
   - Animation popping: Check transition times and blending
   - Missing animation events: Verify event names and timing 