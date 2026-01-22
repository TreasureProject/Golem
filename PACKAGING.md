# Golem Unity Package Distribution

This document describes how to package and distribute the Golem framework.

## Option 1: Unity Package (.unitypackage)

The traditional way to distribute Unity assets.

### Export Steps

1. Open your Unity project containing Golem
2. In the Project window, select the `Assets/Scripts/Golem` folder
3. Right-click and select **Export Package...**
4. In the dialog, ensure these are checked:
   - `Assets/Scripts/Golem/Core/*`
   - `Assets/Scripts/Golem/Interactions/*`
5. Optionally include:
   - `Assets/Scripts/Character/PointClickController.cs` (if needed for compatibility)
6. Click **Export...**
7. Save as `Golem-v1.0.0.unitypackage`

### Import Steps (For Users)

1. Open Unity project
2. Go to **Assets > Import Package > Custom Package...**
3. Select the `.unitypackage` file
4. Click **Import**

## Option 2: Unity Package Manager (UPM)

Modern package distribution via Git URL or local path.

### Package Structure

Create this structure in a separate repository or folder:

```
com.golem.framework/
├── package.json
├── README.md
├── LICENSE
├── Runtime/
│   ├── Golem.Runtime.asmdef
│   ├── Core/
│   │   ├── GolemAgent.cs
│   │   ├── PersonalityProfile.cs
│   │   ├── PersonalityPresets.cs
│   │   ├── InteractableObject.cs
│   │   ├── Affordances.cs
│   │   ├── WorldScanner.cs
│   │   ├── WorldMemory.cs
│   │   ├── InteractionExecutor.cs
│   │   ├── GolemActionController.cs
│   │   └── GolemStateReporter.cs
│   └── Interactions/
│       ├── SeatInteraction.cs
│       ├── DoorInteraction.cs
│       ├── ArcadeInteraction.cs
│       └── ExamineInteraction.cs
└── Samples~/
    └── BasicAgent/
        └── ... (example scene and scripts)
```

### package.json

```json
{
  "name": "com.golem.framework",
  "version": "1.0.0",
  "displayName": "Golem AI Framework",
  "description": "Universal AI agent framework for Unity. Drop AI-controlled characters into any world.",
  "unity": "2021.3",
  "documentationUrl": "https://github.com/yourname/golem",
  "changelogUrl": "https://github.com/yourname/golem/blob/main/CHANGELOG.md",
  "licensesUrl": "https://github.com/yourname/golem/blob/main/LICENSE",
  "keywords": [
    "ai",
    "agent",
    "npc",
    "bot",
    "behavior",
    "claude",
    "llm"
  ],
  "author": {
    "name": "Your Name",
    "email": "your@email.com",
    "url": "https://yoursite.com"
  },
  "dependencies": {}
}
```

### Assembly Definition (Golem.Runtime.asmdef)

```json
{
  "name": "Golem.Runtime",
  "rootNamespace": "Golem",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

### Install via Git URL

Users can install via Package Manager:

1. Open **Window > Package Manager**
2. Click **+** button > **Add package from git URL...**
3. Enter: `https://github.com/yourname/golem.git?path=/com.golem.framework`
4. Click **Add**

### Install via Local Path

For development:

1. Clone repository to local machine
2. Open **Window > Package Manager**
3. Click **+** button > **Add package from disk...**
4. Navigate to `com.golem.framework/package.json`
5. Click **Open**

## Option 3: OpenUPM

For public distribution via OpenUPM registry.

### Setup

1. Create a GitHub repository for the package
2. Submit to OpenUPM: https://openupm.com/packages/add/
3. Users can then install via:

```bash
openupm add com.golem.framework
```

Or add to `manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.golem"]
    }
  ],
  "dependencies": {
    "com.golem.framework": "1.0.0"
  }
}
```

## Version Numbering

Follow Semantic Versioning (SemVer):

- **MAJOR** (1.0.0 → 2.0.0): Breaking API changes
- **MINOR** (1.0.0 → 1.1.0): New features, backwards compatible
- **PATCH** (1.0.0 → 1.0.1): Bug fixes, backwards compatible

## Release Checklist

Before each release:

- [ ] Update version in `package.json`
- [ ] Update CHANGELOG.md
- [ ] Run all tests
- [ ] Test import into fresh Unity project
- [ ] Test with minimum supported Unity version (2021.3)
- [ ] Update documentation if needed
- [ ] Create GitHub release with tag (e.g., `v1.0.0`)
- [ ] Export .unitypackage for non-UPM users

## Current Package Contents

### Core Scripts (Required)

| File | Description |
|------|-------------|
| `GolemAgent.cs` | Central controller |
| `PersonalityProfile.cs` | Personality traits ScriptableObject |
| `PersonalityPresets.cs` | Preset personality archetypes |
| `InteractableObject.cs` | Self-describing object component |
| `Affordances.cs` | Interaction type constants |
| `WorldScanner.cs` | Object discovery system |
| `WorldMemory.cs` | Persistent memory |
| `InteractionExecutor.cs` | Interaction routing |

### Interaction Handlers (Optional)

| File | Description |
|------|-------------|
| `SeatInteraction.cs` | Sit/stand handling |
| `DoorInteraction.cs` | Open/close handling |
| `ArcadeInteraction.cs` | Play handling |
| `ExamineInteraction.cs` | Look/examine handling |

### Networking (Optional)

| File | Description |
|------|-------------|
| `GolemActionController.cs` | WebSocket command handler |
| `GolemStateReporter.cs` | State reporting to backend |

## Dependencies

Golem has no external dependencies. It uses only:

- Unity Engine (2021.3+)
- Unity AI Navigation (NavMesh)

If users need WebSocket support, they can use:
- NativeWebSocket (recommended)
- Unity's built-in networking
- Any WebSocket library of their choice

## File Size

Approximate package sizes:

- Core scripts only: ~50 KB
- With interactions: ~65 KB
- With samples: ~100 KB
- Full .unitypackage: ~150 KB
