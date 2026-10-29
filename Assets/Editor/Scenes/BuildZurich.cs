using ExitClause.AI;
using ExitClause.Stealth;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Act 3, Helvetia Private Trust in Zurich. Marble lobby (staff disguise, lift panel hack, Nadia's help
    /// if she was spared), the executive archive floor (ledger evidence, two directors), and the penthouse
    /// confrontation with Marcus Vale followed by the ending choice. Floors are linked by a lift teleporter.
    /// </summary>
    public static class BuildZurich
    {
        public static void Build()
        {
            GB.NewScene("Act3_Zurich", MoodPreset.ZurichMarble);
            GB.Ground(new Vector3(0, 0, 0), new Vector2(160, 160), "Cobblestone");
            GB.Road(new Vector3(-80, 0, -10), new Vector3(80, 0, -10), 10);
            GB.Building("NeighbourL", new Vector3(-35, 0, 20), new Vector3(30, 22, 24), "Plaster");
            GB.Building("NeighbourR", new Vector3(40, 0, 20), new Vector3(24, 20, 24), "PlasterBlue");
            GB.Box("BankFacade", new Vector3(0, 13, 30.4f), new Vector3(30.6f, 10, 20.6f), "Marble");
            GB.Box("BankSign", new Vector3(0, 9.2f, 9.6f), new Vector3(10, 0.8f, 0.2f), "Gold");
            foreach (var x in new[] { -12, -6, 6, 12 }) GB.Box("Column", new Vector3(x, 4, 8.5f), new Vector3(1, 8, 1), "Marble");

            // Lobby
            GB.Room("Lobby", new Vector3(0, 0, 20), new Vector2(30, 20), 8f, "Marble", "MarbleDark", -1, 0.3f, 0.5f, -1);
            GB.Box("Counter", new Vector3(0, 0.6f, 25), new Vector3(10, 1.2f, 1.2f), "Wood");
            GB.Box("Planter1", new Vector3(-9, 0.6f, 15), new Vector3(2, 1.2f, 2), "Hedge");
            GB.Box("Planter2", new Vector3(9, 0.6f, 15), new Vector3(2, 1.2f, 2), "Hedge");
            GB.Light(new Vector3(0, 7.5f, 20), new Color(1f, 0.9f, 0.75f), 12000, 24);
            GB.Room("Lockers", new Vector3(20, 0, 24), new Vector2(10, 8), 3.5f, "Plaster", "Concrete", -1, -1, -1, 0.5f);
            for (int i = 0; i < 4; i++) GB.Box("Locker" + i, new Vector3(17 + i * 2, 1, 27.4f), new Vector3(1.6f, 2, 0.6f), "Metal");
            var uniform = GB.Prop<DisguisePickup>("StaffUniform", new Vector3(23, 1, 27), new Vector3(0.6f, 1.2f, 0.4f), "Fabric", "disguise");
            uniform.disguiseId = "bank_staff";
            uniform.objectiveOnUse = "disguise";
            GB.Light(new Vector3(20, 3, 24), Color.white, 1500, 8);

            GB.Player(new Vector3(0, 0, -14), 0);
            GB.ObjTrigger("lobby", new Vector3(0, 1.5f, 12), new Vector3(4, 3, 2));
            var route = GB.Route("LobbyPatrol", true, new Vector3(-10, 0, 13), new Vector3(10, 0, 13), new Vector3(10, 0, 27), new Vector3(-10, 0, 27));
            GB.Enemy("Bank Guard 1", new Vector3(-10, 0, 13), 90, route, "agency_pistol", "bank_staff");
            GB.Enemy("Lift Guard", new Vector3(-9, 0, 28), 180, null, "agency_pistol", "bank_staff");
            var extra = GB.Enemy("Bank Guard 2", new Vector3(10, 0, 20), 270, null, "agency_smg", "bank_staff");

            var nadia = GB.Npc("Nadia Roux", new Vector3(-4, 0, 11.5f), 30, "TeamSuit");
            var talk = nadia.AddComponent<NPCConversation>();
            talk.npcName = "Nadia";
            talk.dialogue = DataBuilder.Dialogue("dlg_nadia_zurich");
            GB.Flag("Informant", StoryKeys.InfiltratorSpared, null, new[] { nadia }, new[] { extra.gameObject });

            // Lift panel and the lift itself
            GB.Terminal("LiftPanel", new Vector3(-12, 1.4f, 29.6f), DataBuilder.Device("zurich_lift"), "vault_access");
            GB.Box("LiftDoors", new Vector3(-12, 1.5f, 29.8f), new Vector3(2.2f, 3, 0.2f), "Gold");
            var execSpawn = new GameObject("ExecutiveArrival");
            execSpawn.transform.SetPositionAndRotation(new Vector3(-12, 40, -8), Quaternion.identity);
            execSpawn.transform.SetParent(GB.Logic);
            var lift = GB.Prop<Teleporter>("Lift", new Vector3(-12, 0.1f, 28.8f), new Vector3(2, 0.2f, 1), "Gold");
            lift.destination = execSpawn.transform;
            lift.label = "Take the executive lift";
            lift.range = 2.5f;
            GB.Activate("vault_access", ObjectiveActivator.Mode.ActivateWhenCompleted, null, lift.gameObject);

            BuildExecutiveFloor();
            BuildPenthouse();

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_zurich_intro"),
                GB.Shot("Street", new Vector3(-20, 3, -25), new Vector3(0, 8, 10), 45),
                GB.Shot("Facade", new Vector3(6, 1.6f, -4), new Vector3(0, 9, 10), 40));
            var dir = GB.Finish(DataBuilder.Mission("act3_agency"));
            dir.introCutscene = intro;
            GB.Save();
        }

        static void BuildExecutiveFloor()
        {
            var c = new Vector3(0, 40, 0);
            GB.Room("Executive", c, new Vector2(30, 24), 4f, "MarbleDark", "Marble", -1, -1, -1, -1);
            GB.Light(c + new Vector3(0, 3.6f, 0), new Color(1f, 0.92f, 0.8f), 8000, 22);
            for (int i = 0; i < 4; i++) GB.Box("ArchiveShelf" + i, c + new Vector3(-9 + i * 6, 1.1f, 6), new Vector3(1, 2.2f, 6), "Wood");
            Vector3[] files = { new Vector3(-6, 0.95f, -4), new Vector3(6, 0.95f, -4), new Vector3(10, 0.95f, 9) };
            for (int i = 0; i < files.Length; i++)
            {
                GB.Box("Desk" + i, c + files[i] - Vector3.up * 0.5f, new Vector3(2, 0.9f, 1), "Wood");
                var doc = GB.Prop<ObjectiveInteract>("LedgerFile" + i, c + files[i], new Vector3(0.4f, 0.06f, 0.3f), "Fabric", "ledger");
                doc.objectiveId = "ledger"; doc.prompt = "Take ledger file"; doc.addEvidence = 1; doc.hideAfterUse = true;
                doc.notification = i == 0 ? "Pension transfers, retired agents, dates of death." : i == 1 ? "Property deeds signed over to shell companies." : "Approval memo signed: M. Vale.";
            }
            var d1 = GB.Enemy("Director Haas", c + new Vector3(-8, 0, 9), 180, null, "agency_pistol", "", "TeamSuit");
            var d2 = GB.Enemy("Director Lindqvist", c + new Vector3(8, 0, -9), 0, null, "agency_pistol", "", "TeamSuit");
            GB.OnDeath(d1, "directors");
            GB.OnDeath(d2, "directors");
            var patrol = GB.Route("ExecPatrol", true, c + new Vector3(-12, 0, -9), c + new Vector3(12, 0, -9), c + new Vector3(12, 0, 0), c + new Vector3(-12, 0, 0));
            GB.Enemy("Exec Guard", c + new Vector3(-12, 0, -9), 90, patrol, "agency_smg");
            GB.Hide(c + new Vector3(14, 0, 10), 270, "Metal");
        }

        static void BuildPenthouse()
        {
            var c = new Vector3(0, 80, 0);
            GB.Room("Penthouse", c, new Vector2(24, 20), 5f, "Glass", "MarbleDark", -1, -1, -1, -1);
            GB.Light(c + new Vector3(0, 4.5f, 0), new Color(1f, 0.85f, 0.65f), 9000, 22);
            GB.Box("ValeDesk", c + new Vector3(0, 0.5f, 7), new Vector3(4, 1, 1.4f), "Wood");
            for (int i = 0; i < 4; i++) GB.Box("Pillar" + i, c + new Vector3(i < 2 ? -6 : 6, 2.5f, i % 2 == 0 ? -3 : 3), new Vector3(1, 5, 1), "Marble");
            GB.Box("Sofa", c + new Vector3(-8, 0.45f, 6), new Vector3(3, 0.9f, 1), "Fabric");

            var entry = new GameObject("PenthouseEntry");
            entry.transform.SetPositionAndRotation(c + new Vector3(0, 0, -8), Quaternion.identity);
            entry.transform.SetParent(GB.Logic);
            var valeNpc = GB.Npc("Marcus Vale (cutscene)", c + new Vector3(0, 0, 5.5f), 180, "TeamSuit");
            var cs = GB.Cutscene("Vale", DataBuilder.Dialogue("dlg_vale_confront"),
                GB.Shot("ValeWide", c + new Vector3(-7, 2.5f, -6), c + new Vector3(0, 1.5f, 5.5f), 45),
                GB.Shot("ValeClose", c + new Vector3(1.5f, 1.8f, 2.5f), c + new Vector3(0, 1.6f, 5.5f), 30),
                GB.Shot("Adrian", c + new Vector3(-1.5f, 1.8f, -5), c + new Vector3(0, 1.6f, -8), 30));
            cs.placePlayerAfter = entry.transform;
            GB.CutsceneOn("penthouse", false, cs, "penthouse");
            GB.Activate("penthouse", ObjectiveActivator.Mode.DeactivateWhenCompleted, null, valeNpc);

            var vale = GB.Enemy("Marcus Vale", c + new Vector3(0, 0, 5.5f), 180, null, "agency_smg", "", "TeamSuit", false);
            vale.stationary = false;
            vale.canBeTakenDown = false;
            var h = vale.GetComponent<Combat.Health>();
            h.maxHealth = 450; h.current = 450;
            GB.OnDeath(vale, "kill_handler");
            var groupA = new GameObject("ValeGuardsA");
            groupA.transform.SetParent(GB.Actors);
            var groupB = new GameObject("ValeGuardsB");
            groupB.transform.SetParent(GB.Actors);
            for (int i = 0; i < 2; i++)
            {
                var a = GB.Enemy("Vale Guard A" + i, c + new Vector3(i == 0 ? -10 : 10, 0, 8), 180, null, "agency_smg");
                a.stationary = false; a.transform.SetParent(groupA.transform, true);
                var b = GB.Enemy("Vale Guard B" + i, c + new Vector3(i == 0 ? -10 : 10, 0, -8), 0, null, "agency_pistol");
                b.stationary = false; b.transform.SetParent(groupB.transform, true);
            }
            groupA.SetActive(false);
            groupB.SetActive(false);
            var boss = vale.gameObject.AddComponent<BossController>();
            boss.bossName = "Marcus Vale";
            boss.phases = new[]
            {
                new BossController.Phase { healthThreshold = 0.6f, activateOnPhase = groupA,
                    line = new DialogueLine("Vale", "Do you know what a pension is, Adrian? A debt. I collect debts.", 4f) },
                new BossController.Phase { healthThreshold = 0.3f, activateOnPhase = groupB,
                    line = new DialogueLine("Vale", "Twenty-two years and you never once asked where the money went.", 4f) },
            };
            GB.Activate("kill_handler", ObjectiveActivator.Mode.ActivateWhenStarted, null, vale.gameObject);
            var end = GB.Reaction("kill_handler", true);
            end.dialogue = DataBuilder.Dialogue("dlg_vale_ending");
            var choice = new GameObject("EndingChoice");
            choice.transform.SetParent(GB.Logic);
            choice.AddComponent<EndingChoiceTrigger>().openOnObjective = "final_choice";
        }
    }
}
