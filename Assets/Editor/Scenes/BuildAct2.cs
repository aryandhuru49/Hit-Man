using ExitClause.AI;
using ExitClause.Hacking;
using ExitClause.Stealth;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Act 2 scenes: the Genoa safehouse hub (device hacking, shop, travel map), the Alps sniper duel,
    /// the Marseille crowd hunt for the Infiltrator, and the Berlin data centre with drones and CCTV.
    /// </summary>
    public static class BuildAct2
    {
        public static void BuildSafehouse()
        {
            GB.NewScene("Act2_Safehouse", MoodPreset.SafehouseEvening);
            GB.Ground(new Vector3(0, -6, 0), new Vector2(120, 120), "Cobblestone");
            GB.Room("Apartment", Vector3.zero, new Vector2(14, 10), 3.2f, "PlasterPink", "Wood", -1, -1, 0.5f, -1);
            GB.Box("Balcony", new Vector3(0, -0.1f, -7), new Vector3(8, 0.2f, 4), "Concrete");
            GB.Box("BalconyRail", new Vector3(0, 0.5f, -9), new Vector3(8, 1, 0.1f), "Metal");
            GB.Box("Table", new Vector3(0, 0.75f, 1), new Vector3(3, 0.1f, 1.4f), "Wood");
            GB.Box("Bed", new Vector3(5, 0.35f, 3), new Vector3(2, 0.7f, 2.4f), "Fabric");
            GB.Light(new Vector3(0, 2.8f, 1), new Color(1f, 0.75f, 0.5f), 2500, 10);
            GB.Light(new Vector3(-5, 2.8f, -2), new Color(1f, 0.75f, 0.5f), 1500, 8);
            for (int i = 0; i < 8; i++)
                GB.Building("City" + i, new Vector3(-50 + i * 14, -6, 30 + (i % 3) * 6), new Vector3(12, 14 + (i % 4) * 4, 10), i % 2 == 0 ? "Plaster" : "Terracotta");

            GB.Player(new Vector3(-3, 0, -2), 0);
            GB.Terminal("Elias Phone", new Vector3(-0.8f, 0.85f, 1), DataBuilder.Device("phone_elias"), "crack_elias");
            GB.Marker("decrypt_elias", new Vector3(-0.8f, 0.85f, 1));
            GB.Marker("plant_intel", new Vector3(-0.8f, 0.85f, 1));
            GB.Terminal("Kessler Phone", new Vector3(0.8f, 0.85f, 1), DataBuilder.Device("phone_kessler"), "crack_kessler");
            GB.Marker("decrypt_kessler", new Vector3(0.8f, 0.85f, 1));
            GB.Marker("spoof", new Vector3(0.8f, 0.85f, 1));
            GB.Prop<TravelMap>("WallMap", new Vector3(-6.75f, 1.6f, 1), new Vector3(0.05f, 1.4f, 2.2f), "Concrete", "travel");
            var shop = GB.Prop<ShopTerminal>("BurnerPhone", new Vector3(5.5f, 0.75f, -3), new Vector3(0.2f, 0.05f, 0.3f), "Metal");
            shop.shopName = "Old contact (burner phone)";
            GB.Prop<BankTerminal>("Laptop", new Vector3(-2, 0.82f, 1.2f), new Vector3(0.45f, 0.04f, 0.3f), "ScreenGreen");

            var found = GB.Reaction("decrypt_kessler", true);
            found.notification = "Locations found: the Alps, Marseille, Berlin. Check the map.";
            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_safehouse_intro"),
                GB.Shot("Window", new Vector3(5, 2.4f, -4), new Vector3(0, 0.9f, 1), 45),
                GB.Shot("Phones", new Vector3(0, 1.8f, -0.5f), new Vector3(0, 0.85f, 1), 30));
            var dir = GB.Finish(DataBuilder.Mission("act2_safehouse"));
            dir.alternateMission = DataBuilder.Mission("act2_safehouse_return");
            dir.alternateIfFlag = StoryKeys.DevicesCracked;
            dir.introCutscene = intro;
            GB.Save();
        }

        static void EvidenceSquad(params Vector3[] positions)
        {
            // Rage left evidence and no fake intel was planted: the agency sent extra people.
            var root = new GameObject("RageEvidenceSquad");
            root.transform.SetParent(GB.Actors);
            foreach (var p in positions)
            {
                var e = GB.Enemy("Tracker", p, Random.Range(0, 360), null, "agency_smg");
                e.transform.SetParent(root.transform, true);
                e.GetComponent<AIPerception>().detectTime = 1f;
            }
            GB.Flag("RageSquad", null, StoryKeys.FakeIntelPlanted, new[] { root }, null, StoryKeys.RageEvidence, 2);
        }

        public static void BuildAlps()
        {
            GB.NewScene("Act2_Alps", MoodPreset.AlpsCold);
            GB.Ground(new Vector3(0, 0, 110), new Vector2(320, 300), "Snow");
            GB.Box("NorthRidge", new Vector3(0, 10, 218), new Vector3(80, 20, 30), "Rock");
            GB.Box("EastCol", new Vector3(120, 8, 150), new Vector3(30, 16, 30), "Rock");
            GB.Box("WestWall", new Vector3(-120, 20, 110), new Vector3(30, 40, 280), "Rock");
            GB.Box("EastWall", new Vector3(150, 20, 60), new Vector3(30, 40, 160), "Rock");
            GB.Stairs(new Vector3(-42, 0, 175), Vector3.forward, 20f, 4f, "Rock");
            GB.Box("StairLanding", new Vector3(-42, 19.9f, 205.8f), new Vector3(4, 0.2f, 4), "Rock");
            GB.Stairs(new Vector3(103, 0, 125), Vector3.forward, 16f, 4f, "Rock");
            // Cover rocks across the valley
            var rng = new System.Random(7);
            for (int i = 0; i < 40; i++)
            {
                float x = -80 + (float)rng.NextDouble() * 160;
                float z = 20 + (float)rng.NextDouble() * 170;
                float s = 1.5f + (float)rng.NextDouble() * 2.5f;
                GB.Box("Rock" + i, new Vector3(x, s * 0.5f, z), new Vector3(s * 1.4f, s, s), "Rock", null, (float)rng.NextDouble() * 90);
            }
            GB.Building("Chalet", new Vector3(-15, 0, -10), new Vector3(10, 6, 8), "Wood");
            GB.Player(new Vector3(0, 0, 5), 0);
            GB.ObjTrigger("approach", new Vector3(-42, 1.5f, 172), new Vector3(8, 3, 4));

            var north = Sniper("Hale (north ridge)", new Vector3(0, 20, 210), 180);
            var east = Sniper("Hale (east col)", new Vector3(120, 16, 140), 90);
            GB.Flag("Misdirected", StoryKeys.SniperMisdirected, null, new[] { east }, new[] { north });
            var nest = GB.Prop<ObjectiveInteract>("HaleNest", new Vector3(5, 20.4f, 214), new Vector3(1.6f, 0.5f, 0.8f), "Fabric", "nest");
            nest.objectiveId = "nest"; nest.prompt = "Search Hale's nest"; nest.addEvidence = 1; nest.giveItemId = "rifle";
            nest.dialogue = DataBuilder.Dialogue("dlg_alps_nest");
            var kill = GB.Reaction("kill_sniper", true);
            kill.counterKey = StoryKeys.TeamKilled;
            kill.counterDelta = 1;
            var mis = GB.Reaction("approach", false);
            mis.requireFlag = StoryKeys.SniperMisdirected;
            mis.dialogue = DataBuilder.Dialogue("dlg_alps_misdirected");

            var route = GB.Route("Valley", true, new Vector3(-30, 0, 90), new Vector3(30, 0, 90), new Vector3(30, 0, 130), new Vector3(-30, 0, 130));
            GB.Enemy("Spotter 1", new Vector3(-30, 0, 90), 0, route, "agency_smg");
            GB.Enemy("Spotter 2", new Vector3(-36, 0, 196), 90, null, "agency_pistol");
            EvidenceSquad(new Vector3(-20, 0, 60), new Vector3(25, 0, 70), new Vector3(0, 0, 150));

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_alps_intro"),
                GB.Shot("Valley", new Vector3(10, 6, -5), new Vector3(0, 15, 200), 40),
                GB.Shot("Ridge", new Vector3(-10, 24, 190), new Vector3(0, 21, 210), 35));
            var dir = GB.Finish(DataBuilder.Mission("act2_sniper"));
            dir.introCutscene = intro;
            GB.Save();
        }

        static GameObject Sniper(string name, Vector3 pos, float yaw)
        {
            var root = new GameObject(name) { layer = Layers.Character };
            root.transform.SetParent(GB.Actors);
            root.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.9f, 0); col.height = 1.8f; col.radius = 0.35f;
            var h = root.AddComponent<Combat.Health>();
            h.maxHealth = 120; h.current = 120;
            var head = new GameObject("Head");
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0, 1.5f, 0);
            var per = root.AddComponent<AIPerception>();
            per.eyes = head.transform; per.viewDistance = 240; per.viewAngle = 80; per.detectTime = 1.5f; per.closeAwareness = 6;
            var s = root.AddComponent<SniperAI>();
            s.rifle = DataBuilder.Weapon("sniper_rifle");
            s.head = head.transform;
            root.AddComponent<Combat.LockOnTarget>().aimPoint = head.transform;
            PrefabBuilder.Humanoid(root.transform, "TeamSuit", true);
            CharacterArt.AttachNpc(root, "Agent");
            PrefabBuilder.AddCone(root.transform, per);
            GB.OnDeath(h, "kill_sniper");
            return root;
        }

        public static void BuildMarseille()
        {
            GB.NewScene("Act2_Marseille", MoodPreset.MarseilleDay);
            GB.Ground(new Vector3(0, 0, 50), new Vector2(200, 120), "Concrete");
            GB.Box("Sea", new Vector3(0, -2, -60), new Vector3(300, 1, 100), "Water");
            for (int i = 0; i < 5; i++) GB.Building("Quay" + i, new Vector3(-80 + i * 40, 0, 100), new Vector3(34, 14, 14), i % 2 == 0 ? "Plaster" : "Terracotta");
            for (int i = 0; i < 10; i++)
                GB.Box("Container" + i, new Vector3(60 + (i % 2) * 4, 1.3f, 20 + i * 7), new Vector3(2.5f, 2.6f, 6), i % 2 == 0 ? "Container" : "ContainerRed");
            // Market stalls
            for (int i = 0; i < 6; i++)
            {
                float x = -25 + i * 10;
                GB.Box("Stall" + i, new Vector3(x, 0.5f, 30), new Vector3(4, 1, 1.5f), "Wood");
                GB.Box("Awning" + i, new Vector3(x, 2.6f, 30.5f), new Vector3(4.4f, 0.1f, 2.5f), i % 2 == 0 ? "Fabric" : "PlasterBlue");
            }
            GB.Player(new Vector3(0, 0, 80), 180);
            GB.ObjTrigger("dock_market", new Vector3(0, 1.5f, 40), new Vector3(30, 3, 4));

            // Backup agents and Nadia, revealed on confrontation
            var backup = new GameObject("Backup");
            backup.transform.SetParent(GB.Actors);
            Vector3[] bp = { new Vector3(-30, 0, 15), new Vector3(30, 0, 12), new Vector3(0, 0, 5) };
            for (int i = 0; i < bp.Length; i++)
            {
                var e = GB.Enemy("Agency Backup " + i, bp[i], 0, null, "agency_smg");
                e.stationary = false;
                e.GetComponent<AIPerception>().Suspicion = 0.9f;
                e.transform.SetParent(backup.transform, true);
                GB.OnDeath(e, "subdue");
            }
            backup.SetActive(false);
            var nadia = GB.Npc("Nadia Roux", new Vector3(0, 0, 2), 0, "TeamSuit");
            var choiceGo = new GameObject("SpareChoice");
            choiceGo.transform.SetParent(nadia.transform, false);
            var spare = choiceGo.AddComponent<ChoiceTrigger>();
            spare.choice = DataBuilder.Choice(StoryKeys.ChoiceSpareInfiltrator);
            spare.range = 0;
            var talk = nadia.AddComponent<NPCConversation>();
            talk.npcName = "Nadia";
            talk.dialogue = DataBuilder.Dialogue("dlg_nadia_beg");
            talk.followUpChoice = spare;
            talk.objectiveId = "";
            GB.Marker("decide_nadia", nadia.transform.position);
            GB.Activate("subdue", ObjectiveActivator.Mode.ActivateWhenCompleted, null, nadia);

            string[] obs =
            {
                "Old man mending nets. Hands like rope. Thirty years of this, at least.",
                "Young couple arguing about a ferry ticket. Real tears. Not her.",
                "Sunglasses, cash in hand, very nervous. Probably selling counterfeit watches.",
                "Fishmonger's apron over expensive running flats. Hasn't sold a fish. Touches her ear when gulls scream: an earpiece. Watching the exits, not the customers.",
                "Tourist with a camera. Photographs everything, mostly pigeons.",
                "Woman feeding the gulls. Laughs too loud; her friends laugh louder. Locals.",
                "Street cook. Burn scars on both forearms, laughing with every customer.",
                "Dock worker on a smoke break. Hands black with engine grease.",
            };
            bool[] isHer = { false, false, false, true, false, false, false, false };
            for (int i = 0; i < obs.Length; i++)
            {
                float x = -27 + i * 7.5f;
                var npc = GB.Npc(isHer[i] ? "Fishmonger" : "Local " + i, new Vector3(x, 0, isHer[i] ? 32 : 26 + (i % 2) * 2), 180, isHer[i] ? "Fabric" : "Civilian");
                var cs = npc.AddComponent<CrowdSuspect>();
                cs.observation = obs[i];
                cs.isInfiltrator = isHer[i];
                cs.revealObjects = new[] { backup };
                cs.range = 2.5f;
            }
            GB.Marker("study_crowd", new Vector3(0, 0, 28));
            GB.Marker("confront", new Vector3(0, 0, 28));

            var killed = GB.Reaction("decide_nadia", true);
            killed.requireFlag = "infiltrator_killed";
            killed.counterKey = StoryKeys.TeamKilled;
            killed.counterDelta = 1;
            killed.deactivate = new[] { nadia };
            var spared = GB.Reaction("decide_nadia", true);
            spared.requireFlag = StoryKeys.InfiltratorSpared;
            spared.counterKey = StoryKeys.EvidenceCollected;
            spared.counterDelta = 1;
            spared.notification = "Nadia hands you Helvetia Private Trust's floor plan. (+1 evidence)";

            var strollRoute = GB.Route("Promenade", true, new Vector3(-40, 0, 45), new Vector3(40, 0, 45), new Vector3(40, 0, 60), new Vector3(-40, 0, 60));
            for (int i = 0; i < 6; i++) GB.Civilian("Shopper" + i, new Vector3(-30 + i * 10, 0, 50), strollRoute);
            EvidenceSquad(new Vector3(-50, 0, 70), new Vector3(50, 0, 70));

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_marseille_intro"),
                GB.Shot("Docks", new Vector3(-30, 10, 70), new Vector3(0, 1, 28), 45),
                GB.Shot("Crowd", new Vector3(5, 1.8f, 38), new Vector3(0, 1.5f, 28), 35));
            var dir = GB.Finish(DataBuilder.Mission("act2_infiltrator"));
            dir.introCutscene = intro;
            GB.Save();
        }

        public static void BuildBerlin()
        {
            GB.NewScene("Act2_Berlin", MoodPreset.BerlinNeon);
            GB.Ground(new Vector3(0, 0, 0), new Vector2(200, 200), "WetAsphalt");
            // Kraftwerk shell: x -24..24, z 0..40, door on the south wall
            GB.Room("Kraftwerk", new Vector3(0, 0, 20), new Vector2(48, 40), 10f, "ConcreteDark", "Concrete", -1, -1, 0.5f, -1);
            GB.Building("Chimney", new Vector3(-30, 0, 35), new Vector3(6, 40, 6), "Brick");
            // Fence around the courtyard with a gap at the gate
            GB.Wall(new Vector3(-40, 0, -40), new Vector3(40, 0, -40), 3, "Metal", 0.15f, 0.5f, 6f);
            GB.Wall(new Vector3(-40, 0, -40), new Vector3(-40, 0, 0), 3, "Metal", 0.15f);
            GB.Wall(new Vector3(40, 0, 0), new Vector3(40, 0, -40), 3, "Metal", 0.15f);
            for (int i = 0; i < 6; i++) GB.Box("Barrier" + i, new Vector3(-25 + i * 10, 0.6f, -15 + (i % 2) * 6), new Vector3(3, 1.2f, 0.6f), "Concrete");
            GB.Box("NeonSign", new Vector3(0, 9, -0.4f), new Vector3(12, 1.2f, 0.2f), "NeonPink");
            GB.Box("NeonStripL", new Vector3(-24.3f, 5, 10), new Vector3(0.1f, 0.3f, 18), "NeonCyan");
            GB.Light(new Vector3(0, 7, -3), new Color(1f, 0.2f, 0.7f), 8000, 25);
            GB.Light(new Vector3(-26, 4, 10), new Color(0.2f, 0.9f, 1f), 5000, 18);
            for (int x = -30; x <= 30; x += 20) GB.Lamp(new Vector3(x, 0, -30), new Color(0.8f, 0.9f, 1f), 3000, 18, 6);

            GB.Player(new Vector3(0, 0, -60), 0);
            GB.ObjTrigger("infiltrate", new Vector3(0, 1.5f, 2), new Vector3(4, 3, 3));
            GB.Terminal("CCTV Relay", new Vector3(24.6f, 1.4f, -2), DataBuilder.Device("relay_kraftwerk"), "cameras");
            GB.Box("RelayBox", new Vector3(24.6f, 1.2f, -2), new Vector3(0.6f, 0.8f, 0.4f), "Metal");

            // Cameras outside and inside
            Camera("Cam Gate", new Vector3(-6, 6, -0.6f), 180);
            Camera("Cam Yard", new Vector3(24, 6, -0.6f), 220);
            Camera("Cam Hall", new Vector3(-23, 7, 20), 90);
            Camera("Cam Servers", new Vector3(23, 7, 30), 270);
            var yard = GB.Route("DroneYard", true, new Vector3(-30, 0, -30), new Vector3(30, 0, -30), new Vector3(30, 0, -8), new Vector3(-30, 0, -8));
            GB.Drone("Drone 1", new Vector3(-30, 6, -30), yard, false);
            var yard2 = GB.Route("DroneYard2", true, new Vector3(30, 0, -20), new Vector3(-30, 0, -20));
            GB.Drone("Drone 2", new Vector3(30, 6, -20), yard2, true);

            // Server hall
            for (int r = 0; r < 4; r++)
                for (int k = 0; k < 3; k++)
                    GB.Box("Rack", new Vector3(-15 + r * 10, 1.2f, 15 + k * 7), new Vector3(1.2f, 2.4f, 5), "MarbleDark");
            GB.Light(new Vector3(0, 9, 20), new Color(0.6f, 0.8f, 1f), 9000, 30);
            GB.ObjTrigger("server_room", new Vector3(0, 1.5f, 12), new Vector3(30, 3, 2));
            var hall = GB.Route("Hall", true, new Vector3(-20, 0, 8), new Vector3(20, 0, 8), new Vector3(20, 0, 36), new Vector3(-20, 0, 36));
            GB.Enemy("Hall Guard 1", new Vector3(-20, 0, 8), 90, hall, "agency_smg");
            GB.Enemy("Hall Guard 2", new Vector3(10, 0, 36), 180, null, "agency_pistol");
            var yardRoute = GB.Route("Yard", false, new Vector3(-20, 0, -25), new Vector3(20, 0, -25));
            GB.Enemy("Yard Guard", new Vector3(-20, 0, -25), 90, yardRoute, "agency_pistol");

            var brenner = GB.Enemy("Konrad Brenner", new Vector3(0, 0, 37), 180, null, "agency_smg", "", "TeamSuit");
            brenner.isTeamMember = true;
            brenner.GetComponent<Combat.Health>().maxHealth = 220;
            brenner.GetComponent<Combat.Health>().current = 220;
            GB.OnDeath(brenner, "kill_brenner");
            var drones = new GameObject("BrennerDrones");
            drones.transform.SetParent(GB.Actors);
            var d1 = GB.Drone("Brenner Drone A", new Vector3(-8, 6, 30), null, true);
            var d2 = GB.Drone("Brenner Drone B", new Vector3(8, 6, 30), null, true);
            d1.transform.SetParent(drones.transform, true);
            d2.transform.SetParent(drones.transform, true);
            drones.SetActive(false);
            var boss = brenner.gameObject.AddComponent<BossController>();
            boss.bossName = "Konrad Brenner";
            boss.phases = new[]
            {
                new BossController.Phase { healthThreshold = 0.5f, activateOnPhase = drones,
                    line = new Story.DialogueLine("Brenner", "You're a ghost in my system, Kade. Ghosts get deleted.", 4f) }
            };
            GB.Box("Desk", new Vector3(0, 0.45f, 38.5f), new Vector3(2.5f, 0.9f, 1), "Metal");
            GB.Terminal("Brenner Laptop", new Vector3(0, 0.93f, 38.5f), DataBuilder.Device("laptop_brenner"), "laptop");
            GB.Activate("laptop", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_berlin_laptop"));
            GB.Marker("kill_brenner", brenner.transform.position);
            EvidenceSquad(new Vector3(-30, 0, -50), new Vector3(30, 0, -50));

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_berlin_intro"),
                GB.Shot("Plant", new Vector3(-20, 4, -70), new Vector3(0, 6, 10), 45),
                GB.Shot("Relay", new Vector3(30, 2, -10), new Vector3(24.6f, 1.4f, -2), 40));
            var dir = GB.Finish(DataBuilder.Mission("act2_tech"));
            dir.introCutscene = intro;
            GB.Save();
        }

        static void Camera(string name, Vector3 pos, float yaw)
        {
            var root = new GameObject(name) { layer = Layers.Props };
            root.transform.SetParent(GB.Actors);
            root.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            PrefabBuilder.Part(root.transform, PrimitiveType.Cube, "Housing", Vector3.zero, new Vector3(0.3f, 0.25f, 0.6f), "Metal");
            var eyes = new GameObject("Lens");
            eyes.transform.SetParent(root.transform, false);
            eyes.transform.localPosition = new Vector3(0, 0, 0.35f);
            eyes.transform.localEulerAngles = new Vector3(30, 0, 0);
            var per = root.AddComponent<AIPerception>();
            per.eyes = eyes.transform; per.viewDistance = 20; per.viewAngle = 50; per.detectTime = 1.4f; per.closeAwareness = 0;
            var cam = root.AddComponent<SecurityCamera>();
            cam.groupId = "kraftwerk";
        }
    }
}
