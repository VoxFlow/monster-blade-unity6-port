# Glossary

| Term | Meaning |
|---|---|
| **1.3.3** | The Android release of Monster Blade used as the base for the port (version code 21). An earlier release, 1.3.1, was used for comparison only. |
| **Asset bundle** | A Unity file that packs assets for loading at run time. The original ships 352 of them inside its expansion file (OBB). Unity 6 cannot read Unity 4 bundles, so they are rebuilt from the exported sources. |
| **AssetRipper** | A tool that exports the scenes, prefabs and assets of a built Unity game into an editable Unity project. |
| **Battle Points** | The currency spent to enter events (named `shield` in the code). Easy, Normal and Hard cost different amounts; the maximum is 3. In the port one point regenerates every two hours, a substitute value because the original server setting is lost (CD-008). |
| **Compatibility register** | The list of every place the port differs from the original, with reason, evidence and status (`CD-` numbers). See [the register](../technical/compatibility-register.md). |
| **Divergence class** | The category of a difference: faithful to the source, platform adapter, surrogate data, optional enhancement or debug only. |
| **Engine semantics** | Behaviour that belongs to the engine rather than to the game code, such as the order in which callbacks run or how an animation blend is computed. Several of these changed between Unity 4 and Unity 6. |
| **God-beast event** | One of the six special events (Aralez, Nhang, Hydra, Vahagn, Anahit, Aramaz) with an end boss and its own equipment set. |
| **Legacy Animation** | Unity's original animation system (`Animation` component), used by the game for all characters. |
| **Legacy particles** | Unity's original particle system, removed in Unity 5; the game's effects were converted to the current system with the legacy rules emulated. |
| **NGUI** | The user interface library shipped with the game. The original version is kept. |
| **Offline server** | The in-process replacement for Nubee's servers. It answers the client's requests with the original response formats. |
| **Oracle build** | A small test program built with the original engine version (Unity 4.0.1) to measure how the original engine behaves, for comparison with Unity 6. |
| **Port adapter** | Code written for the port that bridges a platform or engine difference without changing game logic. |
| **Probe** | A purpose-built measurement run that records one behaviour in both engines, or in the original and the port. |
| **Shuriken** | Unity's current particle system. |
| **Surrogate rule** | A rule in the offline server where the original server's logic is unknown. It is derived from shipped data, the client's displays and localisation, and labelled as a substitute. |
| **Tier, grade, skill** | Equipment attributes: tier 1-5, grade (Lowest to VeryHigh) and a skill variant (HILL health, GHOUL life steal, BRUTE damage, ROGUE critical, SOLDIER armour). |
