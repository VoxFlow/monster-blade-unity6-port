# 5. Animation and effects

## Character animation

All characters use Unity's legacy `Animation` component, driven by the game's `NBAnimation` wrapper with cross-fades and queued
clips. The legacy animation system still exists in Unity 6, and measurements against the original engine show it behaves the same:

- Sampling of uncompressed clips: identical within 0.0016 degrees per bone.
- Cross-fades, including bones that only one of the two clips animates: identical.

The differences that did exist were in the data, not the engine: compressed clips are re-encoded by Unity 6 at build time and
single-key curves are applied differently. Both are corrected in the clip assets (see [assets and bundles](02-assets-and-bundles.md)).

Animation culling settings were compared and found equal; the planned adjustment was retired (CD-016).

## Particle effects

The game's effects were made with Unity's legacy particle system (emitters, animators and renderers), which was removed in
Unity 5. They are converted to the current particle system (Shuriken) with the legacy rules emulated where the two systems
differ (CD-038):

| Legacy behaviour | Emulation | Register |
|---|---|---|
| Size growth over the particle's life | The same rule applied per particle | CD-017 |
| Trails rebuilt per camera with linear tapering | Reproduced; the game's own trail toggles still work | CD-018 |
| One-shot emitters that destroy themselves | The emitting component keeps ownership of emission and destruction | CD-019 |
| Particles surviving deactivation of their object | Saved before the game deactivates a system and restored after | CD-020 |
| Velocity damping per frame | Converted to the current system's formula: `d6 = 1 - (1 - d4)^2` at 60 fps | CD-051 |
| Random start values, bursts and off-screen simulation | Matched to the original's behaviour | CD-052 |
| Start delay and prewarm | Reproduced from a numeric emulation of the original's countdown | CD-061 |

Each rule was measured on the original engine with a test program built from the same source as the port's measurement.

## Delayed destruction

Effects and temporary objects are often destroyed with a delay (`Destroy(obj, t)`). Unity 4.0.1 removed such an object before
`LateUpdate` of the first frame whose time had reached the delay; Unity 6 sometimes removes it one frame earlier. The port
schedules delayed destruction with the original rule (CD-053).
