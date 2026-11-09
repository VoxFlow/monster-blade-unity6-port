# 3. The offline server

## The problem

The original client cannot run without Nubee's servers. At start-up it calls `GameInitialization` and waits for the reply; every
menu action that changes the player's state (starting a dungeon, ending it, fusing items, buying, selling, collecting rewards) is
a service call whose reply carries the new state. With the servers gone, the game stops at the first call.

## The approach

The port answers the client's requests inside the game process, at the transport layer the client already uses. The request is
built by the original client code, the reply is parsed by the original client code, and only the network round trip is replaced.

| Aspect | Rule |
|---|---|
| Transport | Requests go through the client's own `Services` and service-call classes. The port hooks the connection type and answers one frame after the request, as a fast network would. |
| Format | The original JSON envelope: a numeric result code (200 for success), values encoded as JSON strings as the original server sent them. |
| Identity | A local account. The Nubee platform SDK calls answer as a logged-in device. |
| Maintenance and version checks | Never in maintenance; the minimum version is the client's own version. |
| State | A JSON profile holds level, experience, currencies, inventory, unlocked and cleared content, event progress and tutorial state. |

The full contract is described in the [offline server protocol](../technical/offline-server-protocol.md).

## Where the rules come from

The client only shows the results of the server's decisions: which items drop, how much experience a fight is worth, which
monsters appear in an event. The original server logic is lost, so each rule in the offline server is classified by its evidence:

| Class | Meaning | Example |
|---|---|---|
| Proven | Taken directly from client code or shipped data | Item prices displayed by the client; costs and limits from shipped settings |
| Proven by recording | Confirmed against video of the original game | The rewards of the first two tutorial fights; the fusion results of the tutorial |
| Community documentation | Reported by players on the fan wiki while the game was live | Gold per battle won; stamina and Battle Points refilled on level-up; named-boss and god-beast set drops |
| Surrogate | Derived from shipped tables, the client's displays and localisation where the original rule is unknown | Loot rolls by chest tier; stamina and Battle Point regeneration intervals; the warm-up monster pool of god-beast events |

Sources are described in [data sources and provenance](../technical/data-sources.md). Every surrogate rule is labelled in the code and in the [compatibility register](../technical/compatibility-register.md)
(CD-008, CD-028, CD-031, CD-036, CD-037, CD-040, CD-042, CD-043). None is presented as original.

## Online features

Leaderboards, the friend list, purchases with real money, push notifications and promotional content were server features. Their
screens remain reachable and behave as an online service with no content would: empty leaderboards, no real-money purchases,
three generated "strangers" a day available as battle allies (the original also offered random players as allies), and the
current event's banners in the promotions window.

## Purchases with in-game currency

Diamond purchases that the original allowed without real money (stamina refills, Battle Point refills, inventory space) are
honoured by the offline server and charged in diamonds at the prices in the game's store table.
