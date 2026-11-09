# What this project is

## The original game

Monster Blade is a third-person action game for mobile devices by Nubee Pte Ltd. The player fights large monsters one at a
time with swipe gestures: swipes are sword strikes, and dedicated buttons dodge, block, trigger a super attack or call a friend's
assist. Progression runs through a story campaign of cities and dungeons, an equipment system with fusion, evolution and element
matchups, rotating daily events and six "god-beast" events with their own bosses.

The game was built with Unity 4.0.1 and depended on Nubee's servers for nearly everything: account creation, the player
profile, inventory, loot, the shop, events, friends and the gameplay parameter tables. When the servers were shut down the game
stopped working.

## What this port is

The port is a Unity 6 project that reproduces the Android release 1.3.3 on Windows:

- **Code.** The game's compiled C# was decompiled and recompiled for Unity 6. Changes to game code are limited to API updates and
  to documented, evidence-backed corrections for engine behaviour that changed between Unity 4 and Unity 6.
- **Assets.** Models, animations, textures, shaders, particles, audio, scenes and the 352 asset bundles were exported from the
  original package and rebuilt in their original structure and under their original names.
- **Server.** An offline server inside the game process answers all 61 service calls the client makes, using the original
  response formats and, where the original server logic is unknown, rules derived from the shipped data tables, the client's own
  displays and its localisation text.
- **Platform.** Windows-specific adapters cover input (mouse and keyboard), save storage, frame pacing, video playback and the
  places where Android-only plugins were called.

## What it is not

- It is not a remake or remaster. Graphics, animation, quality settings and game rules are those of the
  original.
- It is not the original server. The offline server reproduces the client-visible contract. Where the original server's rules
  could not be recovered, the substitute rules are labelled as such in the code and in the
  [compatibility register](../technical/compatibility-register.md).
- It is not affiliated with the rights holder. See the [disclaimer](disclaimer.md).

## Where to go next

- [Download and run](download-and-run.md)
- [How it works: overview](../how-it-works/01-overview.md)
- [Fidelity: every place the port differs](../fidelity/divergences.md)
