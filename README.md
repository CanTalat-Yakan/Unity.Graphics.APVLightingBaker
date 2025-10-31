# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# APV Lighting Baker

> Quick overview: Editor helper to add/apply and bake HDRP Adaptive Probe Volume (APV) lighting scenarios by name (sync or async), plus a one‑click mesh converter to receive GI from Probe Volumes. Includes a tiny provider for accessing the active ProbeReferenceVolume and an init callback.

Bake and manage APV lighting scenarios directly from a component. Create/apply a scenario on the current baking set, then trigger `Lightmapping.Bake`/`BakeAsync`. Optionally convert all scene MeshRenderers to receive GI from Probe Volumes. A small provider class exposes `ProbeReferenceVolume.instance` and notifies when APV is initialized.

![screenshot](Documentation/Screenshot.png)

## Features
- Bake lighting scenarios
  - `BakeLightingScenario(scenarioName, async = false)`
  - Guards against concurrent bakes with `IsBakingInProgress` (wraps `Lightmapping.isRunning`)
  - Times the bake and logs success with duration
- Create/apply scenarios on the current baking set
  - `AddAndApplyLightingScenario(scenarioName)` calls `TryAddScenario` and sets `Volume.lightingScenario`
- Convert meshes for Probe Volumes GI
  - `ConvertAllMeshesToProbeVolumesGI()` sets all loaded-scene `MeshRenderer`s to:
    - `receiveGI = LightProbes`
    - `lightProbeUsage = UseProxyVolume`
    - Marks them dirty so changes persist
- Probe Reference Volume provider
  - `ProbeReferenceVolumeProvider.Volume` → `ProbeReferenceVolume.instance`
  - `IsInitialized` flips true once `Volume.isInitialized` and a `currentBakingSet` exists
  - `AddListener(Action)` invokes your callback upon first initialization

## Requirements
- Unity 6000.0+ (Editor‑only scripts)
- HDRP with Adaptive Probe Volumes enabled
- A Probe Reference Volume in the scene with a valid Baking Set

## Usage

1) Set up APV in your HDRP project
- Enable Adaptive Probe Volumes in the HDRP Asset and project settings
- Add a Probe Reference Volume to your scene and create/select a Baking Set

2) Add the components
- Create an empty GameObject
- Add `ProbeReferenceVolumeProvider` and `APVLightingBaker` components

3) Create/apply and bake a scenario
- From code (example):
```csharp
using UnityEssentials;

public class BakeCurrentScenario : UnityEngine.MonoBehaviour
{
    public APVLightingBaker Baker;

    public void BakeNow(string name)
    {
        // Adds (if missing), applies, then bakes synchronously
        Baker.AddAndApplyLightingScenario(name);
        Baker.BakeLightingScenario(name, async: false);
    }
}
```
- Scenario naming tip: If you also use Time Of Day’s APV blender, prefer names like `"<Scene> HHmm"` (e.g., `Forest 0630`)

4) Convert meshes to Probe Volumes GI (optional)
- Call `ConvertAllMeshesToProbeVolumesGI()` once to migrate MeshRenderers in all loaded scenes

5) React when APV initializes (optional)
```csharp
ProbeReferenceVolumeProvider.AddListener(() =>
{
    // APV is initialized and a Baking Set exists
    var prv = ProbeReferenceVolumeProvider.Volume;
    // e.g., list scenarios or trigger an initial bake
});
```

## Notes and Limitations
- Baking
  - `BakeLightingScenario` returns false if a bake is already running or if no Baking Set exists
  - `BakeAsync` starts an async bake; progress/cancellation is handled by Unity’s Lightmapping system
- Scenarios
  - Requires `ProbeReferenceVolume.instance.isInitialized` and a non‑null `currentBakingSet`
  - `AddAndApplyLightingScenario` uses `TryAddScenario`; existing names are reused
- Mesh conversion
  - Affects all loaded scenes; review your light probe proxy volumes if needed
- Init listener
  - `ProbeReferenceVolumeProvider.AddListener` stores a single callback until init; call a method that dispatches to multiple handlers if you need more
- Editor scope
  - These helpers are intended for Editor workflows; no runtime components are required

## Files in This Package
- `Editor/APVLightingBaker.cs` – Add/apply/bake scenarios; mesh GI converter
- `Editor/ProbeRerenceVolumeProvider.cs` – APV instance access + initialization listener
- `Editor/UnityEssentials.APV.Editor.asmdef` – Editor assembly definition
- `package.json` – Package manifest metadata

## Tags
unity, hdrp, apv, adaptive-probe-volumes, gi, lighting, bake, scenarios, editor, lightmapping
