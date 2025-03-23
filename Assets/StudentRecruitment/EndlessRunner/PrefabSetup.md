# Prefab Setup Guide for Endless Runner

This guide provides detailed instructions for creating all the necessary prefabs for the endless runner game.

## Track Segment Prefabs

### Basic Track Segment
1. Create a new empty GameObject named "TrackSegment"
2. Add a mesh for the base (plane or custom model):
   - Scale to width: 9 units (3 lanes x 3 units each)
   - Scale to length: 20 units (standard segment length)
3. Add a Box Collider component:
   - Adjust to match the size of the floor mesh
   - Tag as "Ground"
   - Add to Ground layer
4. Create lane markers or visual dividers between lanes
5. Create the prefab by dragging the TrackSegment GameObject to the Prefabs folder

### Track Segment Variants (Optional)
1. Duplicate the basic track segment
2. Add decorative elements to the sides (trees, buildings, etc.)
3. Ensure decorations don't interfere with player movement
4. Create multiple variants for visual variety

## Obstacle Prefabs

### Wall Obstacle
1. Create a new empty GameObject named "WallObstacle"
2. Add a Box Collider component:
   - Size X: 3 (one lane width)
   - Size Y: 2 (player height)
   - Size Z: 0.5 (thickness)
   - Check "Is Trigger"
3. Add the Obstacle script component
4. Create a 3D model/primitive for the visual representation as a child
5. Tag as "Obstacle"
6. Configure Obstacle script properties:
   - Assign hit effect prefab if available
   - Assign hit sound if available
7. Create the prefab

### Jump Obstacle
1. Create a new empty GameObject named "JumpObstacle"
2. Add a Box Collider component:
   - Size X: 3 (one lane width)
   - Size Y: 1 (lower height to jump over)
   - Size Z: 0.5 (thickness)
   - Position Y: 0.5 (half height above ground)
   - Check "Is Trigger"
3. Add the Obstacle script component
4. Create a 3D model/primitive for the visual representation as a child
5. Tag as "Obstacle"
6. Configure Obstacle script properties
7. Create the prefab

### Slide Obstacle
1. Create a new empty GameObject named "SlideObstacle"
2. Add a Box Collider component:
   - Size X: 3 (one lane width)
   - Size Y: 1 (upper portion to slide under)
   - Size Z: 0.5 (thickness)
   - Position Y: 1.5 (above player sliding height)
   - Check "Is Trigger"
3. Add the Obstacle script component
4. Create a 3D model/primitive for the visual representation as a child
5. Tag as "Obstacle"
6. Configure Obstacle script properties
7. Create the prefab

## Power-Up Prefabs

### Invincibility Power-Up
1. Create a new empty GameObject named "InvincibilityPowerUp"
2. Add a Sphere Collider component:
   - Radius: 0.5
   - Check "Is Trigger"
3. Add the PowerUp script component
4. Set the PowerUpType to "Invincibility"
5. Create a visual model as a child:
   - Use a shield or star shape
   - Add a glow effect material
   - Add rotation animation
6. Add a ParticleSystem component for collection effect
7. Tag as "PowerUp"
8. Create the prefab

### Speed Boost Power-Up
1. Create a new empty GameObject named "SpeedBoostPowerUp"
2. Add a Sphere Collider component:
   - Radius: 0.5
   - Check "Is Trigger"
3. Add the PowerUp script component
4. Set the PowerUpType to "SpeedBoost"
5. Create a visual model as a child:
   - Use a lightning bolt or arrow shape
   - Add a bright material
   - Add rotation animation
6. Add a ParticleSystem component for collection effect
7. Tag as "PowerUp"
8. Create the prefab

### Extra Life Power-Up
1. Create a new empty GameObject named "ExtraLifePowerUp"
2. Add a Sphere Collider component:
   - Radius: 0.5
   - Check "Is Trigger"
