# Download and run

## Requirements

| Item | Requirement |
|---|---|
| Operating system | Windows 10 or 11, 64-bit |
| Graphics | Direct3D 11 or 12 capable GPU |
| Disk space | About 2 GB |
| Display | Any resolution; a 60 Hz refresh rate gives the most faithful motion (see [troubleshooting](troubleshooting.md)) |
| Network | Not required. The game never contacts a server. |

## Installing

1. Open the **Releases** section of the repository and download the Windows archive of the latest release.
2. Extract the archive to a folder of your choice. Avoid protected locations such as `C:\Program Files`.
3. Run `Monster Blade.exe`.

The archive contains only the Unity player: `Monster Blade.exe`, `UnityPlayer.dll`, `UnityCrashHandler64.exe` and the
`Monster Blade_Data`, `MonoBleedingEdge` and `D3D12` folders. There is no installer and nothing is written outside the save
folders listed below.

## First launch

The first launch shows the publisher splash, the title screen and the account setup exactly as on the phone. The offline server
creates a local profile, and the tutorial starts as it did in the original.

## Save data

| Data | Location |
|---|---|
| Player profile (the offline server's account: level, inventory, currencies, progress) | `%USERPROFILE%\AppData\LocalLow\Nubee Pte Ltd\Monster Blade\OfflineServer\profile.json` |
| Client settings (the original PlayerPrefs) | `%LOCALAPPDATA%\Nubee Pte Ltd\Monster Blade\PlayerPrefs.json` |
| Downloaded-content cache (regenerated automatically) | `%LOCALAPPDATA%\Temp\Nubee Pte Ltd\Monster Blade` |

To start over, close the game and delete the first two items. Back them up first if you want to keep the progress.

## Uninstalling

Delete the extracted folder and, if desired, the three save locations above.
