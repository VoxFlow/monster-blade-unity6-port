# Open questions

Items that are not resolved. Each is either a substitute in the port or an inference that could be confirmed with more evidence.

## Server rules

| Question | Current state | What would resolve it |
|---|---|---|
| Loot and drop rules of story dungeons and events | Substitute rules from the item tables | Recordings of the original's result screens over many runs |
| The original Battle Point and stamina regeneration intervals | Substitute values (two hours, eight minutes) | A record of the original server's game settings |
| The original event prices per difficulty | The client's built-in defaults (Easy 3, Normal 2, Hard 1) | The original server's game settings |
| The warm-up monster pool of the god-beast events | Derived from shipped tables and recordings | More recordings of the original events |
| Starting currencies, bag size and friend limit | Substitute values | A recording of a new account in the original |
| The original event rotation | Events are listed from the shipped event table; the original server's rotation is unknown | The original server's schedule |

## Behaviour

| Question | Current state |
|---|---|
| Whether the fire wolf's rage loop occurred on the original devices | The code is unchanged and produces it; not reproduced on the original engine |
| Why one texture's shipped mipmaps cannot be rebuilt from its top level | Measured as impossible from the top level; the cause (a finer source image) is inferred |
| Trigger timing when attack colliders are re-enabled inside the player | Measured as an engine difference; its effect on each boss attack was counted but not changed |
| GPU output of the original shaders on the original phones | Checked against an analytic model of the shipped GLES programs; no phone capture |
