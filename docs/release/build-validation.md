# Build validation

Checks run on each release build before publication.

| Check | Method | Pass condition |
|---|---|---|
| Clean-checkout build | Clone into a new folder with Git LFS, open in Unity 6000.3.8f1, build | Build succeeds with no compile errors |
| Shipped folder content | List every file in the player folder | Only the Unity player layout: `Monster Blade.exe`, `UnityPlayer.dll`, `UnityCrashHandler64.exe`, `Monster Blade_Data`, `MonoBleedingEdge`, `D3D12` |
| No development tooling | Search the player's managed assemblies for test switches, test log tags and tooling class names | No matches |
| No personal or secret data | Scan the source tree and the player for user paths, e-mail addresses, keys and tokens | No matches |
| First launch | Start with no save data | Splash, title, account setup and the tutorial's first battle |
| Progression | Play the tutorial and the first city | No exceptions in the player log; saves persist across restarts |
| Events | Enter a god-beast event | Battle Points deducted, warm-up and boss battles load, rewards granted |
| Shutdown | Quit from the title and from a menu | Progress saved; reopening resumes |

## Engine-equivalence evidence

The engine-behaviour bridges in the release were each verified against Unity 4.0.1 during development (see the
[probe catalogue](../technical/probe-catalogue.md)). They are not re-measured for each release unless the engine version changes.