3. Add the PowerUp script component
4. Set the PowerUpType to "ExtraLife"
5. Create a visual model as a child:
   - Use a heart or plus symbol
   - Add a red/green material
   - Add floating animation
6. Add a ParticleSystem component for collection effect
7. Tag as "PowerUp"
8. Create the prefab

## Coin Prefab

1. Create a new empty GameObject named "Coin"
2. Add a Sphere Collider component:
   - Radius: 0.3
   - Check "Is Trigger"
3. Add the Coin script component
4. Set the coin value (default: 1)
5. Create a coin model as a child:
   - Use a flat cylinder or custom coin model
   - Add gold material
   - Add rotation animation
6. Add a ParticleSystem component for collection effect
7. Tag as "Coin"
8. Create the prefab

## Finish Line Prefab

1. Create a new empty GameObject named "FinishLine"
2. Add a Box Collider component:
   - Size X: 9 (full track width)
   - Size Y: 4 (taller than player)
   - Size Z: 1 (thickness)
   - Check "Is Trigger"
3. Add the FinishLineTrigger script component
4. Create a visual representation:
   - Add an arch or banner model
   - Use clear "FINISH" text
   - Add bright, visible materials
5. Add a ParticleSystem component:
   - Configure with confetti/celebration particles
   - Set to trigger when player crosses
6. Assign the particle system to the "finishParticles" field
7. Assign finish sound effect if available
8. Tag as "Finish"
9. Create the prefab

## Player Prefab

1. Create a new empty GameObject named "Player"
2. Add a Character Controller component:
   - Height: 2
   - Radius: 0.5
   - Center: (0, 1, 0)
   - Step Offset: 0.3
3. Add the RunnerController script component
4. Configure RunnerController:
   - Lane Distance: 3
   - Jump Height: 2
   - Jump Time: 0.5
   - Slide Height: 0.5
   - Slide Time: 0.5
   - Lane Change Speed: 5
   - Max Lives: 3
5. Create a GroundCheck empty child GameObject:
   - Position at the bottom of the player (0, 0, 0)
   - Assign to the Ground Check field in RunnerController
6. Add a player model as a child:
   - Use a character model with animations
   - Assign to the Model Transform field
7. Add a shield VFX object (disabled by default)
   - Assign to Shield VFX field
8. Create an Animator Controller with:
   - Idle animation
   - Run animation
   - Jump animation
   - Slide animation
   - Turn animation
   - Hit animation
9. Set the Tag to "Player"
10. Create the prefab

## Boulder Prefab

1. Create a new empty GameObject named "Boulder"
2. Add a Sphere Collider component:
   - Radius: 2
   - Check "Is Trigger"
3. Add the BoulderController script component
4. Configure BoulderController:
   - Distance Behind Player: 15
   - Rotation Speed: 180
5. Add a boulder model as a child:
   - Use a large sphere or rock model
   - Assign to Boulder Model field
6. Add a dust ParticleSystem as a child:
   - Configure to emit from the bottom of the boulder
   - Assign to Dust Effect field
7. Add an AudioSource component (optional):
   - Assign a rolling sound clip
   - Set to loop
   - Adjust spatial blend and volume
8. Create the prefab

## Advanced Tips

1. **Collider Optimization**:
   - Use primitive colliders (box, sphere) instead of mesh colliders
   - Simplify collider shapes where possible for better performance

2. **Visual Hierarchy**:
   - Keep collision components at the root level
   - Group visual elements as children for easier management

3. **Animation Setup**:
   - Add subtle animations to objects for visual appeal
   - Consider using Unity's Animation system for simple movements

4. **Performance Considerations**:
   - Keep polygon counts reasonable for mobile platforms
   - Use Level of Detail (LOD) for complex objects
   - Batch similar materials where possible

5. **Testing**:
   - Test each prefab individually before integrating
   - Verify collision detection works as expected
   - Check visibility from the player's perspective 