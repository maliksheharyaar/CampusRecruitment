# Endless Runner - Student Recruitment Game

This package contains an endless runner mini-game for the student recruitment project.

## Overview

The endless runner allows players to:
- Run through procedurally generated tracks
- Jump over obstacles and slide under barriers
- Collect power-ups and coins
- Avoid the rolling boulder chasing from behind
- Reach the finish line to complete levels

## Complete Setup Instructions

### 1. Scene Setup
1. Create a new scene named "EndlessRunner"
2. Add required components:
   - Main Camera
   - Directional Light
   - UI Canvas (scale mode: Scale With Screen Size)
   - EventSystem

### 2. Manager Objects Setup
Set up a hierarchy of manager objects:
```
[GameManager]
  - EndlessRunnerManager
  - UIManager
  - AudioManager
  - PauseManager
```

### 3. Player Setup
1. Create a Player GameObject:
   - Add CharacterController component
   - Add RunnerController script
   - Create a GroundCheck child object at the player's feet
   - Add player model with Animator
2. Configure RunnerController:
   - Lane Distance: 3
   - Jump Height: 2
   - Jump/Slide Time: 0.5
   - Max Lives: 3
   - Assign the model transform
   - Assign GroundCheck transform
   - Set Ground Distance: 0.4
   - Create and assign Ground Layer

### 4. Track System
1. Create Track Segment prefabs:
   - Standard track with 3 lanes
   - Add colliders for ground
   - Tag as "Ground"
   - Add to ground layer
2. Create a Finish Line prefab:
   - Add distinct visual elements
   - Add collider (set as trigger)
   - Tag as "Finish"
   - Add FinishLineTrigger script
3. Configure EndlessRunnerManager:
   - Assign track segment prefabs
   - Assign finish line prefab
   - Set track segments to spawn (10-15)
   - Set segment length (20)
   - Create and assign track parent transform
   - Assign player controller

### 5. Obstacle System
1. Create obstacle prefabs:
   - Wall: requires lane change
   - Jump: requires jumping
   - Slide: requires sliding
2. For each obstacle:
   - Add appropriate colliders
   - Add Obstacle script
   - Tag as "Obstacle"
   - Configure hit effects

### 6. Power-up System
1. Create power-up prefabs:
   - Invincibility
   - Speed Boost
   - Extra Life
2. For each power-up:
   - Add trigger collider
   - Add PowerUp script
   - Tag as "PowerUp"
   - Set power-up type
   - Add visual effects

### 7. UI Setup
Configure the UIManager with:
- Main Game HUD (lives, score)
- Game Over Panel
- Win Panel
- Pause Menu
- Power-up indicators

### 8. Boulder Setup
1. Create a Boulder GameObject:
   - Add SphereCollider component
   - Add BoulderController script
   - Set distance behind player (15)
   - Set rotation speed (180)
   - Add dust effects/sound

### 9. Audio Setup
Configure AudioManager with sounds for:
- Background music
- Jump
- Slide
- Hit obstacle
- Collect power-up
- Collect coin
- Game over
- Win

### 10. Input System
The PlayerInputActions script automatically sets up these controls:
- **W/Up Arrow/Space**: Jump
- **S/Down Arrow**: Slide
- **A/Left Arrow**: Move left
- **D/Right Arrow**: Move right
- **ESC/P**: Pause the game

### 11. Testing and Debugging
1. Enter Play mode to test
2. Check that the player can:
   - Move between lanes
   - Jump and slide
   - Collect power-ups
   - Lose lives on obstacle hits
   - Reach the finish line
3. Verify that UI updates correctly
4. Check that the game pauses and resumes

## Integration with Main Game

The endless runner mini-game can be integrated with your main game by:
1. Creating a trigger or interaction point in your main scene
2. Loading the endless runner scene when triggered
3. Saving rewards/progress when the runner game ends
4. Returning to the main scene

## Scripts Overview

- **EndlessRunnerManager**: Controls game flow, track generation, and score
- **RunnerController**: Handles player movement, jumping, sliding, and collisions
- **PowerUp**: Manages power-up behavior and effects
- **Obstacle**: Controls obstacle behavior and collisions
- **UIManager**: Manages all UI elements and updates
- **BoulderController**: Controls the boulder chasing the player
- **FinishLineTrigger**: Handles level completion
- **PauseManager**: Manages game pauses
- **PlayerInputActions**: Handles player input using the Input System

## Troubleshooting

If you encounter issues:
1. **Player movement problems**: Verify CharacterController settings and ground check
2. **Collision issues**: Check tags and collider configurations
3. **Input not working**: Ensure PlayerInputActions is properly initialized
4. **Performance issues**: Reduce visible track segments or simplify effects

See the additional markdown guides for detailed setup of:
- PrefabSetup.md: Complete prefab creation guide
- PhysicsSetup.md: Physics configuration instructions
- UISetup.md: Detailed UI setup guide
- LevelDesignGuide.md: Level design principles 