# Student Recruitment Game Demo

A 3D web-based interactive demo showcasing a university campus environment where players can explore and interact with buildings. Built with Unity and optimized for WebGL deployment.

**[Play the Demo](https://maliksheharyaar.github.io/my-unity-webgl-game/)**

## 🎮 Features

### Player Controls
- **Movement**: WASD keys for character movement
- **Camera Control**: 
  - Mouse movement for camera rotation
  - Locked cursor system for smooth 360° viewing
  - Automatic cursor state management
- **Cursor Management**: 
  - ESC to toggle cursor visibility
  - Left-click to re-lock cursor when unlocked
  - Scene-aware cursor state transitions
  - Automatic cursor unlocking in UI scenes

### Scene Management
- **Main Scene (3D Environment)**:
  - Fully 3D navigable environment
  - Building interaction system with proximity detection
  - Position persistence between scene transitions
  - Optimized cursor controls for smooth camera movement
  - Raycast-based interaction detection

- **Canvas Test Scene (UI Interface)**:
  - Clean UI interface with responsive buttons
  - Scene transition management
  - Future mini-game launch capability
  - Automatic cursor state handling

### Interaction Systems
- **Building Interaction**:
  - Proximity-based interaction detection using collider bounds
  - Visual feedback for interaction zones using Gizmos
  - Interaction point calculated from building center
  - Configurable interaction distance
  - Scene transition handling with position saving

### Technical Features
- **Position Management**:
  - Static position management system
  - Vector3 position validation
  - Position bounds checking
  - Transition state tracking
  - Debug logging system
  - Position update counter
  - Error handling for invalid positions


### 🔧 Optimization
- WebGL-specific optimizations
- Scene loading optimization
- Memory management
- Position validation system
- Error handling for WebGL context

## 🚀 Setup Instructions

### Prerequisites
1. **Unity Version**: 2022.3.19f1 or higher
2. **Required Packages**:
   - Universal Render Pipeline (URP)
   - ProBuilder
   - TextMeshPro
   - Input System (New)

### Project Setup
1. **Initial Setup**:
   ```
   1. Clone repository
   2. Open project in Unity
   3. Install required packages via Package Manager
   4. Open MainScene from Assets/Scenes/
   ```

2. **Scene Configuration**:
   ```
   1. Create empty GameObject named "Managers"
   2. Add required manager components:
      - CursorManager
      - PlayerSpawnHandler
      - PersistentObject
   3. Ensure Player object has "Player" tag
   ```

3. **Building Setup**:
   ```
   1. Add BuildingInteraction component to building
   2. Set interaction distance in Inspector
   3. Ensure building has either:
      - MeshRenderer component
      - Or valid transform position
   ```

4. **Player Configuration**:
   ```
   1. Tag player GameObject as "Player"
   2. Add required components:
      - Character Controller/Rigidbody
      - Camera setup for rotation
   3. Configure movement settings
   ```

### Build Settings
1. **WebGL Settings**:
   ```
   1. Switch platform to WebGL
   2. Player Settings:
      - Compression Format: Disabled (for GitHub Pages)
      - Memory Size: 512MB
      - Enable Exception Support: Yes
   ```

2. **Scene Setup**:
   ```
   1. Add scenes to build settings:
      - MainScene (index 0)
      - CanvasTestScene (index 1)
   2. Enable "Auto Build" for WebGL
   ```

### Debug Setup
1. **Position Debugging**:
   ```
   1. Add empty GameObject "PositionDebugger"
   2. Attach PlayerPositionDebugger component
   3. Enable "Development Build" for logging
   ```

## 📝 Development Notes

### Script Dependencies

## 🚀 Deployment

### GitHub Pages Setup
1. Build WebGL project
2. Configure repository settings
3. Enable GitHub Pages
4. Set build folder as source

### Build Optimization
- Disabled compression (GitHub Pages handles this)
- Minimal memory allocation
- Optimized asset loading
- Browser compatibility checks

## 📦 Future Updates
- [ ] Additional building interactions
- [ ] Mini-game implementation
- [ ] Enhanced UI feedback
- [ ] More campus areas
- [ ] Character customization options


### Important Considerations for now
1. **Position Management**:
   - Position is saved before scene transitions
   - Validation occurs during save/load
   - Automatic cleanup after position restoration

2. **Scene Transitions**:
   - Cursor state changes automatically
   - Position data persists through static manager
   - Scene loading handles edge cases

3. **Debugging**:
   - Console logs track position updates
   - Visual feedback in Scene view
   - Inspector debugging tools available
