# Kinetic Shift - Coursework Game Project

A Unity-based stealth and puzzle game featuring dynamic object scaling mechanics and AI-controlled security bots.

## Overview

Kinetic Shift is a game that combines stealth gameplay with physics-based puzzle solving. Players control a character that can manipulate objects by changing their size and mass properties. The game features intelligent security bot AI, pressure plate mechanics, and multi-level progression.

## Game Features

### Core Mechanics
- **Kinetic Shift System**: Scale objects between small and large sizes to solve puzzles and navigate levels
- **Dynamic Physics**: Object mass and friction change based on scale, affecting how they interact with the environment
- **Stealth Gameplay**: Avoid detection by security bots with intelligent patrol, chase, and search behaviors
- **Pressure Plate Puzzles**: Coordinate multiple pressure plates to unlock doors and progress

### Gameplay Elements
- **Security Bot AI**: Intelligent enemies with:
  - Patrol patterns
  - Vision-based detection
  - Chase mechanics
  - Confusion state when losing the player
  - Search behavior after losing sight of target
  
- **Multiple Levels**: Three progressively challenging levels with unique puzzles
- **Lives System**: Player starts with 3 lives, lost when hit by drones
- **UI Feedback**: Real-time life counter and level information display
- **Victory/Defeat Conditions**: Game over and victory flow screens

## Project Structure

```
Assets/
├── Scripts/              # Core game scripts
│   ├── GameManager.cs           # Global game state management
│   ├── KineticShiftController.cs # Object scaling mechanics
│   ├── SecurityBotController.cs  # AI bot behavior
│   ├── AI_Orchestrator.cs        # Multi-bot coordination
│   ├── PressurePlateController.cs # Pressure plate logic
│   ├── DroneController.cs        # Drone mechanics
│   ├── DoorMovementController.cs # Dynamic door behavior
│   ├── LevelLoader.cs           # Scene loading logic
│   ├── LevelInfoDisplay.cs      # UI information display
│   ├── GameOverFlow.cs          # Game over state
│   ├── VictoryFlow.cs           # Victory state
│   └── MainMenu.cs              # Main menu logic
├── Scenes/
│   ├── MainMenu.unity           # Main menu scene
│   ├── Level_01.unity           # First level
│   ├── Level_02.unity           # Second level
│   ├── Level_03.unity           # Third level
│   ├── Scene_Game_Over.unity    # Game over screen
│   ├── Scene_Victory.unity      # Victory screen
│   └── Test_PhysicsScene.unity  # Physics testing
├── Prefabs/              # Reusable game objects
├── AttackBot/            # Bot-related assets
├── Level_01/             # Level-specific assets
├── Level_02/             # Level-specific assets
├── Editor/               # Editor scripts
└── Settings/             # Game configuration
```

## Key Scripts

### GameManager.cs
Singleton pattern manager handling:
- Player lives tracking
- Invulnerability frames
- Game over/victory conditions
- Scene transitions

### KineticShiftController.cs
Manages object scaling with:
- Scale variations (small: 0.2x, large: 3x)
- Dynamic mass adjustment
- Physics material switching (high/low friction)
- NavMesh obstacle updates

### SecurityBotController.cs
Advanced AI bot with states:
- **Patrol**: Following designated waypoints
- **Chase**: Pursuing detected player
- **Search**: Searching last known position
- **Confused**: Disoriented when losing the player

### AI_Orchestrator.cs
Coordinates multiple bots for:
- AND gate logic on pressure plates
- Multi-bot puzzle solutions
- Level progression triggers

### PressurePlateController.cs
Handles pressure-sensitive triggers for:
- Door unlocking mechanisms
- Puzzle progression
- Coordinated puzzle solutions

## Unity Configuration

- **Engine Version**: Unity 6 (6000.0.60f1)
- **Input System**: Modern Input System
- **UI Framework**: TextMesh Pro
- **AI Navigation**: NavMesh system
- **Physics**: 3D Physics with Rigidbody components

## Getting Started

### Prerequisites
- Unity 6.0 or later
- Windows/Mac/Linux
- At least 2GB of disk space

### Installation
1. Open the `Coursework.sln` solution in Unity or your IDE
2. Load the `MainMenu.unity` scene to start
3. Press Play in the Unity Editor

### Controls
- **Movement**: WASD or Arrow Keys
- **Action**: Spacebar (to activate Kinetic Shift)
- **Menu Navigation**: Mouse

## Level Descriptions

### Level 1
Introduction to basic mechanics and simple stealth gameplay.

### Level 2
Introduces pressure plate mechanics and multi-object puzzle solving with the AND gate system.

### Level 3
Advanced challenges combining stealth, scaling, and coordinated puzzle solutions.

## Physics Materials

- **High Friction Material**: Prevents sliding, used for stable ground
- **Low Friction Material**: Allows sliding, used for ice/slippery surfaces

## Development Notes

- Uses C# with MonoBehaviour component system
- NavMesh-based AI pathfinding
- Coroutine-based timing and state management
- Input actions configuration for flexible input handling

## Credits

This is a coursework project developed as part of a game development curriculum.

## License

This project is for educational purposes.
