# Release notes

## First release

**Base:** Monster Blade 1.3.3 for Android (Unity 4.0.1).
**Target:** Windows 10/11, 64-bit, Unity 6000.3.8f1, Mono.

### Included

- The complete single-player game: tutorial, story campaign, equipment, fusion and evolution, daily events and the six
  god-beast events.
- An in-process offline server replacing the original online services.
- Mouse and keyboard controls.

### Known differences from the original

- Server-side rules that could not be recovered are replaced by labelled substitute rules (loot, experience, event lineups,
  regeneration intervals). See [where the port differs](../fidelity/divergences.md).
- Online features (friends, leaderboards, real-money purchases, notifications) are unavailable.
- On displays whose refresh rate is not a multiple of 60 Hz, motion is slightly uneven; see
  [troubleshooting](../start/troubleshooting.md).

### Known original behaviours

See [shipped behaviour kept](../fidelity/shipped-behaviour-kept.md).

## Reporting an issue

Open an issue describing what happened, what the original does (a recording of the original is the best evidence) and attach the
`Player.log` from the session.
