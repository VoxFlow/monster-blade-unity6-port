# Compatibility register

Every place where the port's code or data differs from the shipped game, with its class and status. The identifiers (`CD-`) are
referenced throughout the documentation and in code comments.

## Classes

| Class | Meaning |
|---|---|
| Source-faithful | Reproduces the original's behaviour where Unity 6 or the target platform would otherwise change it |
| Platform adapter | Replaces something that cannot exist on Windows (Android plugins, touch, the dead servers) with the closest equivalent |
| Surrogate | A rule or value of the original server that is lost; derived from shipped data and labelled as a substitute |
| Quality fix | Corrects an effect of much faster hardware where the original relied on slow devices; labelled, not original |
| Enhancement | An optional addition that is off by default |

Test and diagnostic tooling used during development (register entries CD-007, CD-015 and CD-060) is not part of the release and
is not listed.

## Entries

| ID | Difference | Class |
|---|---|---|
| CD-001 | Delegates bound to methods with optional parameters pass the declared default, as the original compiler's binding did | Source-faithful |
| CD-002 | A Unity 4 `Instantiate` overload binding kept in unreachable code | Source-faithful |
| CD-003 | The expansion-file check reports the data as present; no download | Platform adapter |
| CD-004 | Calls into Android-only Java plugins (platform SDK, analytics, offer wall, web view, billing) answer as an unavailable service | Platform adapter |
| CD-005 | The game's service calls are answered by the in-process offline server with the original formats | Platform adapter |
| CD-006 | The server patch cache is seeded with the original files, bytes unchanged, and the patch check answered | Platform adapter |
| CD-008 | Server game settings the client needs and the original server sent (for example regeneration intervals) | Surrogate |
| CD-009 | Bundles: when Unity 6 has no main asset, the shipped main asset is loaded by its original lookup name | Source-faithful |
| CD-010 | Local bundle paths turned into well-formed `file://` URIs | Platform adapter |
| CD-011 | Bundle requests complete no earlier than the frame after they are made, as on the original player | Source-faithful |
| CD-012 | A local account profile replaces the server account | Platform adapter, with surrogate rules |
| CD-013 | The combined character texture uses RGB24 where Direct3D 11 cannot create ETC1 | Platform adapter |
| CD-014 | Reading a missing `_Color` property returns the Unity 4 value without an error | Source-faithful |
| CD-016 | Animation culling: compared, no difference, retired | Retired |
| CD-017 | Legacy particle size growth reproduced | Source-faithful |
| CD-018 | Legacy trails reproduced | Source-faithful |
| CD-019 | Legacy one-shot emitters reproduced | Source-faithful |
| CD-020 | Particles survive the game deactivating and re-activating their system | Source-faithful |
| CD-021 | A `RequireComponent` attribute left out where the shipped data lacks the component | Source-faithful |
| CD-022 | Compressed legacy clips stored with their decoded curves | Source-faithful |
| CD-023 | NGUI's Direct3D 9 half-pixel offset disabled, as on Android | Platform adapter |
| CD-024 | Unity 4 built-in shaders and default materials rebuilt from the shipped programs | Source-faithful |
| CD-025 | A touch input seam so mouse input reaches the game as touch | Platform adapter |
| CD-026 | An identical menu-bar slide already running is left to finish | Platform adapter |
| CD-027 | Unity API calls moved out of field initializers, same values on first use | Platform adapter |
| CD-028 | Gacha (treasure) answers for the tutorial | Surrogate |
| CD-029 | PC mouse and keyboard controls | Platform adapter |
| CD-030 | The game's pause save runs on quit | Platform adapter |
| CD-031 | Loot and fusion results; tutorial rewards confirmed against recordings of the original | Surrogate |
| CD-032 | Tutorial progress kept by the offline server | Source-faithful |
| CD-033 | The soft-particle ground plane built from the GPU projection matrix | Platform adapter |
| CD-034 | Unity 4's built-in sphere, cylinder and default particle stored as project assets | Source-faithful |
| CD-035 | A Windows Theora decoder behind the original movie plugin's interface | Platform adapter |
| CD-036 | Offline server answers for screens beyond the tutorial (shop, sell, retry, social, promotions) | Platform adapter |
| CD-037 | Game rules of the server beyond the tutorial, built from shipped tables, displays and localisation | Surrogate |
| CD-038 | Legacy particle systems converted to Shuriken with the legacy rules emulated | Source-faithful |
| CD-039 | Texture, mipmap and audio import matched to the original build's results | Source-faithful |
| CD-040 | God-beast event battles, rewards and drops | Surrogate |
| CD-041 | Optional "Enhanced" quality level (full-resolution textures, 4x MSAA, vertical sync); development builds only, not in the release | Enhancement |
| CD-042 | Promotions window shows the current event's banners | Surrogate |
| CD-043 | Social features without the social servers: generated strangers as allies, gift tickets at set levels | Surrogate |
| CD-044 | The publisher splash reproduced with Unity 6's splash screen | Source-faithful |
| CD-045 | Single-key animation curves removed from the source clips | Source-faithful |
| CD-046 | Unity's development console disabled | Source-faithful |
| CD-047 | Audio clips flagged 3D in Unity 4 play as 3D | Source-faithful |
| CD-048 | Audio buffer size kept; the count cannot be set in Unity 6 | Residual difference |
| CD-049 | Frame rate capped at 60, synchronised to 60-multiple displays | Platform adapter |
| CD-050 | Layer lookups for "UI" resolve the game's own layer | Source-faithful |
| CD-051 | Particle velocity damping converted to the current formula | Source-faithful |
| CD-052 | Particle start randoms, bursts and off-screen simulation matched | Source-faithful |
| CD-053 | Delayed `Destroy` follows the Unity 4 frame rule | Source-faithful |
| CD-054 | PlayerPrefs stored in a file instead of the registry | Platform adapter |
| CD-055 | Sorting uses the original runtime's quicksort, preserving the order of equal elements | Source-faithful |
| CD-056 | `GetComponent(s)InChildren` on an inactive caller returns the Unity 4 result | Source-faithful |
| CD-057 | The invariant culture is pinned, as on the original Android runtime | Platform adapter |
| CD-058 | An empty web reply returns an empty byte array, as in Unity 4 | Source-faithful |
| CD-059 | `WaitForSeconds` resumes on the Unity 4 frame | Source-faithful |
| CD-061 | Particle start delay and prewarm follow the Unity 4 timing | Source-faithful |
| CD-062 | The title fade-in waits for the offline server's start-up reply | Quality fix |
| CD-063 | Loading-screen tips stay in clear view at least four and a half seconds | Quality fix |
| CD-064 | A widget whose transparency is animated is redrawn when the value changes | Quality fix |
| CD-065 | Clipped user-interface layers get their materials in Unity 4's order, so overlapping layers draw as in the original | Source-faithful |

Each entry in the development register also records the evidence for the original behaviour, why data alone could not reproduce
it, its cost, the alternatives tested, the acceptance tests and how to reverse it. Summaries of that evidence appear in the
[How it works](../how-it-works/01-overview.md) chapters and in [engine differences](engine-differences.md).
