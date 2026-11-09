# Where the port differs

The port differs from the shipped game in four ways. The complete list, entry by entry, is the
[compatibility register](../technical/compatibility-register.md).

## 1. Restoring the original where Unity 6 would change it

Most entries exist to keep the original's behaviour, not to change it. Unity 6 differs from Unity 4.0.1 in shaders, legacy
particles, animation data handling, callback order, timing rules, sorting, component lookups and web replies. Each of these was
measured on both engines and bridged so that the game sees what it saw on the original engine. See
[engine differences](../technical/engine-differences.md).

## 2. Platform replacements

Windows has no touch screen, no Android plugins, no Android lifecycle and no Theora video support, and the original servers no
longer exist. These are replaced by:

- mouse and keyboard input delivered as touch;
- inert answers from the Android plugins (no analytics, no platform login, no billing);
- an in-process offline server with the original response formats;
- a save-on-quit that replaces Android's pause-before-close;
- PlayerPrefs in a file instead of the Windows registry;
- a Windows Theora decoder behind the original movie interface;
- a 60 frames-per-second cap matching the original hardware.

## 3. Substitutes for lost server rules

The original server's decisions (loot, experience, event lineups, regeneration intervals, social content) are lost. The offline
server replaces them with rules derived from the shipped tables, the client's own displays and its localisation text. Where a
recording of the original game showed the real outcome, the rule was corrected to match it (for example the tutorial's rewards
and fusion results). These rules are labelled as substitutes in code and documentation and are the main area where the port can
differ from what players experienced.

The client's displays also limit what a substitute may contain. The achievement list draws the diamond and the gold amount at the
same position, so each achievement pays one currency (named-boss achievements: diamonds and experience, as recorded by the
community wiki). The daily-login calendar draws gold and named items only, so the substitute calendar pays gold, in the amounts
named by its coin pictures (500, 750, 1000 and 1250).

## 4. Labelled quality fixes

Three changes correct side effects of much faster hardware: the title fade-in waits for start-up to finish, loading screens stay
readable for at least four and a half seconds, and an animated widget transparency is redrawn when it changes.

## Not changed

Game rules, balance, difficulty, monster behaviour, controls logic, art, audio and the original's own defects. Original behaviours
that look like defects are documented in [shipped behaviour kept](shipped-behaviour-kept.md).
