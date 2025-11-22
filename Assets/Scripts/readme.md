# ✈️ Wind Rises - Code Architecture

## Overview

Wind Rises is a cozy flight simulator built with a modular, event-driven architecture. The codebase is organized into 5 independent systems that communicate through a central **EventBus**, ensuring loose coupling and high maintainability.

## Architecture Principles

- **Separation of Concerns**: Each module has a single, well-defined responsibility
- **Event-Driven Communication**: Systems communicate exclusively through events, avoiding direct dependencies
- **Modularity**: Systems can be developed, tested, and modified independently
- **Scalability**: Easy to add new features without affecting existing code

## System Overview

| System | Responsibility | Key Components |
|--------|---------------|----------------|
| **Core** | Central coordination and event management | GameManager, EventBus |
| **Flight** | Aircraft controls and physics simulation | PlaneInput, FlightController, PlaneData |
| **Camera** | Dynamic camera system following the plane | CameraFollow |
| **Environment** | Weather, time of day, and atmospheric effects | WeatherManager, TimeOfDay |
| **UI** | Heads-up display and user interface | UIManager, SpeedUI, AltitudeUI |

## Project Structure

```
Scripts/
├─ Core/
│  ├─ GameManager.cs        # Main game controller
│  └─ EventBus.cs           # Event system for inter-module communication
│
├─ Flight/
│  ├─ PlaneInput.cs         # Input handling (keyboard, controller, etc.)
│  ├─ FlightController.cs   # Physics simulation and flight dynamics
│  └─ PlaneData.cs          # Aircraft configuration and parameters
│
├─ Camera/
│  └─ CameraFollow.cs       # Smooth camera tracking system
│
├─ Environment/
│  ├─ WeatherManager.cs     # Weather system and conditions
│  └─ TimeOfDay.cs          # Day/night cycle and lighting
│
└─ UI/
   ├─ UIManager.cs          # UI coordination
   ├─ SpeedUI.cs            # Speed indicator display
   └─ AltitudeUI.cs         # Altitude indicator display
```

## Data Flow

```
User Input
    ↓
PlaneInput
    ↓
FlightController
    ↓
EventBus ──→ UI (SpeedChanged, AltitudeChanged)
    ↓
CameraFollow
```

## Core Systems

### Flight System

Handles all aircraft-related functionality:

- **PlaneInput**: Captures and processes player input from various sources
- **FlightController**: Implements realistic flight physics and dynamics
- **PlaneData**: Stores aircraft specifications (max speed, turn rate, etc.)

### EventBus System

Central nervous system of the application:

- Enables decoupled communication between modules
- Events are strongly-typed and immutable
- Common events:
  - `SpeedChanged`: Fired when aircraft speed changes
  - `AltitudeChanged`: Fired when aircraft altitude changes
  - `PositionUpdated`: Fired for camera tracking

### Camera System

Provides cinematic camera experience:

- Smooth following with configurable lag
- Responds to plane position/rotation via EventBus
- No direct reference to plane object

### Environment System

Creates atmospheric immersion:

- Dynamic weather conditions
- Day/night cycle with realistic lighting transitions
- Environmental audio and visual effects

### UI System

Displays flight information:

- Listens to EventBus for real-time updates
- HUD elements update independently
- Clean separation from game logic

## Development Guidelines

### Adding New Features

1. Identify which system the feature belongs to
2. Create events for any data that needs to be shared
3. Use EventBus for all cross-system communication
4. Keep systems independent

### Event Naming Convention

- Use past tense for events: `SpeedChanged`, `LandingCompleted`
- Be specific: `EngineStarted` instead of `StateChanged`

### Code Organization

- One class per file
- Keep scripts focused on a single responsibility
- Use namespaces to organize related classes

## Future Enhancements

- **Audio System**: Engine sounds, wind effects
- **Mission System**: Objectives and waypoints
- **Save System**: Player progress and settings
- **Customization**: Aircraft skins and modifications