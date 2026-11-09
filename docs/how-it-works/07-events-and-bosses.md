# 7. Events and bosses

## God-beast events

Six events each end in a god beast with its own arena, intro and death cutscenes and equipment set:

| Event | God beast | Element | Notable mechanics |
|---|---|---|---|
| Aralez | Fire wolf | Fire | Becomes exhausted at half health and enraged at a quarter: the rage restores full health and doubles defence for 60 seconds, then health drops back to a quarter |
| Nhang | Winged chicken (wyvern) | Earth | Flame attacks; a fireball that is ignored during the first half of a dodge roll |
| Hydra | Three-headed hydra | Water | The body takes damage only while all three heads are cut off; heads regrow |
| Vahagn | Land dragon | Lightning | Burrows underground and attacks with geysers placed where the player stands and where a roll would land |
| Anahit | Stone dragon | Metal | A stone gaze that petrifies the player, or, when perfect-blocked, petrifies the dragon |
| Aramaz | Epic dragon | Metal | Ground, flight and final phases; in flight only reflected fireballs damage him; the final phase is a 60-second fall |

An event is entered at Easy, Normal or Hard, at a cost in Battle Points. The boss's level follows the player's level, higher on
Hard. Each event has a warm-up battle before the boss.

The original server decided an event's battles, rewards and drops. The offline server builds them from the shipped event and item
tables and from recordings of the original; the rules it uses where the original is unknown are labelled surrogate (CD-040).

## Aramaz in detail

Aramaz is the most complex fight and shows how much of the original's design lives in timers and phase changes:

1. **Ground phase.** Melee attacks, a ground flamethrower that sweeps across the arena, and fireballs.
2. **Flight.** On a rest timer he takes off. In the air he is out of melee reach; only fireballs blocked back at him at the right
   moment damage him, and a set fraction of damage brings him down again.
3. **Final phase.** He falls from the sky in a dedicated scene with 25 % of his maximum health. The player has 60 seconds to bring
   that to zero while blocking his glide attacks. When the timer runs out, the game sets the player's health to zero.

The take-off is driven by an animation event in the take-off clip. A strike landing in the first frames of the take-off replaces
the clip before the event fires, and the player's controls stay disabled for the whole flight. This is original behaviour; see
[shipped behaviour kept](../fidelity/shipped-behaviour-kept.md).

## Story bosses and named bosses

Each city's last dungeon ends in a boss with an intro cutscene. Ten named bosses carry equipment sets that do not appear in any
regular drop table (every piece has a drop probability of zero in the shipped item table); only the server awarded them. The
offline server awards them from the named-boss fights.

## Daily events

Rotating daily events with their own monsters and rewards are listed by the server each day. The offline server lists them from
the shipped event table.
