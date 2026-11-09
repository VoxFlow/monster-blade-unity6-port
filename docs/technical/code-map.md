# Code map

Where the port's own code sits relative to the original code. Original code is under `Assets/Scripts/Assembly-CSharp` and
`Assets/Plugins/Assembly-CSharp-firstpass`; changes inside it are limited to API updates, platform guards and the calls into the
adapters below, each marked with a comment naming its register entry.

## Port adapters

| Area | Components | Register |
|---|---|---|
| Offline server | Connection replacement, request router, profile, table access, rule modules for game state, loot, events, social, store and promotions, patch-cache seeding | CD-005, CD-006, CD-008, CD-012, CD-028, CD-031, CD-032, CD-036, CD-037, CD-040, CD-042, CD-043 |
| Input | Touch seam, PC input (mouse as one finger, keyboard shortcuts), virtual-finger injector | CD-025, CD-029 |
| Storage and lifecycle | PlayerPrefs file store, save on quit | CD-030, CD-054 |
| Timing | Frame pacing, wait helper with the Unity 4 resume rule, delayed-destroy helper, loading-screen hold, title fade hold | CD-049, CD-053, CD-059, CD-062, CD-063 |
| Runtime library | Original sort algorithm, invariant culture, empty web reply, component lookup on inactive objects | CD-055, CD-056, CD-057, CD-058 |
| Particles | Legacy emitter, size growth, trails, deactivation, start delay and prewarm emulation | CD-017 to CD-020, CD-038, CD-051, CD-052, CD-061 |
| Rendering | Recovered shaders and Unity 4 built-ins, combined texture format, `_Color` read, soft-particle plane, UI layer lookup | CD-013, CD-014, CD-024, CD-033, CD-050 |
| Audio and video | 3D clip flag, Windows Theora decoder behind the original plugin interface | CD-035, CD-047 |
| Bundles | Main-asset fallback, file URIs, one-frame completion | CD-009, CD-010, CD-011 |
| User interface | Menu-bar slide, animated-alpha redraw, splash configuration, clipped-layer material order | CD-026, CD-044, CD-064, CD-065 |
| Platform plugins | Expansion-file check, Android plugin guards | CD-003, CD-004 |

## Editor tools

The bundle builder and the release build script are the only editor code in the release. Development tools (conversion scripts,
verification tools, probes) are not part of the published project.
