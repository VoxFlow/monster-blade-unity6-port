# 6. Combat

Combat is entirely the original's code; the port changes none of its rules. This chapter describes the mechanics as they were
established from the code and confirmed in play, because they explain several behaviours that look like defects but are not.

## The basic exchange

- **Strikes.** A swipe across the monster is a strike in the swipe's direction. Plain strikes cost no stamina but slow its
  regeneration; power moves cost stamina.
- **Dodge.** Each monster attack has a dodge window that ends shortly before the hit lands. A dodge in the window on the attack's
  side avoids the hit. A dodge late in the window, while the monster faces the player, is a *perfect dodge* and is followed by a
  counter-attack (riposte) that stuns the monster.
- **Block.** Blockable attacks can be blocked. A block entered within 0.18 seconds of the hit is a *perfect block*: no damage and a
  stun of about four seconds. A normal block reduces the damage.
- **Stamina.** Dodges and blocks cost stamina. A dodge that empties stamina leaves the player exhausted, unable to dodge
  again until part of the stamina has returned.
- **Super attack and friend assist.** A charged super attack plays as a short cutscene. The ally chosen before the fight provides
  an assist whose type depends on the ally's weapon skill: heavy damage, healing, damage or critical buffs, or a stun.

## Damage

Damage is computed from the attacker's attack value, the defender's resistance to the attack's element and the level difference:
`(attack - resistance x 0.125) x (1 + (attacker level - defender level) x 0.1)`, multiplied for critical hits. Resistance comes
from the character's constitution plus each armour piece's armour value weighted by an element table, so armour of the right
element counts double against a given attacker element.

## Monster behaviour worth knowing

- **Hit reactions.** A normal strike interrupts a normal monster but not a boss; bosses react only when stunned or hit by a heavy,
  critical or counter attack.
- **Stuns interrupt attacks.** A stun can cut an attack short before the point where the attack would normally end. The monster
  then stays in its attack state until something changes it. This is the root of several original behaviours listed in
  [shipped behaviour kept](../fidelity/shipped-behaviour-kept.md).
- **Phases.** Some bosses change behaviour at health thresholds, for example the fire wolf, which becomes exhausted at half health
  and enraged at a quarter. See [events and bosses](07-events-and-bosses.md).

## Timing

The original ran at a fixed 60 frames per second on its target phones, and many combat windows are tuned in frames. The port caps
the frame rate at 60 and reproduces the original's timing rules for coroutine waits and delayed events (see
[time and frame pacing](09-time-and-frame-pacing.md)).
