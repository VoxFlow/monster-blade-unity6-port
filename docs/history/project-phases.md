# Project phases

The project ran in the order below. Each phase ended with its findings written into the project's records (the compatibility
register, the list of disproved hypotheses and the failure ledger) before the next phase built on them.

## 1. Census and data availability

The shipped package was inventoried: scenes, assemblies, the 352 asset bundles and every place the code reads data. The key finding
was that the gameplay parameter tables were never in the package; the client downloaded them from the server. Without them the
game cannot be reproduced, so finding an original copy of the server's patch files was the first blocker. Web archives held no
copy of the game's servers. A web search for `web archive monster blade` led to an archived thread on the ZenHax
reverse-engineering forum that linked a copy of a device's download cache; its files verified against the client's own checksums
and provided the tables. See [data sources](../technical/data-sources.md).

## 2. Asset export and code recovery

The scenes and assets were exported with identifiers preserved, the managed code was decompiled and recompiled for Unity 6, and
compiler and exporter artefacts were cleaned up. A code-parity check against the shipped IL confirmed that the recompiled code
does what the shipped code did.

## 3. Boot and the offline server

The player was brought from launch to the title screen and through the first battles. Every service call the client makes was
catalogued, and the offline server was written against the client's own request and response code. A contract test sends each
call and checks that the client's parser accepts the reply.

The player-written fan wiki was read in full and its observations of server-side outcomes (rewards, drops, refills) were recorded
as community evidence for the offline server's rules.

## 4. Rendering and assets

Shaders were recovered from the shipped GLES programs, Unity 4 built-ins were rebuilt, asset bundles were rebuilt under their
original names, and import settings were matched to the original's output.

## 5. Engine semantics

Each Unity API the game calls was triaged for behaviour changes between Unity 4.0.1 and Unity 6. Candidates were measured with
small test programs built from the same source on both engines (oracle probes). Real differences became register entries;
suspected differences that measurement disproved were recorded as such.

## 6. Playability: tutorial, campaign and events

An automated driver played the tutorial, the full story campaign and the god-beast events, answering attacks with the game's own
inputs, to find stalls, exceptions and divergences that only appear deep into the game. Each finding was traced either to the
port (and fixed) or to the original (and documented).

## 7. Comparison with recordings of the original

Recordings of the original game were compared with the port screen by screen. Differences were investigated to a root cause:
several were original behaviour, several were effects of PC hardware speed, and a few were port defects that were fixed.

## 8. Release preparation

The development tooling (driver, probes, test switches) was separated from the game code, the release player was built without
it, and this documentation was written from the project's records.
