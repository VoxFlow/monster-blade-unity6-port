# Offline server protocol

The contract between the original client and its servers, as reconstructed from the client code, and how the offline server
fulfils it. Facts marked *proven* are read from the client code; the others are labelled.

## Transport

| Item | Fact |
|---|---|
| Services | Three hosts: the game API, a status check and a patch-file host; plus the Nubee platform SDK, an Android Java library |
| Request | `WWW(url)` or `WWW(url, WWWForm)`; some URLs carry identifiers, a timestamp and a checksum |
| Waiting | `ServiceHandle.WaitForQuery` polls the request until done, with a 30-second timeout |
| Errors | No connection is a timeout; a transport error is a connection failure; an empty body is "no results" |
| Result code | `code` is read as a JSON **number**: 200 success, 100 expired key, anything else failure. An optional `ts` sets the server-time offset |
| Values | Almost every value is read as a JSON **string** and parsed; a few fields are read as numbers, booleans or lists |
| Secret key | Calls that need a key first renew it when it has expired |

**Offline cut.** The request object is replaced by one with the same three members (`isDone`, `error`, `text`). In offline mode,
`WaitForQuery` sends nothing, waits one frame and takes the offline server's answer. The platform SDK's desktop branch reports a
successful connection one frame after the request.

## Boot sequence

| Step | Client action | Offline answer |
|---|---|---|
| 1 | Status check | Not in maintenance; the minimum version equals the client's version |
| 2 | Platform SDK connection | Success; local identity |
| 3 | Device and account registration | Success with an empty payload |
| 4 | `GameInitialization` | Game settings, patch information, character, inventory, events, store items and the other sections the client reads |
| 5 | Patch check and download | The cache is pre-seeded with the original files, so every checksum matches and nothing is downloaded |
| 6 | Manager initialisation, inventory, main menu or tutorial | Driven by the client from the received state |

## The new account

| Item | Value | Source |
|---|---|---|
| Level, experience | 1, 0 | Client first-run flow |
| Strength, agility, constitution | 40, 5, 240 | The client's default level-1 player |
| Maximum stamina, Battle Points | 25, 3 | The level table's stamina reward; the user interface's three Battle Point slots |
| Starting equipment | Five base items, item level 0, equipped | Item tables; the client rejects base items with any other level |
| First city and dungeon | The first rows of the city and dungeon tables | Inferred |
| Currencies, bag size, friend limit | Substitute values | No source |

## Game-state calls

| Area | Rule | Evidence class |
|---|---|---|
| Story dungeon start | Monsters from the dungeon's element and species list, the boss last; stamina cost deducted | Inferred from an unused client routine and the dungeon table |
| Event start | Tutorials: the table's monster order; other events: monsters from the event's list, then the boss; Battle Points deducted by the chosen difficulty | Supported for tutorials, inferred otherwise |
| Dungeon end | Gold, experience and unlocks from the tables; chest and drop rules as labelled substitutes; tutorial rewards confirmed by recordings | Mixed; see the [compatibility register](compatibility-register.md) |
| Level-up | Experience counted against the level table; maximum stamina increased; stamina and Battle Points refilled | The refill is documented by players of the original |
| Regeneration | One stamina point per 480 seconds, one Battle Point per 7,200 seconds | Substitute timers; the countdown format is proven |
| Store | The store table's items; diamond purchases honoured, real-money purchases refused | Supported |
| Social | Generated strangers as allies; empty friend lists and leaderboards | Substitute |

## Verification

A contract test sends every service call the client can make and checks that the client's own parser accepts each reply
without errors. Play-throughs of the tutorial, the campaign and the events exercised the calls in their real order.
