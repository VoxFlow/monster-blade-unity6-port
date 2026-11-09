# Shipped behaviour kept

Several behaviours of the port look like defects but are the original game's own behaviour. Each was reported during testing,
investigated against the original's code, data or engine, and kept. They are listed here so they are not "fixed" by mistake.

| Behaviour | Cause in the original | How it was established |
|---|---|---|
| **Strong camera shake** in the friend-assist stun and attack cutscenes and in some boss entrances | The cutscene prefabs contain long camera-shake components: the stun and attack assists have a 20-second, low-amplitude rumble under two strong jolts | Every shake value and object compared field by field with the original prefabs; the shake measured frame by frame at real speed and found to match the authored amplitudes and timing |
| **Monsters freeze in their last hit pose** for up to about 1.4 seconds during a super attack | Each super hit plays the monster's heavy hit reaction, which ends before the next super hit and holds its last frame; the monster's AI is disabled during the super and the stun has already ended, so nothing plays a new animation until the next hit | A probe that detects stalled animations and frozen skeletons, run over three full campaigns; every non-design freeze occurred during a super after a stun |
| **The fire wolf stops attacking** in rare cases | A stun can cut the wolf's attack before the attack's normal end. Its state stays "attacking", the rage transition waits for it to become idle, times out after 20 seconds and restarts indefinitely | Traced in the unchanged AI code and reproduced in testing; not yet compared on the original engine, so classed as very likely original. Striking the wolf ends it, as a player would |
| **Battle Points drop by 3** when a Hard event starts, although Hard costs 1 | The client subtracts the price of the event's own difficulty field from its displayed count. That field is never read from the server's event list and is always 0 (Easy, price 3). The server deducts the correct amount, and the display takes the server's value with the next update | Code reading; the stored count after one Hard event decreased by exactly 1 |
| **Anahit has no death cutscene** if she was petrified earlier in the fight | Her first petrification clears her death-cutscene flag before saving it, so restoring it later restores "off" for the rest of the battle | Code reading and a probe on the original engine |
| **Bosses vanish on death** without the usual fade in some cases | The petrified-death path of the original code; the fade itself is identical in both engines | Probe on the original engine |
| **Controls lock for Aramaz's whole flight** after a strike lands as he takes off | The flight phase disables the player's controls, and only an animation event in the take-off clip re-enables them. A strike landing in the take-off's first frames plays a hit reaction that replaces the take-off clip before the event fires | Code reading; the clip replacement measured in testing |
| **"Coroutine couldn't be started because the game object 'FX_Fire' is inactive"** in the log | The original code starts a coroutine on an inactive object | Reproduced on the original engine |
| **"Playlist X does not exist"** warnings in the log | The shipped data references playlists that 1.3.3 does not define | Checked against the shipped bytes |

## Why these are kept

The port's rule is to reproduce the shipped game. Changing an original behaviour is a separate decision with its own entry in the
[compatibility register](../technical/compatibility-register.md), and none of the behaviours above has been changed.
