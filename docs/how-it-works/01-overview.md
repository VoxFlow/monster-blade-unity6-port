# 1. Overview: the porting pipeline

The port turns a built Android game into an editable Unity 6 project in five stages. Each stage keeps the original's names,
identifiers and structure so that the result can be compared against the original at every level.

## Inputs

| Input | Contents |
|---|---|
| Android package (APK) | The Unity 4.0.1 player data: scenes, the main asset files, the managed assemblies (`Assembly-CSharp.dll` and its dependencies) |
| Expansion file (OBB) | 352 asset bundles: characters, monsters, equipment, effects, user-interface atlases, audio |
| Server patch cache | The gameplay parameter tables and localisation that the client downloaded from the server at run time. They do not ship inside the APK or OBB. A copy of a device's cache (patch 1.2.0), found through an archived reverse-engineering forum thread and verified by checksum, was used with its bytes unchanged; see [data sources](../technical/data-sources.md) |

## Stages

1. **Asset export.** The scenes, prefabs, materials, meshes, animation clips, textures and audio are exported into a Unity
   project. Bundle contents are exported as groups that keep each bundle's membership. Object identifiers (GUIDs and file IDs) are
   kept so that references between assets survive unchanged. See [assets and bundles](02-assets-and-bundles.md).
2. **Code recovery.** The shipped managed assemblies are decompiled to C# and recompiled for Unity 6. Edits are limited to API
   updates, platform guards for Android-only plugins, and evidence-backed corrections for engine behaviour that changed between
   Unity 4 and Unity 6. A code-parity check compares the recompiled methods against the shipped IL; the remaining differences are
   compiler artefacts (dead branches, switch and iterator lowering, unused locals), none behavioural.
3. **Engine bridging.** Where Unity 6 behaves differently from Unity 4.0.1 in a way the game depends on (shaders, legacy
   particles, animation data, callback order, timing rules, sorting), the difference is first measured against the original
   engine and then bridged with the smallest faithful change. See [engine differences](../technical/engine-differences.md).
4. **Server replacement.** The client's 61 service calls are answered in-process by an offline server with the original response
   formats. See [the offline server](03-offline-server.md).
5. **Platform adaptation.** Windows replacements for touch input, save storage, video decoding and frame pacing. See the
   individual chapters.

## Project layout

| Folder | Contents |
|---|---|
| `Assets/Scripts/Assembly-CSharp` | The recovered game code, organised as in the shipped assembly |
| `Assets/Plugins` | The recovered first-pass code (plugins and libraries shipped with the game) and the port's early-loading adapters |
| `Assets/Scripts/PortAdapters` | Code written for the port: offline server, platform adapters, engine-behaviour bridges |
| `Assets/Scenes` | The original scenes, in the shipped build order |
| `Assets/Resources` | Assets the game loads by path, under their original paths |
| `Assets/AnimationClip`, `Material`, `Mesh`, `Shader`, `Texture2D`, `GameObject` | Exported original assets referenced by scenes and prefabs, grouped by type as the exporter writes them |
| `Assets/BundleSources` | The contents of the 352 original asset bundles, grouped by bundle |
| `Assets/StreamingAssets` | Files read at run time: the rebuilt bundles (under their original paths), the seeded server patch cache, the offline server's tables, the intro movie |
| `Assets/Editor` | Build scripts, including the bundle builder |

## Guiding rule

Every behaviour is taken from the original's own code, data or engine semantics. When the port differs, the difference is
entered in the [compatibility register](../technical/compatibility-register.md) with the evidence for the original behaviour, the
reason data alone cannot reproduce it, how to reverse it and how it was verified.
