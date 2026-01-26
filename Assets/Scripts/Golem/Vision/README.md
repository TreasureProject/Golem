# Golem Vision System

Visual perception system for Golem AI agents using Vision-Language Models (VLMs).

## Quick Start

### 1. Create VisionConfig Asset

1. Right-click in Project window
2. Select **Create > Golem > Vision Config**
3. Configure settings:
   - **Provider**: OpenAI, Anthropic, or Ollama
   - **API Key**: Your API key (not needed for Ollama)
   - **Model Name**: e.g., `gpt-4o`, `claude-3-opus-20240229`, `llava`

### 2. Add to GolemAgent

Add the `GolemVisionIntegration` component to your GolemAgent:

```
GameObject with GolemAgent
├── GolemAgent (existing)
├── GolemVisionIntegration (add this)
│   └── Assign your VisionConfig
```

The integration will automatically create all required child components.

### 3. Test with Debug UI

Add `VisionDebugUI` component and press **F9** at runtime to:
- Trigger visual scans
- View detected objects
- Monitor cache and VLM statistics
- Check hallucination detection rates

## Components

| Component | Purpose |
|-----------|---------|
| `VisionConfig` | ScriptableObject with all settings |
| `FrameCaptureService` | Captures camera frames |
| `VLMClient` | Communicates with VLM APIs |
| `VisualObjectCache` | Caches results to minimize API calls |
| `HallucinationDetector` | Filters VLM hallucinations |
| `ActionVerifier` | VIGA-style before/after verification |
| `VisualPerceptionManager` | Orchestrates the pipeline |
| `PerceptionFuser` | Merges structured + visual data |
| `GolemVisionIntegration` | Connects to GolemAgent |
| `VisionDebugUI` | Runtime debug panel |

## Configuration

### VisionConfig Settings

```
[General]
- enabled: Enable/disable the system
- provider: OpenAI | Anthropic | Ollama
- apiKey: Your API key
- modelName: Model to use (e.g., gpt-4o)

[Capture]
- captureWidth/Height: Frame resolution (256-1024)
- jpegQuality: Compression quality (50-100)
- captureMode: AgentPOV | ThirdPerson | Overhead

[Cache]
- enableCache: Enable response caching
- cacheTTL: Time-to-live in seconds
- cacheInvalidationDistance: Distance threshold (meters)
- maxCacheEntries: Maximum cached entries

[Quality]
- minVisualConfidence: Minimum object confidence (0-1)
- minVerificationConfidence: Minimum verification confidence

[Cost]
- maxCostPerHour: Budget limit in dollars
- pauseOnBudgetExceeded: Auto-pause when over budget
```

### Prompt Templates

Create custom prompts via **Create > Golem > VLM Prompt Templates**:
- Scene Understanding
- Action Verification
- Affordance Discovery

## API Usage

### Request a Visual Scan

```csharp
var vision = GetComponent<GolemVisionIntegration>();

vision.RequestVisualScan(result => {
    if (result.success) {
        Debug.Log($"Scene: {result.sceneDescription}");
        foreach (var obj in result.objects) {
            Debug.Log($"Found: {obj.name} ({obj.confidence:P0})");
        }
    }
});
```

### Get Fused Perception

```csharp
var fusedResult = vision.GetFusedPerception();

foreach (var obj in fusedResult.objects) {
    if (obj.source == PerceptionSource.CrossValidated) {
        // High confidence - found by both WorldScanner and VLM
    }
}
```

### Enhanced World State

```csharp
var report = vision.GenerateEnhancedWorldState();

// Includes both structured objects AND visual-only discoveries
Debug.Log($"Visual objects: {report.visualObjects.Count}");
Debug.Log($"Scene: {report.sceneDescription}");
```

## Running Tests

### Unity Test Runner

1. Open **Window > General > Test Runner**
2. Select **EditMode** tab
3. Run tests in `Golem.Vision.Tests`

### Command Line

```bash
Unity -runTests -batchmode \
  -projectPath /path/to/Golem \
  -testPlatform EditMode \
  -testResults results.xml
```

## Providers

### OpenAI (GPT-4V)

```
Provider: OpenAI
API Key: sk-...
Model: gpt-4o
Base URL: (leave empty for default)
```

### Anthropic (Claude 3)

```
Provider: Anthropic
API Key: sk-ant-...
Model: claude-3-opus-20240229
Base URL: (leave empty for default)
```

### Ollama (Local)

```
Provider: Ollama
API Key: (leave empty)
Model: llava
Base URL: http://localhost:11434/api
```

## Architecture

```
GolemAgent
    │
    ├── WorldScanner (structured perception)
    │
    └── GolemVisionIntegration
            │
            ├── FrameCaptureService → Camera
            │
            ├── VLMClient → OpenAI/Anthropic/Ollama
            │
            ├── VisualObjectCache
            │
            ├── HallucinationDetector
            │
            ├── ActionVerifier
            │
            ├── VisualPerceptionManager
            │
            └── PerceptionFuser → Merges structured + visual
```

## Trigger-Based Capture

Visual scans are triggered by:
- Agent movement (>2m distance)
- Agent rotation (>45° angle)
- Zone/area changes
- Interaction completion (for verification)
- Manual request

## Cost Management

- **Caching**: Reuses results for similar positions
- **Budget Tracking**: Configurable hourly limit
- **Tiered Providers**: Use Ollama for low-priority queries
- **Smart Triggers**: Only capture when valuable

## Hallucination Detection

Objects are filtered if:
- Confidence < threshold (default 0.6)
- Position impossible (too high/low)
- Affordance violates common sense (e.g., "sit on wall")
- Not cross-validated with WorldScanner

## License

Part of the Golem AI Agent Framework.
