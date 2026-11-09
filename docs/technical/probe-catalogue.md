# Probe catalogue

Measurements that compared Unity 4.0.1f2 with Unity 6000.3.8f1, or the original data with the port. Unless stated, each probe was
built from the same source for both engines, run at a fixed 60 frames per second, and compared log against log.

**Setup.** The Unity 4.0.1 side is a small Windows standalone player (Direct3D 9) built with the original engine version; the
Unity 6 side is a Windows player (Direct3D 11/12) or the port itself. The original engine runs on Windows here, not on Android, so
engine-side behaviour is measured directly while GPU output of the original's GLES shaders is checked separately.

| ID | Behaviour | Result | Consequence |
|---|---|---|---|
| R1 | Particle `Play` after a `Stop` in `OnEnable` | Same | None |
| R2 | Particle prewarm sequence (`Simulate`, then `Play`) after re-activation | Differs: Unity 4 keeps the simulated particles across re-activation, Unity 6 clears them | Particles preserved across deactivation (CD-020) |
| R3 | Trigger events when a collider is re-enabled while already overlapping | Differs: Unity 4 reports the overlap on the next physics step | Assessed per call site; attack hit boxes re-enabled inside the player were counted per boss |
| R4 | `OnLevelWasLoaded` with and without a parameter | Same | None |
| R5 | A ground-geyser effect's implosion burst | Differs without emulation: the burst is lost | Legacy emission rules emulated (CD-019, CD-038) |
| R6 | "Coroutine couldn't be started" error on a fire effect | Same error in the original | Kept; original behaviour |
| R7 | A boss intro camera showing the arena from below | Caused by a single-key curve that Unity 6 applies for the whole clip | Single-key curves removed from the data (CD-045) |
| R8 | Bosses vanishing on death | The fade is the same; the vanish is the original's petrified-death path | Kept; original behaviour |
| R9 | "Playlist does not exist" warnings | The playlists are referenced but not defined in the shipped tables | Kept; original behaviour |
| R10 | Audio playback, 3D flag, buffers | 3D clip flag lost on import; buffer count differs | 3D restored (CD-047); buffer count residual (CD-048) |
| R11 | Legacy animation sampling, cross-fades, queued clips, blends, layers | Same within 0.0016 degrees for uncompressed clips | No engine change; compressed clips handled in data (CD-022) |
| R12 | Engine manager settings (time, physics, quality) against the shipped values | Same | None |
| R13 | Converted legacy particles against the original legacy system | Matches after emulation | Validates CD-038 |
| R14 | Particle damping, bursts and start randoms | Damping formula differs | Converted formula (CD-051, CD-052) |
| R15 | Delayed `Destroy` timing | Differs in 6 of 20 cases (one frame early in Unity 6) | Unity 4 rule applied (CD-053) |
| R16 | Cross-fade between clips that animate different bones | Same with uncompressed clips | No change |
| R17 | Runtime library: sorting, dictionaries, string and number formatting, maths | Sort order of equal elements differs; the rest the same | Original sort algorithm (CD-055) |
| R18 | `WWW` replies (status codes, encodings, empty replies, redirects) | Empty reply differs (empty array versus null) | Unity 4 result (CD-058) |
| R19 | Particle first emission, prewarm phase and start delay | Start delay and prewarm phase differ | Unity 4 timing (CD-061) |
| R20 | An animation clip driving a user-interface widget's private colour field; first geometry fill | Same in both engines; the visible difference on PC comes from hardware speed | Redraw on change (CD-064) |
| R21 | `LateUpdate` order of behaviours disabled in `Awake` and re-enabled by a sibling | Same | None |
| R22 | Draw order of equally deep transparent renderers; `Start` order of objects instantiated in one frame; `OnWillRenderObject` call order | Tie-break and `Start` order the same; `OnWillRenderObject` order reversed | Original order for clipped materials (CD-065) |

## Other censuses

| Census | Purpose |
|---|---|
| Coroutine wait rule (72 cases) | Established the exact `WaitForSeconds` resume rule of each engine (CD-059) |
| Component lookup on inactive objects | `GetComponent(s)InChildren` on an inactive caller (CD-056) |
| Equal-depth camera order | Draw order follows enable order in both engines |
| Locale | The original Android runtime selected no culture (CD-057) |
| Service field census | Every service call sent and its reply parsed by the client, to check the offline server's contract |
