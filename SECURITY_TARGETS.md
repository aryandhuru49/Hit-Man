# Exit Clause: Security Targets

Exit Clause is the target of a game-security portfolio project. **The first version of every system is insecure on purpose.** This file lists each intentional weakness, the attack it enables, and the hardening you can apply later.

Search the code for `INSECURE` to find most of them. The comments say `INSECURE BY DESIGN` or just `INSECURE`.

> Rule: do not fix a weakness quietly. Attack it first and record the result (screenshots, Cheat Engine tables, diffs). Then harden it and attack it again. Note the before and after in this file.

## Attack classes

| Code | Attack | Typical tools |
|---|---|---|
| **MEM** | Memory editing | Cheat Engine (scan, freeze, pointer scan, Mono dissector) |
| **SAVE** | Save tampering | Any text editor, a JSON diff, a hex editor |
| **DEC** | Decompiling and patching | dnSpy / dnSpyEx, ILSpy, dotPeek, AssetRipper, UABE |
| **DATA** | Data-file tampering | A text editor on `StreamingAssets`, asset rippers on `resources.assets` |
| **NET** | Traffic tampering | Fiddler, mitmproxy, Burp Suite (see the [Traffic tampering](#traffic-tampering-net) section: there is no network code yet) |

## Hardening options

| Code | Hardening |
|---|---|
| **IL2CPP** | Switch the scripting backend from Mono to IL2CPP, strip the engine code, and do not ship `global-metadata.dat` in plain form |
| **OBF** | Value obfuscation: XOR or keyed "obscured" types for hot values, decoy copies, and checksums across copies |
| **SIGN** | Save signing and/or encryption: HMAC-SHA256 over the payload, AES-GCM, and a versioned schema with range validation on load |
| **INT** | Integrity checks: hash the shipped data files and assemblies, detect a debugger or a known cheat process, and run invariant checks (balance never below 0, ammo never above the max) |
| **SRV** | Server validation: move authority (prices, rewards, balances, hacking answers) to a backend that the client asks and never tells |

## Weakness table

| # | Weakness | File / class | Attacks | What the attacker gets | Hardening |
|---|---|---|---|---|---|
| 1 | The whole game state is one plain public object: `GameSession.State` holds cash, pension, health, rage, ammo, inventory and story flags | `Assets/_Game/Scripts/Systems/GameSession.cs` `GameSession`; `Systems/GameState.cs` `GameState`, `PlayerState`, `EconomyState` | MEM, DEC | Find any value with a first or next scan in Cheat Engine. Because the fields have stable names, the Mono dissector shows them straight away | OBF on hot fields, INT invariant checks, IL2CPP, SRV for the economy |
| 2 | Saves are unsigned, unencrypted, pretty-printed JSON that is loaded as-is | `Systems/SaveSystem.cs` `SaveSystem` (`persistentDataPath/saves/slotN.json`, `JsonUtility.FromJson`) | SAVE | Edit `economy.cash`, `pensionBalance`, `player.health`, `ammo[]`, `inventory.items`, `story.flags`, `story.counters` (for example `evidence_collected`), or `progress.sceneName` to skip missions or unlock any ending | SIGN (HMAC plus AES-GCM), schema and range validation, cloud saves under SRV |
| 3 | No limits or validation on load: any `sceneName`, `missionId` or `objectiveIndex` is trusted | `SaveSystem.LoadAndContinue`, `SceneMissionDirector.Begin` | SAVE | Jump straight to `Act3_Zurich` or `Ending` with the flags you want | SIGN plus checks that the progress graph is consistent (each completed mission must follow its predecessor) |
| 4 | `Purchase` trusts the price the client passes in | `Systems/EconomyManager.cs` `EconomyManager.Purchase(item, clientPrice, qty)` | MEM, DEC, DATA | Set the price to 0 or a negative number before you click Buy. Patch the call in dnSpy to pass 0 | Look up the price from an authoritative table on the server (SRV). At minimum, re-read `item.price` inside `Purchase` and reject a mismatch |
| 5 | `TrySpend` accepts negative amounts (`cash < amount` is false when the amount is negative, so a negative spend adds money) | `EconomyManager.TrySpend` | MEM, DEC | A negative price or quantity turns spending into a gain | Reject `amount <= 0`, INT invariant (cash >= 0), SRV |
| 6 | `WithdrawPension` trusts the requested amount: no balance check, and negative values are allowed | `EconomyManager.WithdrawPension(float)` | MEM, DEC | Withdraw more than the balance (the pension goes negative while cash grows), or enter a negative amount to "deposit" in reverse | Clamp to `0 < amount <= balance`, SRV ledger |
| 7 | The bank UI parses any float and passes it straight through | `Assets/_Game/Scripts/UI/BankScreen.cs` `BankScreen.Withdraw` | Plain input abuse, MEM | Type `99999999` or `-500` into the Amount field. No tool is needed | Validate input in the UI *and* in `EconomyManager`. Never trust UI-side checks alone |
| 8 | The shop charges the price it reads from the item asset or from memory at the moment of the click | `UI/ShopScreen.cs` `ShopScreen.Refresh` (Buy button lambda); `Systems/ItemDefinition.cs` `ItemDefinition.price` | MEM, DATA, DEC | Freeze or rewrite `ItemDefinition.price` in memory, edit the asset in `resources.assets`, or patch the lambda | SRV price table, INT hash of item data, IL2CPP |
| 9 | Mission and objective rewards are paid by the client from its own data, with no server check | `Story/MissionDefinition.cs` `MissionDefinition.rewardCash`; `Systems/MissionManager.cs` `MissionManager.CompleteMission` and `CompleteObjective` (`ObjectiveDefinition.rewardCash`) | DATA, DEC, MEM | Change `rewardCash` in the assets or in memory, or call `ForceComplete` and `Progress` again to farm rewards | SRV grants rewards once per mission id per account. INT check that rewards match a signed table |
| 10 | `MissionManager.ForceComplete` and `Progress` are public and complete objectives without checking anything | `Systems/MissionManager.cs` | DEC | Inject a call (for example with a BepInEx or MelonLoader plugin, or a dnSpy patch) to skip any objective | IL2CPP, INT, keep authoritative progression on a server |
| 11 | Health is a public float | `Assets/_Game/Scripts/Combat/Health.cs` `Health.current` (also `invulnerable` and `damageMultiplier`) | MEM, DEC | God mode: freeze `current` at 100, or flip `invulnerable` to true, or set `damageMultiplier` to 0. This works for enemies too (one-hit kills) | OBF (store an encoded value plus a shadow copy), INT check of the rate of change, IL2CPP |
| 12 | Player health is also copied into `GameState.player.health` and saved | `GameState.PlayerState.health` / `maxHealth`; `PlayerCharacter.WriteToState` | SAVE | Load with `maxHealth: 9999` | SIGN, range validation |
| 13 | Ammo is stored as plain ints | `Player/PlayerCombat.cs` `PlayerCombat.Ammo()` returns `GameState.player.ammo[]` (`AmmoEntry.inMagazine`, `reserve`) | MEM, SAVE | Infinite ammo: freeze `inMagazine`. Scan for the number after each shot, which drops by exactly 1 per shot, so it is easy to find | OBF, INT (magazine never above `magazineSize`), SIGN |
| 14 | The rage meter is a plain float | `Player/RageMode.cs` `RageMode.Meter` -> `GameState.player.rage`; tuning fields `speedMultiplier`, `damageMultiplier`, `damageTakenMultiplier`, `drainPerSecond` | MEM, SAVE | Permanent rage: freeze the meter at 100 or set `drainPerSecond = 0`. Set `damageTakenMultiplier = 0` for god mode. The setter clamps to 0..100, but a raw memory write skips the setter | OBF, INT |
| 15 | Story counters decide the ending and are saved as plain data | `Systems/StoryState.cs` `StoryState.counters` and `flags`; `Story/EndingResolver.cs` (`evidence_collected`, `rage_uses`, `infiltrator_spared`) | SAVE, MEM | Unlock the "Expose" ending (needs `minEvidence`) or "Take Over" without earning it | SIGN, SRV-side achievement and ending validation |
| 16 | Hacking answers (PIN, cipher shifts, auth codes) are stored in plain text inside ScriptableObjects | `Assets/_Game/Scripts/Hacking/DeviceDefinition.cs` `DeviceDefinition.pin`, `InterceptedMessage.shift`, `SpoofOrder.authCode`; generated by `Assets/Editor/DataBuilder.cs` | DEC (AssetRipper, UABE), MEM | Read every answer from `resources.assets` without playing the puzzle | Store only salted hashes of answers. Better: SRV checks the answer |
| 17 | The "server config" `devices.json` ships in plain text next to the exe, holds every answer, and **overrides** the asset values at runtime | `Assets/StreamingAssets/hacking/devices.json`; `Hacking/HackingDataFile.cs` `HackingDataFile` (`Pin`, `Shift`, `AuthCode`) | DATA | Read the answers in Notepad, or set every PIN to `0000`. Remove the file and the asset values are used instead | Do not ship answers. INT (signed manifest of StreamingAssets). SRV owns the config |
| 18 | The hacking lockout is client-side only, and the attempt counter resets when the device is reopened | `UI/HackingScreen.cs` `HackingScreen.Open` (`m_attempts = 0`), `Submit`, `m_lockedUntil` | Gameplay exploit, MEM | Brute-force the PIN: Disconnect and reconnect to reset the attempts. The "digits in the right place" feedback also makes brute force fast | Keep lockout state per device in the session, rate-limit on the SRV, and add backoff |
| 19 | Assemblies are compiled with Mono, so they ship as IL | `Assets/Editor/ProjectSetup.cs` `ProjectSetup.SetupPlayer` (`ScriptingImplementation.Mono2x`) | DEC | Open `Exit Clause_Data/Managed/Assembly-CSharp.dll` in dnSpy and you get near-original C#. You can edit any method and save the module | IL2CPP plus code stripping, encrypt `global-metadata.dat`, INT hash of the assemblies |
| 20 | All game data is in one `Resources` asset that is easy to rip | `Assets/_Game/Data/Resources/GameDatabase.asset`; `Systems/GameDatabase.cs` | DEC, DATA | Dump items, prices, weapons, rewards, endings and device answers with AssetRipper | Move secrets out of the client. Use encrypted Addressables or AssetBundles for anything that must stay |
| 21 | Settings are stored as JSON in PlayerPrefs (on Windows this is the registry key `HKCU\Software\ExitClause\Exit Clause`) | `Systems/SettingsManager.cs`, `SettingsData` | SAVE (low impact) | Out-of-range values such as FOV or sensitivity. This is mainly a demo of unvalidated input | Clamp on load |
| 22 | Inventory `Add` takes any count, and the ammo route adds to `reserve` without a cap | `Systems/InventoryManager.cs` `InventoryManager.Add` | DEC, SAVE | Give yourself `keycard_zurich` or any key item, or 99999 rounds | INT caps, SIGN |

## Memory editing (MEM)

**Tools:** Cheat Engine 7.x. Attach to `Exit Clause.exe`. Turn on *Mono features* (Mono > Activate mono features) so you can browse classes by name.

**Targets:** cash (int, shown on the HUD and in the shop), ammo `inMagazine` (int, drops by 1 per shot), `Health.current` (float), `RageMode.Meter` / `PlayerState.rage` (float), `ItemDefinition.price` (int).

**Method:**
1. Do an "Exact value" scan for the current cash.
2. Spend or earn money, then do a "Next scan" with the new value.
3. Freeze or edit the result.
4. For floats such as health, use "Unknown initial value" and then "Decreased value" after you take damage.
5. Use the Mono dissector to jump straight to `ExitClause.Systems.GameSession` > `State`.

**Why it works:** Every value is a plain field on one long-lived object (`GameSession.Instance.State`). Values are not encoded, there are no shadow copies, and nothing checks invariants.

**Hardening, in order of payoff:** IL2CPP (removes the Mono dissector shortcut) -> OBF on cash, health, ammo and rage (XOR with a per-session key, keep a shadow copy, compare on read) -> INT (cash never below 0, magazine never above `magazineSize`, a health increase only counts when it comes from `Heal`) -> SRV for anything that is worth money.

## Save tampering (SAVE)

**Where:** `%USERPROFILE%\AppData\LocalLow\ExitClause\Exit Clause\saves\slot0.json` (slot 0 is the autosave, slots 1 to 3 are manual). The Editor menu **Exit Clause > Open Save Folder** opens it.

**Method:**
1. Save the game.
2. Close it or go back to the main menu.
3. Edit the JSON, for example `"cash": 999999`, add `"rage_unlocked"` to `story.flags`, set `"sceneName": "Act3_Zurich"`, or set the `evidence_collected` counter to 20.
4. Load the save.

**Why it works:** `SaveSystem.Read` is just `JsonUtility.FromJson<GameState>(File.ReadAllText(...))`. There is no MAC, no encryption, no version or schema check and no range validation.

**Hardening:**
- SIGN: compute HMAC-SHA256 over the canonical JSON, with a key derived per install, and reject a save when the MAC fails.
- Add AES-GCM if you also want the save to be unreadable.
- Validate ranges and the progress graph after a save loads.
- Keep in mind that any key stored in the client can be recovered under DEC. Write up that limit in the portfolio, and offer SRV cloud saves as the real fix.

## Decompiling and patching (DEC)

**Where:** `Exit Clause_Data/Managed/Assembly-CSharp.dll` (Mono build).

**Method:**
1. Open the DLL in dnSpy.
2. Browse the `ExitClause.*` namespaces.
3. Patch a method with *Edit Method*, for example make `EconomyManager.TrySpend` always `return true`, make `Health.TakeDamage` return early, or make `HackingScreen.Submit` always unlock.
4. Use *Save Module* and run the game.

Use AssetRipper to read `GameDatabase` and the `DeviceDefinition` assets.

**Why it works:** `ProjectSetup.SetupPlayer` forces `Mono2x`, so the IL ships with full type and member names.

**Hardening:**
- IL2CPP: native code, but `global-metadata.dat` still exposes names until you protect it, and Il2CppDumper can recover a lot.
- Managed stripping.
- INT: hash the assemblies and check the hash at startup.
- Keep secrets (answers, prices) out of the client entirely (SRV).

## Data-file tampering (DATA)

**Where:** `Exit Clause_Data/StreamingAssets/hacking/devices.json`, which is shipped verbatim. Also `resources.assets` / `sharedassets*.assets` for ScriptableObjects.

**Method:**
- Read the PINs and auth codes in any text editor.
- Change a PIN to `0000`, or change a cipher shift so the messages decrypt to the shift you prefer. Anything in the file wins over the asset values (see `HackingDataFile.Pin`, `Shift` and `AuthCode`).
- With UABE, change `ItemDefinition.price` or `MissionDefinition.rewardCash`.

**Hardening:**
- INT: ship a signed manifest (SHA-256 of every data file, signed with an Ed25519 key) and verify it at load.
- Store only answer hashes in the client.
- Better: SRV serves the config and checks the answers itself.

## Traffic tampering (NET)

**Current state:** The game has **no network code** (there is no `UnityWebRequest` or socket use in `Assets/_Game/Scripts`). The "server-style" trust boundaries are simulated locally:
- `EconomyManager.Purchase(item, clientPrice, qty)` and `WithdrawPension(amount)` are shaped like API calls in which the client sends the price or amount.
- `MissionManager` pays `rewardCash` the way a reward endpoint would.
- `HackingDataFile` reads `devices.json`, which plays the role of a "server config" download.

**Suggested next step for the portfolio:**
1. Add a small backend (for example ASP.NET Minimal API or Flask), and make the first version insecure too: plain HTTP, the client sends `{"itemId":"rifle","price":0}`, and the server trusts it.
2. Intercept the request with mitmproxy, Fiddler or Burp. Point Unity at the proxy, or set `HTTP(S)_PROXY`.
3. Replay and modify the requests: a zero price, a negative withdrawal, a duplicated reward claim.

**Hardening:**
- HTTPS with certificate pinning (in a `CertificateHandler`).
- Server-side price and balance authority (SRV).
- Nonces or idempotency keys against replay.
- Request signing with a per-session token.
- Rate limiting.
- The rule to demonstrate: the server recomputes everything and never accepts a value the client could have chosen.

## Hardening roadmap (suggested order)

1. **SIGN** saves (cheap, and a clear before and after).
2. **Input validation** in `EconomyManager` (`TrySpend`, `WithdrawPension`, `Purchase` re-reads the price). This is a one-line fix each, and it is good to show them.
3. **OBF** for cash, health, ammo and rage, and show that Cheat Engine scans fail.
4. **INT**: a signed StreamingAssets manifest, plus invariant checks.
5. **IL2CPP**: rebuild, then show what dnSpy can no longer do and what Il2CppDumper still can.
6. **SRV**: a backend for the economy and the hacking answers, then the NET section above.
