# 8. Saves and progression

The original keeps the player's real state on the server and only settings and caches on the device. The port keeps both locally.

## The player profile

The offline server stores the account in one JSON file, `OfflineServer/profile.json` under the game's persistent data folder
(CD-012). It holds level and experience, gold and diamonds, stamina and Battle Points with their refresh times, the inventory
with every item's tier, level and experience, equipped items, unlocked and cleared dungeons and events, tutorial progress and the
daily state. Every service call that changes the state rewrites the file.

## Client settings (PlayerPrefs)

On Android, Unity stores PlayerPrefs in the app's own preferences file. A Windows Unity player stores them in the registry. The
port keeps them in a file instead, `%LOCALAPPDATA%\Nubee Pte Ltd\Monster Blade\PlayerPrefs.json`, written atomically with a
backup and the same typed contract as the original (CD-054). On first run, existing registry values are imported.

## When the game saves

The game saves its local state when Android pauses the app, which always happens before an app is closed. A Windows player has no
pause on exit, so progress made since the last pause was lost. The port runs the game's own pause save on quit (CD-030).

## Tutorial progress

The client asks the server which tutorials are finished and which are in progress. The offline server answers from the profile,
so the tutorial is not repeated after a restart (CD-032).

## Culture and number formats

The original Android runtime selected no culture, so numbers and dates were formatted and parsed with the invariant culture. A
Windows player uses the user's regional settings, which changes number parsing in a region with a decimal comma. The port pins the
invariant culture at start-up (CD-057).
