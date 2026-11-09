# Corrected claims

Claims that were believed during the project and later disproved by evidence. They are recorded so that the same wrong turns are
not taken again, and because the corrections often say more about the original than the claims did.

## Reported defects that were original behaviour

| Claim | What disproved it |
|---|---|
| The strong camera shake in some cutscenes is a port defect | The shake values in the port's prefabs equal the original's field by field, and the measured motion matches the authored amplitudes |
| Monsters freezing until hit again is a port defect | Every freeze occurred during a super attack after a stun, a sequence the original code produces |
| A Hard event costs 3 Battle Points | The stored count decreases by 1; only the client's display subtracts 3, from a field the original never fills |
| The "FX_Fire is inactive" error is a port bug | It occurs on the original engine with the original code |
| Bosses vanishing on death is a broken fade | The fade is identical in both engines; the vanish is the original's petrified-death path |
| Anahit vanishes only when petrified at the moment of death | Any earlier petrification in the fight removes her death cutscene |

## Engine assumptions that measurement disproved

| Claim | What disproved it |
|---|---|
| Unity 6's legacy animation blends bones differently | The measured difference was an artefact of the comparison metric; the correct metric gives 0.0016 degrees |
| Legacy cross-fades snap differently at the end of the fade in Unity 6 | The case that appeared to show it used a compressed clip that the test build itself re-encoded; with uncompressed clips the fades are identical |
| Unity 6 runs PhysX 5 | The player log of the Unity 6 build names PhysX 4.1.2 |
| Equal-depth cameras draw in the order of the camera list | Draw order is enable order, in both engines |
| The missing menu bars are an engine difference | The tween library stops the bars at the same point in both engines; the difference is load speed |
| A probe built from the same source gives both engines the same inputs | The Unity 4 compiler read a source file without byte-order mark as ANSI and folded `-0f` to `+0`; probe sources must be ASCII and inputs must be checked first |
| A blank comparison image means both engines agree | A positive control was blank too; every comparison needs a control that must differ |
| Android Java calls throw on a Windows player | Unity's Android Java classes are inert stubs on Windows |

## Data assumptions that were wrong

| Claim | What disproved it |
|---|---|
| The gameplay tables ship inside the APK or OBB | No table appears in any member of either package; the client reads them only from its download cache |
| Every "database" is server-only | The character element database ships in an asset bundle |
| The named bosses' equipment drops from the regular chest pool | All 250 pieces have a drop probability of zero in the shipped table; only the server awarded them |
| Unity 6 renames every main asset after its file | Native assets keep their stored names; only imported files take the file name |
| A Resources path listed twice is a duplicated object | Most such paths are a model and a prefab with the same path |

## Mistakes in the port's own analysis

| Claim | What disproved it |
|---|---|
| A full-damage attack after a perfect block killed the player in Aramaz's final phase | The "damage" was the final-phase timer expiring, which sets the player's health to zero; the real issue was too little damage within 60 seconds |
| The original's mipmaps for one texture were proven to be built from an 8-bit source | Only the impossibility of rebuilding them from the top level was measured; the cause remains an inference |
| Shipped data could be read from the reference copy of an earlier release | That copy was version 1.3.1. Checks were redone on 1.3.3, and every tool now names the build it reads |
