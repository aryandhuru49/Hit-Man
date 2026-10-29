# Exit Clause

A cinematic third person stealth-action story game (Unity 6 HDRP) about a retired hitman whose agency
never lets anyone retire. It is also the target of a game security portfolio: v1 is insecure on purpose
(see [SECURITY_TARGETS.md](SECURITY_TARGETS.md)).

## Open and play
1. Unity Hub > **Projects > Add > Add project from disk** > select `D:\Unity\Projects\ExitClause` > open with 6000.6.7f1.
2. If the **HDRP Wizard** window appears, click **Fix All** (safe; it only fixes pipeline settings), then close it.
3. Open `Assets/_Game/Scenes/MainMenu.unity` and press Play. Any location scene can also be played directly.
4. If scenes are ever missing or broken: menu **Exit Clause > Build Everything**.

## Controls
| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move / look | WASD / mouse | Left / right stick |
| Sprint, crouch, jump/vault | Shift, C, Space | L3, B, A |
| Interact / enter-exit car | E | X |
| Aim (lock-on), fire, reload | RMB, LMB, R | LT, RT, Y |
| Switch weapon | 1 2 3 / scroll | D-pad right |
| Melee / takedown | F | R3 |
| Cover | Q | RB |
| Throw gadget, switch gadget | G, H | LB, D-pad left |
| Rage (after the Interlude) | V | D-pad up |
| Journal / suspect board | J or Tab | Select |
| Survey camera | T | D-pad down |
| Car: handbrake, look behind | Space, B | A, R3 |
| Skip cutscene | Enter | A |
| Pause | Esc | Start |

## How to test each system in the Editor
- **Camera**: play `Prologue_Prague`, walk without touching the mouse and the camera swings behind you; orbit with the mouse and let go to see it ease back. Hold RMB near a guard to lock on (toggle Free Aim in Settings). Press T for the high-angle view. Tune live in `Assets/_Game/Data/Camera/CameraSettings.asset`.
- **Stealth**: in the Prologue courtyard, watch the vision cones (white > yellow > red). Crouch to slow detection, throw a coin (G) to pull a guard away, take the side-door guard down from behind (F), then search the body (E) to wear his uniform.
- **Combat**: kill Sorel to trigger the hotel guards. Use cover (Q), aim (RMB), shoot, reload (R). Ammo is shown bottom-right.
- **Vehicles**: at the Prologue extraction car press E, drive with WASD, Space handbrake, B look behind. `Act1_CoastRoad` tests the escape chase.
- **Journal and clues**: `Act0_Riviera`. Observe (E) the grey-jacket man in both aisles and the parking lot, then press J and pin clues on the Suspect Board. Sleep in the villa bed to advance days.
- **Economy**: the villa laptop (pension portal) and the market/garage shops. Cash is top-left.
- **Hacking**: `Act2_Safehouse`. Use the phones on the table. PINs use smudge marks + "digits in place" feedback, messages need the right cipher shift, and the Orders tab needs the auth code from Elias's second message.
- **Rage**: `Interlude_Harbor`. After reaching pier nine the meter fills; press V. Kills while raging leave red evidence marks (see Journal > Status).
- **Save/load**: Esc > Save Game, then Load Game. Files: Exit Clause > Open Save Folder.
- **Whole story, automated**: Exit Clause > Run Smoke Test (auto-plays every objective to the Ending; report in `Logs/smoke_report.txt`).

## Docs
- [CLAUDE.md](CLAUDE.md): architecture and conventions
- [SECURITY_TARGETS.md](SECURITY_TARGETS.md): every intentional weakness and its hardening
- [ASSETS.md](ASSETS.md): which free assets to download and how to swap them in
