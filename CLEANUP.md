# Demo-Specific Files to Remove

When setting up Golem in a new project, delete these demo-specific files and folders:

## Folders to Delete

```
Assets/PolygonCasino/          # Demo casino 3D assets
Assets/Models/                  # Demo character models
Assets/Camera States/           # Demo camera configurations
Assets/Scenes/                  # Demo scenes (Main.unity, etc.)
Assets/3rd Party Assets/        # Demo third-party assets
```

## Files to Delete

```
Assets/Celeste.prefab           # Demo character prefab
Assets/BlackjackScene.unity     # Demo blackjack scene
```

## Scripts to Keep (Core Golem)

```
Assets/Scripts/Golem/           # Core framework (KEEP)
  - Core/Affordances.cs
  - Core/InteractableObject.cs
  - Core/WorldScanner.cs
  - Core/GolemAgent.cs
  - Core/GolemActionController.cs
  - Core/GolemStateReporter.cs

Assets/Scripts/Character/       # Character control (KEEP)
  - PointClickController.cs
  - EmotePlayer.cs

Assets/Scripts/Systems/         # Systems (KEEP)
  - Networking/CFConnector.cs
  - Camera/CameraStateMachine.cs
  - Camera/CameraStateSO.cs
  - Camera/CameraStateTransitionSO.cs

Assets/Scripts/Utils/           # Utilities (KEEP)
  - WavUtility.cs
```

## Scripts to Optionally Keep

```
Assets/Scripts/Systems/Blackjack/   # Blackjack game (optional example)
Assets/Scripts/Systems/LuaCompiler.cs  # Lua scripting (optional)
Assets/Scripts/Systems/ClaudeCodeController.cs  # Claude integration (optional)
```

## How to Clean Up

In Unity:
1. Delete the folders listed above from the Project window
2. Delete the files listed above
3. Unity will prompt about broken references - click "Remove" for demo scenes
4. Create your own scene and set up your character with:
   - GolemAgent component
   - WorldScanner component
   - PointClickController component
   - NavMeshAgent component
   - Animator component

## Setting Up a New Scene

1. Create a new scene
2. Add a NavMesh to your environment
3. Add your character with the required components
4. Add InteractableObject components to any objects you want discoverable
5. Configure affordances on each InteractableObject
6. Connect CFConnector to your AI backend
