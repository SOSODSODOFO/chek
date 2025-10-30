# Chek Roblox Control Interface

This repository contains a modular Roblox framework that fulfils the extended specification for
control, analytics, visualization, and interface systems. Each subsystem is designed as an
independent ModuleScript with clear lifecycle methods so that the framework can be dropped into
an experience and orchestrated through `src/init.lua`.

## Subsystems Overview

### Control Systems
- **MovementModule** – configurable navigation modes, altitude and speed smoothing, and profile
  application hooks. 【F:src/ControlSystems/MovementModule.lua†L1-L98】
- **EnvironmentModule** – collision profile management, pass-through tuning, and automatic physics
  overrides for tagged objects. 【F:src/ControlSystems/EnvironmentModule.lua†L1-L104】

### Analytics & Visualization
- **AnalyticsHub** – tracks other participants, smooths statistics, and prioritises targets using
  weighted scoring. 【F:src/Analytics/AnalyticsHub.lua†L1-L122】
- **VisualizationHub** – renders indicators, distance billboards, skeleton highlights, and theme-aware
  colour palettes. 【F:src/Visualization/VisualizationHub.lua†L1-L119】

### User Interface
- **ControlPanel** – tabbed navigation, search/filter signal, and animated state transitions. 【F:src/UI/ControlPanel.lua†L1-L133】
- **ThemeManager** – preset and custom theme handling. 【F:src/UI/ThemeManager.lua†L1-L74】
- **PreviewModule** – lightweight 3D preview for demonstrating configuration changes. 【F:src/UI/PreviewModule.lua†L1-L54】
- **BackgroundEffects** – animated backgrounds with particles, waves, and geometry modes. 【F:src/UI/BackgroundEffects.lua†L1-L139】

### Additional Modules
- **AutomationSuite** – scheduled automation hooks for movement and environment adjustments. 【F:src/Automation/AutomationSuite.lua†L1-L69】
- **Logger & SystemManager** – central logging, camera control, and subsystem coordination. 【F:src/Systems/Logger.lua†L1-L46】【F:src/Systems/SystemManager.lua†L1-L129】
- **PerformanceAnalyzer** – heartbeat-driven FPS and memory analytics. 【F:src/Systems/PerformanceAnalyzer.lua†L1-L66】
- **ObjectManager** – descriptor-based tagging, anchoring, and highlighting for world objects. 【F:src/Systems/ObjectManager.lua†L1-L74】
- **EnvironmentModifiers** – atmosphere, fog, and ambient lighting modifiers with reset support. 【F:src/Systems/EnvironmentModifiers.lua†L1-L55】

## Entry Point

The `Framework` module located at `src/init.lua` wires all subsystems together, providing a single
API for `Init`, `Start`, and `Destroy`. Configuration tables allow callers to tailor profiles,
visual themes, automation cadence, camera behaviour, and environmental presets. 【F:src/init.lua†L1-L138】

## Usage

1. Place the `src` folder in `ReplicatedStorage` (or another shared location).
2. Require `src/init.lua` from a LocalScript.
3. Call `Framework:Init()` with optional configuration tables.
4. Call `Framework:Start()` to begin runtime updates.

The modular structure ensures that future components—such as new visualization layers or automation
routines—can be added without altering existing contracts.
