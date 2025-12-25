# TODO

## Luny

### Core

- LunyUrl class => a string wrapper representing a path in game engines
  - converts to engine-native paths when passed into engine-native code ie prefixes with "res://" in Godot, normalizes path separators, provides "relative path" accessor (refactor CodeSmile AssetDatabase Path class)

### Telemetry

- Can we use GoatCounter to submit telemetry of Luny usage? 
  - Ie send ping every time LunyEngine initializes, with non-personal data: machine/user ID, project ID, engine name and version, (editor or build, debug or release)

### Lifecycle: Create/Enable, Destroy/Disable

- State Monitoring or engine event hooks.
    - Godot: signals (NodeAdded..)
    - Unity: lifecycle component + static events; and/or state tracking

## LunyLua

Add LuaCSharp as separate, optional LunyLua submodule which LunyScript will utilize.

## LunyScript

### Issues

- inactive GameObjects still run the scripts

### NEXT-STEPS.md

Briefly listing the tasks in that document:

- Hot Reload (see NEXT-STEPS.md)
- Composite blocks & conditionals
- Variable Get/Set Blocks & Utilities
- Event System Foundation (Input, Collision, ..)
- Testing Infrastructure
- Block Extensibility (write custom blocks, ie document, template)
- Inspector Variables Integration

### API

- Move properties/methods to static subclasses 
  - EditorPausePlayer => Editor.PausePlayer
- rename LunyScript variables?
  - => LocalVar, GlobalVar, EditorVar (or keep InspectorVar?)
  - Variables.Get<> => Variables[""].AsNumber / Variables[""].AsString

### Execution
- ScriptContext => CONSIDER split in two: internal (runnables, debug, profile) and public (variables, object, script def)
- need a way to specificy which observers to run 

### Diagnostics

- Begin/EndObserver calls need a category (OnUpdate, OnLateUpdate, etc)
- disable uses of debughook & profiler unless explicitly enabled (overhead!)
- Diagnostic blocks => ensure they don't even get added to sequences in release builds
