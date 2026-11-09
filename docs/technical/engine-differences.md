# Unity 4.0.1 to Unity 6: engine differences that mattered

The differences below were found in this port and confirmed by measurement on both engine versions (see the
[probe catalogue](probe-catalogue.md)). Each is a behaviour that unchanged game code or data relies on. They are likely to affect
other games built with Unity 4.

## Data interpretation

| Area | Unity 4.0.1 | Unity 6 | Bridge |
|---|---|---|---|
| Compressed legacy animation clips | Shipped compressed curves sampled as stored | Re-encoded at every save and build; poses differ between keys (up to tens of degrees on fast clips) | Store the decoded curves uncompressed |
| Single-key animation curves | Not applied in the observed cases | Applied for the whole clip | Remove them from the data |
| Legacy particle components | Supported | Removed; some prefabs lose them silently | Convert to Shuriken and emulate the legacy rules |
| Trail width curves | Linear taper in the curve's parameter | Taper by distance | Reproduce the original taper |
| Built-in shaders of the same name | GLES programs with doubled direct light and gamma ambient | Different lighting (up to 103 levels per channel) | Ship the original programs under the original names |
| Asset bundles | Unity 4 format with a main asset | Not loadable; no main asset in new bundles | Rebuild bundles; load the main asset by name |
| Audio clip "3D" flag | A clip property, applied on any source | Decided by the source (default 2D) | Set the source from the original flag |
| Layer named "UI" | Layer 5 unnamed | Layer 5 is the built-in "UI"; name lookups return the lowest match | Resolve to the game's layer |

## Runtime behaviour

| Area | Unity 4.0.1 | Unity 6 | Bridge |
|---|---|---|---|
| `WaitForSeconds` | Single-precision wake time, resumes on the first frame at or past it | Double-precision comparison; one frame later in 25 of 72 cases | Original rule in a wait helper |
| `Destroy(obj, t)` | Removed before `LateUpdate` of the first frame at or past the time | Sometimes one frame earlier | Original rule in a destroy helper |
| Local `WWW` completion | Never in the frame it was created | Can complete inside the constructor | Wait a frame |
| Empty `WWW` reply | Empty byte array | `null` | Return an empty array |
| `GetComponent(s)InChildren` on an inactive caller | Returns nothing without `includeInactive` | Searches anyway | Original result |
| `List.Sort` / `Array.Sort` | Mono 2.x quicksort | Introspective sort; equal elements ordered differently | Ship the original algorithm |
| Particle prewarm after re-activation | Simulated particles kept | Cleared | Save and restore |
| Particle start delay, prewarm phase, damping | Original frame-based rules | Different formulas | Converted rules |
| `OnWillRenderObject` call order | Newest renderer first | Oldest renderer first; materials made there come out in the other order, so one of two equally deep user-interface layers covers the other | Original order for the user interface's clipped materials |
| Collider re-enabled while overlapping | Trigger reported on the next physics step | Different timing | Assessed per call site |
| Culture | The Android runtime selected none (invariant) | The Windows user's region | Pin the invariant culture |

## Platform behaviour that looks like an engine difference

| Observation | Actual cause |
|---|---|
| Menu bars stop part way | Faster loading makes two identical animations overlap; the animation library behaves the same in both engines |
| A fade is drawn once and never again | Faster frames: the original relied on slow first frames; the code path is identical |
| Saves are lost on exit | Android pauses an app before closing it; Windows does not |
| The frame rate is double the original | The original's requested rate was limited by the phone's display |

## Same in both engines (no change needed)

Legacy animation sampling and cross-fades with uncompressed clips; `OnLevelWasLoaded`; equal-depth camera draw order (enable
order); `LateUpdate` order of re-enabled behaviours; engine manager settings; animation culling; Android Java classes on Windows
(inert stubs).
