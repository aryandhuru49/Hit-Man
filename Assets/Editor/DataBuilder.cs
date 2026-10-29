using System.Collections.Generic;
using System.IO;
using ExitClause.CameraSystem;
using ExitClause.Combat;
using ExitClause.Hacking;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEditor;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Generates every ScriptableObject that defines the game's content (missions, clues, suspects,
    /// weapons, items, devices, dialogue, choices, endings, camera tuning) and registers them in
    /// Resources/GameDatabase.asset. Re-running overwrites values, so the content is reproducible.
    /// Edit the generated assets freely, but note a rebuild resets them.
    /// </summary>
    public static partial class DataBuilder
    {
        const string Root = "Assets/_Game/Data";
        static GameDatabase s_db;

        [MenuItem("Exit Clause/3. Build Game Data", priority = 3)]
        public static void BuildAll()
        {
            Directory.CreateDirectory(Root + "/Resources");
            s_db = LoadOrCreate<GameDatabase>("Resources", "GameDatabase");
            s_db.missions.Clear(); s_db.clues.Clear(); s_db.suspects.Clear(); s_db.items.Clear(); s_db.weapons.Clear();
            s_db.devices.Clear(); s_db.dialogues.Clear(); s_db.choices.Clear(); s_db.endings.Clear(); s_db.startingItems.Clear();

            s_db.cameraSettings = LoadOrCreate<CameraSettings>("Camera", "CameraSettings");
            s_db.tracerMaterial = MaterialLibrary.Get("Tracer");
            s_db.visionConeMaterial = MaterialLibrary.Get("VisionCone");
            s_db.evidenceMaterial = MaterialLibrary.Get("Evidence");

            BuildWeapons();
            BuildItems();
            BuildSuspectsAndClues();
            BuildDialogue();
            BuildChoices();
            BuildDevices();
            BuildMissions();
            BuildEndings();

            EditorUtility.SetDirty(s_db);
            AssetDatabase.SaveAssets();
            WriteDevicesJson();
            AssetDatabase.Refresh();
            Debug.Log("[ExitClause] Game data built.");
        }

        // ---------- helpers ----------

        public static T LoadOrCreate<T>(string folder, string name) where T : ScriptableObject
        {
            string dir = $"{Root}/{folder}";
            Directory.CreateDirectory(dir);
            string path = $"{dir}/{name}.asset";
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(a, path);
            }
            EditorUtility.SetDirty(a);
            return a;
        }

        public static T Get<T>(string folder, string name) where T : ScriptableObject =>
            AssetDatabase.LoadAssetAtPath<T>($"{Root}/{folder}/{name}.asset");

        public static MissionDefinition Mission(string id) => Get<MissionDefinition>("Missions", id);
        public static DialogueSequence Dialogue(string id) => Get<DialogueSequence>("Dialogue", id);
        public static ChoiceDefinition Choice(string id) => Get<ChoiceDefinition>("Choices", id);
        public static ClueDefinition Clue(string id) => Get<ClueDefinition>("Clues", id);
        public static DeviceDefinition Device(string id) => Get<DeviceDefinition>("Devices", id);
        public static WeaponDefinition Weapon(string id) => Get<WeaponDefinition>("Weapons", id);

        static ObjectiveDefinition O(string id, string text, int count = 1, bool optional = false) => new ObjectiveDefinition(id, text, count, optional);

        static MissionDefinition M(string id, string title, string subtitle, string scene, string next, int reward, params ObjectiveDefinition[] objectives)
        {
            var m = LoadOrCreate<MissionDefinition>("Missions", id);
            m.id = id; m.title = title; m.subtitle = subtitle; m.sceneName = scene; m.nextScene = next; m.nextSpawnPoint = "";
            m.rewardCash = reward;
            m.objectives = new List<ObjectiveDefinition>(objectives);
            m.setFlagsOnComplete = new List<string>();
            s_db.missions.Add(m);
            return m;
        }

        static DialogueSequence D(string id, params string[] speakerTextPairs)
        {
            var d = LoadOrCreate<DialogueSequence>("Dialogue", id);
            d.id = id;
            d.lines = new List<DialogueLine>();
            for (int i = 0; i + 1 < speakerTextPairs.Length; i += 2)
            {
                string text = speakerTextPairs[i + 1];
                float dur = Mathf.Clamp(1.5f + text.Length * 0.055f, 2.5f, 8f);
                d.lines.Add(new DialogueLine(speakerTextPairs[i], text, dur));
            }
            s_db.dialogues.Add(d);
            return d;
        }

        // ---------- weapons and items ----------

        static WeaponDefinition W(string id, string name, float dmg, float rate, bool auto, int mag, int reserve, float range, float spread, float recoil, bool suppressed, float noise, float ai)
        {
            var w = LoadOrCreate<WeaponDefinition>("Weapons", id);
            w.id = id; w.displayName = name; w.damage = dmg; w.fireRate = rate; w.automatic = auto; w.magazineSize = mag;
            w.startingReserve = reserve; w.range = range; w.spread = spread; w.hipSpread = spread * 3f; w.recoilPitch = recoil;
            w.recoilYaw = recoil * 0.4f; w.suppressed = suppressed; w.noiseRadius = noise; w.aiAccuracy = ai; w.reloadTime = 1.5f;
            return w;
        }

        static void BuildWeapons()
        {
            // Player weapons first: keys 1/2/3 map to this order.
            s_db.weapons.Add(W("pistol", "P9 Suppressed", 34, 3.5f, false, 12, 48, 60, 1f, 1.2f, true, 35, 0.4f));
            s_db.weapons.Add(W("smg", "MP-7", 17, 11f, true, 30, 120, 45, 2.5f, 0.6f, false, 45, 0.3f));
            s_db.weapons.Add(W("rifle", "Marksman Rifle", 80, 1.2f, false, 5, 20, 200, 0.2f, 4f, false, 80, 0.8f));
            // Enemy-only weapons (never in the player's inventory).
            W("agency_pistol", "Agency Pistol", 9, 2.5f, false, 15, 999, 45, 2f, 1f, false, 40, 0.35f);
            W("agency_smg", "Agency SMG", 6, 8f, true, 30, 999, 40, 3f, 0.5f, false, 45, 0.25f);
            W("sniper_rifle", "Hale's Rifle", 55, 0.5f, false, 5, 999, 250, 0.1f, 3f, false, 120, 0.95f);
            W("drone_gun", "Drone Gun", 5, 3f, true, 50, 999, 35, 4f, 0.2f, false, 25, 0.3f);
            foreach (var id in new[] { "agency_pistol", "agency_smg", "sniper_rifle", "drone_gun" })
                s_db.weapons.Add(Weapon(id));
        }

        static ItemDefinition I(string id, string name, string desc, ItemCategory cat, int price, bool shop, int qty, string ammoFor = null)
        {
            var it = LoadOrCreate<ItemDefinition>("Items", id);
            it.id = id; it.displayName = name; it.description = desc; it.category = cat; it.price = price;
            it.soldInShop = shop; it.shopQuantity = qty; it.ammoForWeapon = ammoFor;
            s_db.items.Add(it);
            return it;
        }

        static void BuildItems()
        {
            I("pistol", "P9 Suppressed", "Your old service pistol.", ItemCategory.Weapon, 0, false, 1);
            I("smg", "MP-7", "Taken from the men who came to dinner.", ItemCategory.Weapon, 2500, true, 1);
            I("rifle", "Marksman Rifle", "Hale's rifle. Still warm.", ItemCategory.Weapon, 6000, true, 1);
            I("lockpick", "Lockpick", "Opens locked doors. Breaks on a bad attempt.", ItemCategory.Gadget, 150, true, 3);
            I("coin", "Coin", "Throw (G) to make a noise guards will investigate.", ItemCategory.Gadget, 5, true, 5);
            I("noisemaker", "Noisemaker", "A louder distraction (G, switch with H).", ItemCategory.Gadget, 80, true, 2);
            I("ammo_pistol", "9mm rounds", "24 rounds for the P9.", ItemCategory.Ammo, 60, true, 24, "pistol");
            I("ammo_smg", "4.6mm rounds", "60 rounds for the MP-7.", ItemCategory.Ammo, 90, true, 60, "smg");
            I("ammo_rifle", ".308 rounds", "10 rounds for the rifle.", ItemCategory.Ammo, 120, true, 10, "rifle");
            I("groceries", "Groceries", "Tomatoes, bread, a decent Vermentino.", ItemCategory.Consumable, 40, true, 1);
            I("phone_elias", "Elias Ward's phone", "Agency handset. Locked.", ItemCategory.Device, 0, false, 1);
            I("phone_kessler", "Kessler's phone", "Agency handset. Locked.", ItemCategory.Device, 0, false, 1);
            I("kill_list", "The kill list", "Names of retired agents. Half are crossed out.", ItemCategory.KeyItem, 0, false, 1);
            I("keycard_zurich", "Helvetia staff keycard", "Opens the staff entrance.", ItemCategory.KeyItem, 0, false, 1);
            s_db.startingItems.Add(new ItemStack { itemId = "pistol", count = 1 });
            s_db.startingItems.Add(new ItemStack { itemId = "lockpick", count = 3 });
            s_db.startingItems.Add(new ItemStack { itemId = "coin", count = 5 });
            s_db.startingItems.Add(new ItemStack { itemId = "noisemaker", count = 1 });
        }

        // ---------- journal ----------

        static void S(string id, string name, string theory, Color c)
        {
            var s = LoadOrCreate<SuspectDefinition>("Suspects", id);
            s.id = id; s.displayName = name; s.theory = theory; s.cardColor = c;
            s_db.suspects.Add(s);
        }

        static void C(string id, string title, string desc, string trueSuspect, bool herring, int obs, bool days, string objective, int evidence = 0)
        {
            var c = LoadOrCreate<ClueDefinition>("Clues", id);
            c.id = id; c.title = title; c.description = desc; c.trueSuspectId = trueSuspect; c.isRedHerring = herring;
            c.observationsRequired = obs; c.requiresSeparateDays = days; c.objectiveId = objective; c.evidenceValue = evidence;
            s_db.clues.Add(c);
        }

        static void BuildSuspectsAndClues()
        {
            S("agency", "The Agency?", "Old employers tidying loose ends. They'd never... would they?", new Color(0.75f, 0.7f, 0.6f));
            S("brandt", "The Brandt family", "Dieter Brandt. Vienna, 2015. He had a son.", new Color(0.7f, 0.75f, 0.65f));
            S("sorel", "Sorel's people", "Arms brokers hold grudges, and Prague was only weeks ago.", new Color(0.72f, 0.65f, 0.7f));
            S("nothing", "Nothing at all", "Retirement paranoia. Old habits die hard.", new Color(0.65f, 0.7f, 0.75f));

            C("clue_market_tail", "The man in the grey jacket",
                "Produce aisle. Wine aisle. Leaning on a car in the parking lot. Never buys anything.", "agency", false, 3, false, "market_tail");
            C("clue_tinted_car", "Black sedan, tinted windows",
                "Parked on the coast road at the same bend, three mornings running. Engine ticking, so someone is inside.", "agency", false, 3, true, "tinted_car");
            C("clue_cafe_watcher", "The man who never drinks",
                "Window seat with a clear view of our table. His espresso went cold untouched.", "brandt", true, 1, false, "cafe_watcher");
            C("clue_neighbor", "The new neighbour's questions",
                "'Claire' moved in a week ago. Asked when I'm home, whether I travel, what I do. Her accent slips on the r's.", "agency", false, 1, false, "neighbor");
            C("clue_mail", "Resealed mail",
                "Bank statement envelope opened and glued shut again. Pension fund statement, of all things.", "agency", false, 1, false, "mail");
            C("clue_kill_order", "The kill order",
                "Decrypted on Elias's phone: Team Six, my retirement date, my pension and villa 'revert on confirmation'.", "agency", false, 1, false, null);
        }

        // ---------- dialogue ----------

        static void BuildDialogue()
        {
            const string A = "Adrian", V = "Vale", So = "Sofia";
            D("dlg_prologue_intro",
                V, "Last one, Adrian. Viktor Sorel, top floor of the Hotel Zlata. Do it clean and you never hear my voice again.",
                A, "Promises, promises.",
                V, "Alley's on your left. Shift to move quick, C to keep low. Courtyard guard is looking the wrong way.");
            D("dlg_prologue_takedown", V, "Side door guard. Get behind him and put him down with F. Take his coat if you like: guards ignore their own.");
            D("dlg_prologue_lockpick", V, "Service door's locked. You brought picks. Throw a coin with G if anyone gets curious.");
            D("dlg_prologue_sorel",
                "Sorel", "Adrian Kade. They sent the retiring man.",
                A, "Word travels.",
                "Sorel", "You think you're walking away? They'll do the same to you. They always do.",
                A, "Not my problem after tonight.");
            D("dlg_prologue_escape", V, "Shots on the third floor, guards inbound. Right mouse to aim, left to fire, Q for cover. Get out.");
            D("dlg_prologue_drive", V, "Car's in the alley. Take it to the bridge. E to get in.");
            D("dlg_prologue_outro",
                V, "Twenty-two years, Adrian. Not a single loose end.",
                A, "Don't make it a speech.",
                V, "Keys to Villa Serena. The pension clears Friday. Go grow tomatoes, or whatever retired men do.",
                A, "And the agency just lets me go?",
                V, "That's what the exit clause is for. Enjoy the sun.");

            D("dlg_act0_intro",
                A, "Villa Serena. Sea view, lemon trees, and a pension with more zeros than I ever earned honestly.",
                A, "Sofia runs the bookshop by the harbour. She thinks I sold insurance.",
                A, "Normal life. Groceries. Coffee. Sleep. How hard can it be?");
            D("dlg_act0_market", A, "Same grey jacket in two aisles. Could be nothing. Watch him: E to observe.");
            D("dlg_act0_jog", A, "Morning run. Three markers along the coast road. Keep your eyes open on the bends.");
            D("dlg_sofia_cafe",
                So, "You're late. You're never late.",
                A, "Traffic. Sorry.",
                So, "Don't look now, but the man by the window has been staring at his espresso for twenty minutes.");
            D("dlg_luca",
                "Luca", "You don't remember me. Vienna, 2015. Dieter Brandt was my father.",
                A, "...",
                "Luca", "I didn't come to kill you. I just wanted to see what kind of man does that and then buys lemons at the market.");
            D("dlg_neighbor",
                "Claire", "Bonjour! I'm Claire, I just moved in next door. You live alone? What is it you do?",
                "Claire", "Do you travel much? When are you usually home? I'm always locking myself out!",
                A, "Insurance. Rarely. Nice to meet you, Claire.");
            D("dlg_act0_board", A, "Write it down. Pin it up. See what it looks like when it's not in my head. (J to open the journal)");
            D("dlg_act0_dinner", So, "Dinner's ready! Come in before the pasta turns to glue.");

            D("dlg_act1_intro",
                So, "To the quiet life.",
                A, "To the quiet life.",
                A, "Down! Sofia, the cellar, now!");
            D("dlg_act1_elias",
                A, "Elias? I trained you. Prague, Lisbon, Oslo...",
                A, "Agency phone. Agency gun. This isn't revenge. This is housekeeping.");
            D("dlg_act1_garage", A, "More of them on the drive. The garage. Go.");
            D("dlg_coast_intro", A, "Three cars behind me. Lose them on the coast road, or make them regret following.");
            D("dlg_act1_outro",
                V, "Adrian. I heard about the villa. Terrible business. Some old grudge, surely.",
                V, "Lay low. We'll look into it.",
                A, "Sure, Marcus. You do that.");

            D("dlg_interlude_intro",
                So, "Adrian? There are men here, they...",
                "Unknown", "Harbour, pier nine. Come alone. You know how this works.",
                A, "Yes. I do.");
            D("dlg_interlude_rage", A, "Something in me snaps. (RAGE unlocked: press V when the meter is full)");
            D("dlg_save_done",
                So, "Who ARE you?",
                A, "Someone who should have told you a long time ago. Get in the car.");
            D("dlg_chase_done", A, "A list. Retired agents. Half of them crossed out. Mine is next.");
            D("dlg_luca_rescue", "Luca", "I followed them to the pier. Sofia is safe, she's with me. You owe me nothing.");
            D("dlg_sofia_lost", A, "The pier is empty. Sofia... I'm sorry.");

            D("dlg_safehouse_intro",
                A, "Genoa. A safehouse from another life. Nobody at the agency knows this one.",
                A, "Two phones. Two dead agents. Let's hear what the agency says when it thinks no one's listening.");
            D("dlg_safehouse_return", A, "One less. Who's next?");

            D("dlg_alps_intro",
                A, "Hale. Best long gun the agency ever had.",
                A, "He paints you with a laser before he fires. When the red line drops, I move.");
            D("dlg_alps_misdirected", A, "He took the bait. He's watching the wrong col.");
            D("dlg_alps_nest", A, "Orders, signed V. Every retired agent's address. Mine is circled.");

            D("dlg_marseille_intro",
                A, "Nadia Roux. Mirage. She could be anyone in this crowd. She could have been my neighbour.",
                A, "Nobody's perfect. Study the faces, find the tells, then confront her.");
            D("dlg_nadia_beg",
                "Nadia", "Claire, Madame Dubois, whoever I was. I watched you water lemon trees for a week, Adrian.",
                "Nadia", "I didn't want this. They'll send someone for me next. Let me help you end it.");

            D("dlg_berlin_intro",
                A, "Konrad Brenner. If it has a lens, he's looking through it.",
                A, "Camera relay on the east wall. Knowing Konrad, he never changed the default PIN.");
            D("dlg_berlin_laptop", A, "Brenner's laptop. A straight line into the agency's network.");

            D("dlg_zurich_intro",
                A, "Helvetia Private Trust. Marble and silence.",
                A, "The agency's real headquarters, hiding behind a bank that manages its stolen pensions.");
            D("dlg_nadia_zurich", "Nadia", "Staff entrance is unlocked. Lift code is on Brenner's laptop. Don't make me regret this.");
            D("dlg_vale_confront",
                V, "You were always the best of them, Adrian. That's why your pension was so large.",
                A, "How many, Marcus?",
                V, "Fourteen. Fifteen, after tonight. Nobody retires. It was in the contract. You just never read the exit clause.");
            D("dlg_vale_ending", A, "Twenty-two years. Not a single loose end. Except you.");
        }

        // ---------- choices ----------

        static ChoiceDefinition Ch(string id, string prompt, float time, params ChoiceOption[] options)
        {
            var c = LoadOrCreate<ChoiceDefinition>("Choices", id);
            c.id = id; c.prompt = prompt; c.timeLimit = time; c.defaultOption = 0;
            c.options = new List<ChoiceOption>(options);
            s_db.choices.Add(c);
            return c;
        }

        static ChoiceOption Opt(string id, string label, string hint, string objective, params string[] flags)
        {
            var o = new ChoiceOption(id, label, hint) { progressObjective = objective };
            o.setFlags.AddRange(flags);
            return o;
        }

        static void BuildChoices()
        {
            Ch(StoryKeys.ChoiceLuca, "The watcher is Luca Brandt. He only wanted to see the man who killed his father.", 0,
                Opt("hear_him_out", "Hear him out", "He may remember the kindness.", "confront_luca", StoryKeys.LucaAlly),
                Opt("warn_off", "Warn him off", "He leaves. You'll never know what he would have done.", "confront_luca"));
            Ch(StoryKeys.ChoiceLovedOne, "Sofia is tied up on pier nine with a timer ticking. Across the harbour their team leader is driving off with a list of names.", 12,
                Opt("save", "Save Sofia", "The attacker escapes with the list.", null, StoryKeys.LovedOneSaved),
                Opt("chase", "Chase the attacker", "Sofia is left behind.", null, StoryKeys.ChasedAttacker));
            Ch(StoryKeys.ChoiceSpareInfiltrator, "Nadia Roux kneels on the wet stones. \"They'll send someone for me next. Let me help you end this.\"", 0,
                Opt("spare", "Spare her", "She may return the favour in Zurich.", "decide_nadia", StoryKeys.InfiltratorSpared),
                Opt("kill", "Finish it", "One less agent. One less witness.", "decide_nadia", "infiltrator_killed"));
        }

        // ---------- devices ----------

        static InterceptedMessage Msg(string from, string text, int shift, string flag = null, string clue = null, int evidence = 0) =>
            new InterceptedMessage { from = from, plainText = text, shift = shift, revealsFlag = flag, logsClueId = clue, evidenceValue = evidence };

        static void BuildDevices()
        {
            var elias = LoadOrCreate<DeviceDefinition>("Devices", "phone_elias");
            elias.id = "phone_elias"; elias.deviceLabel = "Agency handset (E. Ward)"; elias.ownerName = "Elias Ward";
            elias.pin = "1990"; elias.objectiveOnUnlock = "crack_elias"; elias.objectiveOnAllMessages = "decrypt_elias";
            elias.messages = new List<InterceptedMessage>
            {
                Msg("VALE", "TEAM SIX: KADE RETIRES FRIDAY. PENSION 2.4M AND VILLA SERENA REVERT ON CONFIRMATION. NO WITNESSES.", 7, null, "clue_kill_order", 1),
                Msg("BRENNER", "ORDER AUTH CODES ROTATE WEEKLY. CURRENT CODE KX-7741. ROUX STILL ON THE NEIGHBOUR COVER.", 4, "knows_auth_code"),
                Msg("ROUX", "I DID NOT SIGN UP TO KILL OUR OWN. WHO SIGNS OFF ON THIS. AM I NEXT", 11, "roux_hesitant"),
            };
            elias.orders = new List<SpoofOrder>
            {
                new SpoofOrder { id = "plant_intel", recipient = "VALE (handler)", description = "Status report: where is Kade hiding?",
                    locations = new List<string> { "Riviera", "Lisbon", "Tangier" }, correctLocation = "Riviera", authCode = "KX-7741",
                    setFlag = StoryKeys.FakeIntelPlanted, objectiveId = "plant_intel" }
            };
            s_db.devices.Add(elias);

            var kessler = LoadOrCreate<DeviceDefinition>("Devices", "phone_kessler");
            kessler.id = "phone_kessler"; kessler.deviceLabel = "Agency handset (Kessler)"; kessler.ownerName = "Jonas Kessler";
            kessler.pin = "4321"; kessler.objectiveOnUnlock = "crack_kessler"; kessler.objectiveOnAllMessages = "decrypt_kessler";
            kessler.messages = new List<InterceptedMessage>
            {
                Msg("HALE", "SET UP ON WEISSGRAT NORTH RIDGE. TELL ME THE MOMENT KADE MOVES.", 5, "loc_alps"),
                Msg("BRENNER", "MARSEILLE: ROUX WORKS THE DOCK MARKET. BERLIN NODE IS THE OLD KRAFTWERK, RELAY PIN 0000 UNTIL I FIX IT.", 9, "loc_marseille_berlin"),
            };
            kessler.orders = new List<SpoofOrder>
            {
                new SpoofOrder { id = "misdirect_hale", recipient = "HALE (sniper)", description = "Relocate overwatch",
                    locations = new List<string> { "Weissgrat North Ridge", "Rotstock East Col" }, correctLocation = "Weissgrat North Ridge",
                    authCode = "KX-7741", setFlag = StoryKeys.SniperMisdirected, objectiveId = "spoof" }
            };
            s_db.devices.Add(kessler);

            var relay = LoadOrCreate<DeviceDefinition>("Devices", "relay_kraftwerk");
            relay.id = "relay_kraftwerk"; relay.deviceLabel = "CCTV relay box"; relay.ownerName = "Kraftwerk Security";
            relay.pin = "0000"; relay.attemptsBeforeLockout = 3; relay.objectiveOnUnlock = "cameras"; relay.disableCameraGroupOnUnlock = "kraftwerk";
            relay.messages = new List<InterceptedMessage>(); relay.orders = new List<SpoofOrder>();
            s_db.devices.Add(relay);

            var laptop = LoadOrCreate<DeviceDefinition>("Devices", "laptop_brenner");
            laptop.id = "laptop_brenner"; laptop.deviceLabel = "Brenner's laptop"; laptop.ownerName = "Konrad Brenner";
            laptop.pin = "2580"; laptop.objectiveOnUnlock = "laptop"; laptop.flagOnUnlock = StoryKeys.NetworkAccess; laptop.objectiveOnAllMessages = "";
            laptop.messages = new List<InterceptedMessage>
            {
                Msg("VALE", "HELVETIA PRIVATE TRUST, ZURICH. EXECUTIVE LIFT PANEL CODE 7391. BRING THE LEDGER UPDATE.", 13, "knows_lift_code", null, 1),
                Msg("LEDGER", "RETIRED ASSETS CLOSED: 14. PENSIONS RECOVERED: 31.6M. PROPERTIES SEIZED: 9. NEXT: KADE.", 2, null, null, 1),
            };
            laptop.orders = new List<SpoofOrder>();
            s_db.devices.Add(laptop);

            var lift = LoadOrCreate<DeviceDefinition>("Devices", "zurich_lift");
            lift.id = "zurich_lift"; lift.deviceLabel = "Executive lift panel"; lift.ownerName = "Helvetia Private Trust";
            lift.pin = "7391"; lift.attemptsBeforeLockout = 4; lift.objectiveOnUnlock = "vault_access";
            lift.messages = new List<InterceptedMessage>(); lift.orders = new List<SpoofOrder>();
            s_db.devices.Add(lift);
        }

        /// <summary>INSECURE BY DESIGN: every hacking answer shipped in plain text next to the game.</summary>
        static void WriteDevicesJson()
        {
            var dir = "Assets/StreamingAssets/hacking";
            Directory.CreateDirectory(dir);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"_comment\": \"Device answer config. Loaded at runtime and trusted. (Intentionally insecure, see SECURITY_TARGETS.md)\",");
            sb.AppendLine("  \"devices\": [");
            for (int i = 0; i < s_db.devices.Count; i++)
            {
                var d = s_db.devices[i];
                var shifts = new List<string>();
                foreach (var m in d.messages) shifts.Add(m.shift.ToString());
                var codes = new List<string>();
                foreach (var o in d.orders) codes.Add("\"" + o.authCode + "\"");
                sb.Append($"    {{ \"id\": \"{d.id}\", \"pin\": \"{d.pin}\", \"shifts\": [{string.Join(", ", shifts)}], \"authCodes\": [{string.Join(", ", codes)}] }}");
                sb.AppendLine(i < s_db.devices.Count - 1 ? "," : "");
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            File.WriteAllText(dir + "/devices.json", sb.ToString());
        }

        // ---------- endings ----------

        static void BuildEndings()
        {
            var expose = LoadOrCreate<EndingDefinition>("Endings", EndingResolver.Expose);
            expose.id = EndingResolver.Expose; expose.title = "Exposed"; expose.minEvidence = 8; expose.requiresNoSpare = false;
            expose.epilogue = "The ledger reaches three newsrooms and a Swiss prosecutor before dawn. Helvetia Private Trust's accounts are frozen by noon.\n\nFourteen names, fourteen families, fourteen pensions that never reached anyone. Their stories run for weeks.\n\nAdrian Kade testifies under a name that isn't his. For the first time in twenty-two years, he tells the truth for a living.";
            expose.lovedOneSavedLine = "Sofia sits in the gallery on the last day of the hearings. Afterwards, she asks him to start at the beginning. He does.";
            expose.lovedOneLostLine = "Sofia's name is not in the ledger. It should not have been anywhere near it. He reads it aloud anyway, into the record.";
            expose.informantLine = "Nadia Roux turns state's witness. Her testimony is the one that makes the directors stop smiling.";
            s_db.endings.Add(expose);

            var take = LoadOrCreate<EndingDefinition>("Endings", EndingResolver.TakeOver);
            take.id = EndingResolver.TakeOver; take.title = "The New Handler"; take.minEvidence = 0; take.requiresNoSpare = true; take.minRageUses = 3;
            take.epilogue = "Nobody announces it. The phones simply start ringing for Adrian instead of Marcus.\n\nThe pensions stay where they are. The contracts keep their clause. Somewhere, a young agent signs on the line without reading it.\n\nThe agency never lets anyone retire. Now it is his.";
            take.lovedOneSavedLine = "Sofia leaves the Riviera without a goodbye. She saw his face on the pier. She knew.";
            take.lovedOneLostLine = "There is no one left to disappoint. That makes the job easier.";
            take.informantLine = "Nadia Roux is the first name he crosses out.";
            s_db.endings.Add(take);

            var gone = LoadOrCreate<EndingDefinition>("Endings", EndingResolver.Disappear);
            gone.id = EndingResolver.Disappear; gone.title = "Gone"; gone.minEvidence = 0; gone.requiresNoSpare = false;
            gone.epilogue = "Villa Serena burns on a Tuesday. The agency's investigators find the body of a man about Adrian Kade's height and close the file.\n\nIn a fishing town nobody visits, a quiet man rents a room above a bar. He pays cash and never talks about his old job.\n\nThe agency never lets anyone retire. So he didn't. He just stopped existing.";
            gone.lovedOneSavedLine = "Every spring, a bookshop on the Riviera receives a postcard with no signature and a pressed lemon blossom.";
            gone.lovedOneLostLine = "He keeps one photograph. He never looks at it, and he never throws it away.";
            gone.informantLine = "A postcard arrives from Marseille too, addressed to no one: 'We're even.'";
            s_db.endings.Add(gone);
        }
    }
}
