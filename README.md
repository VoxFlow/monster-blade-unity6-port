# Monster Blade: Unity 6 port (Windows)

> **Notice.** Monster Blade was developed and published by Nubee Pte Ltd. This repository is an unofficial, non-commercial
> preservation and research project and is not affiliated with or endorsed by the rights holder. See the
> [disclaimer](docs/start/disclaimer.md). The documentation was produced with AI assistance and reviewed by the maintainer; treat
> it as a technical record that may contain errors.

This project rebuilds the Android release of Monster Blade (version 1.3.3, Unity 4.0.1) as a Windows 64-bit player on
Unity 6000.3.8f1 with the Mono scripting backend. The original online services no longer exist; an in-process offline server
answers every request the client makes, so the game runs fully offline from first launch to the end of the campaign and the
god-beast events.

The guiding rule throughout was fidelity to the shipped game: behaviour is reproduced from the original code, data and engine
semantics, and every place where the port differs from the original is recorded with its reason and evidence.

## Playing

Download the Windows build from the **Releases** section of this repository, not from the *Code* button (the source tree does
not contain a playable build). See the [download and run guide](docs/start/download-and-run.md) and the
[PC controls](docs/start/controls-pc.md).

## Building from source

The repository is a complete Unity project. Building it requires Unity 6000.3.8f1 (Windows Build Support, Mono). See
[building from source](docs/release/building-from-source.md).

## Documentation

| Section | Contents |
|---|---|
| [Start here](docs/start/what-is-this.md) | What the project is, download, controls, disclaimer, credits, glossary, troubleshooting |
| [How it works](docs/how-it-works/01-overview.md) | The porting pipeline and each subsystem: assets, offline server, rendering, animation, combat, events, saves, timing, UI, audio |
| [Fidelity](docs/fidelity/shipped-behaviour-kept.md) | Original behaviour that was verified and kept, and every place the port differs |
| [History](docs/history/project-phases.md) | Project phases and claims that were later corrected |
| [Quality](docs/quality/testing-and-validation.md) | How the port was tested, the failure ledger, lessons learned, working with AI agents |
| [Release](docs/release/building-from-source.md) | Build instructions, validation matrix, release notes |
| [Technical](docs/technical/engine-differences.md) | Unity 4 to Unity 6 engine differences, data sources and provenance, the offline server protocol, the compatibility register, the probe catalogue |
| [Reusable notes](docs/knowledge/porting-unity4-to-unity6.md) | Techniques that apply to other Unity 4 era ports |

## Scope

- **Platform:** Windows 10/11, 64-bit. The Android code paths of the original remain in the source but are not built.
- **Content:** the full single-player game as shipped in 1.3.3: tutorial, story campaign, item fusion and evolution, daily events
  and the six god-beast events.
- **Not included:** online features (friends, leaderboards, purchases with real money, push notifications). Their screens remain
  reachable and answer as an empty or offline service would.
