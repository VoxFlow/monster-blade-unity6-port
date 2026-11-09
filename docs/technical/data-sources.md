# Data sources and provenance

The port draws on four kinds of source. Every rule and value in the port is traceable to one of them, and the documentation labels
which.

| Source | What it provided | Evidence class |
|---|---|---|
| The shipped game (Android package and expansion file, version 1.3.3) | All code, scenes, models, animation, textures, audio, effects, the user interface and the client-side rules | Proven |
| A device cache of the server's patch files (patch 1.2.0) | The gameplay parameter tables and localisation the client downloaded from the server | Proven (verified by checksum) |
| The Monster Blade fan wiki | Observations of server-side outcomes written by players of the original | Supported (community documentation) |
| Recordings of the original game | Screen-by-screen evidence of rewards, timings, menus and effects | Proven for what is shown |

Where none of these covers a rule, the port uses a labelled substitute (see [where the port differs](../fidelity/divergences.md)).

## The server patch files

### Why they were needed

The client does not ship its gameplay tables. Monsters, attacks, items, equipment, cities, dungeons, events, playlists and most
text are parameter tables that the client downloaded from the publisher's patch server into a local cache after start-up. A full
census of the package confirmed that no table is present in the APK or the expansion file. With the servers shut down, the tables
were the first and largest blocker: without them, the game cannot be reproduced at all.

### How they were found

Web archives held no copy of the game's API or patch hosts. The breakthrough came from a general web search for
`web archive monster blade`, which led to an archived copy (Internet Archive Wayback Machine) of a thread on the ZenHax
reverse-engineering forum (now ResHax). The thread, posted after the game's shutdown, linked a download containing a copy of a
device's download cache with the parameter tables of client patch **1.2.0**. The matching Android package (version 1.3.3) was
obtained separately from an APK archive site and is identical by hash to the base build.

### How they were verified

- The client stores a keyed checksum (HMAC) for every cached file. All 86 table files in the cache verified against their
  checksums, so the bytes are exactly what the server delivered.
- The cache's own file list names patch 1.2.0, the patch version of client 1.3.3.
- The files are used with their bytes unchanged: the port seeds them into the client's cache and answers the client's patch check,
  and the client's own loaders read them (compatibility register CD-006).

### Consequence for the base version

Patch 1.2.0 belongs to client 1.3.3. The earlier client 1.3.1 uses patch 1.1.8, for which no tables survive. This
is why 1.3.3 is the port's base build; 1.3.1 was used only as a reference.

## The fan wiki

The Monster Blade wiki on Fandom was written by players while the game was live. All of its pages were read, and each fact was
classified by whether it changes the port, informs testing, or is context only. Because it records what players observed rather
than the server's code, facts from it are labelled *supported*, not *proven*.

Facts from the wiki that shaped the offline server's rules include:

| Area | Fact used |
|---|---|
| Rewards | Gold is awarded per battle won (confirmed against a dungeon's table values) |
| Level-up | Stamina and Battle Points are refilled on level-up |
| Named bosses | Named bosses drop their own equipment set; their Mystery Treasure holds a piece of that set or an essence |
| God-beast events | The set pieces' level bands per difficulty, the Dragon Points rule, and the events' order for the rotation |
| Stamina | Players' accounts of how often stamina refilled informed the substitute regeneration interval |
| Achievements | Rewards of the named-boss achievements |

Wiki statements that disagreed with shipped data were resolved in favour of the shipped data (for example, regeneration times of
the hydra's heads come from the shipped prefab, not from the wiki's figure).

## Recordings of the original

Video recordings of the original game confirmed or corrected several rules: the rewards and fusion results of the tutorial, the
treasure of the first battle, the gacha price, end-screen values, a dungeon's display name, and the timing of loading screens and
tutorial prompts. Where a recording contradicted a substitute rule, the rule was changed to match the recording.
