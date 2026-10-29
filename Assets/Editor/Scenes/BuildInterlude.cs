using ExitClause.AI;
using ExitClause.Story;
using ExitClause.Systems;
using ExitClause.Vehicles;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Interlude, the harbour at night. Reach pier nine, Rage unlocks, then the timed choice:
    /// save Sofia (fight her captors on the pier) or chase the attacker (car pursuit, take the kill list).
    /// Each branch is its own mission; branch actors are switched on by the ChoiceTrigger.
    /// </summary>
    public static class BuildInterlude
    {
        public static void Build()
        {
            GB.NewScene("Interlude_Harbor", MoodPreset.RivieraNight);
            GB.Ground(new Vector3(0, 0, 60), new Vector2(240, 120), "Concrete");
            GB.Box("Sea", new Vector3(0, -2, -80), new Vector3(400, 1, 160), "Water");
            for (int i = 0; i < 5; i++) GB.Building("Warehouse" + i, new Vector3(-90 + i * 45, 0, 105), new Vector3(36, 12, 18), i % 2 == 0 ? "Brick" : "ConcreteDark");
            string[] cmats = { "Container", "ContainerRed" };
            for (int i = 0; i < 14; i++)
            {
                float x = -100 + (i % 7) * 30, z = 25 + (i / 7) * 30;
                GB.Box("Container" + i, new Vector3(x, 1.3f, z), new Vector3(2.5f, 2.6f, 6), cmats[i % 2]);
                if (i % 3 == 0) GB.Box("ContainerTop" + i, new Vector3(x, 3.9f, z), new Vector3(2.5f, 2.6f, 6), cmats[(i + 1) % 2]);
            }
            for (int p = 0; p < 5; p++)
            {
                float x = -60 + p * 25;
                GB.Box("Pier" + p, new Vector3(x, -0.1f, -30), new Vector3(6, 0.6f, 60), "Wood");
                GB.Lamp(new Vector3(x + 2.5f, 0.2f, -20), new Color(1f, 0.7f, 0.4f), 2500, 14);
            }
            GB.Box("Pier9Sign", new Vector3(40, 3, 1), new Vector3(2, 1, 0.1f), "NeonPink");
            GB.Box("Pier9", new Vector3(40, -0.1f, -30), new Vector3(6, 0.6f, 60), "Wood");
            GB.Box("Crate1", new Vector3(38, 0.7f, -35), new Vector3(1.5f, 1, 1.5f), "Wood");
            GB.Box("Crate2", new Vector3(42, 0.7f, -45), new Vector3(1.5f, 1, 1.5f), "Wood");
            for (int x = -100; x <= 100; x += 40) GB.Lamp(new Vector3(x, 0, 10), new Color(1f, 0.7f, 0.4f), 3500, 20);

            GB.Player(new Vector3(0, 0, 85), 180);
            var playerCar = GB.Car("Your Car", new Vector3(6, 0, 88), 180, "CarRed");
            playerCar.GetComponent<VehicleEntry>().objectiveOnEnter = "escort";
            GB.Marker("escort", playerCar.transform.position);
            GB.ObjTrigger("reach_harbor", new Vector3(40, 1.5f, 3), new Vector3(8, 3, 4));

            // --- Save branch: captors on the pier and Sofia at the end
            var saveRoot = new GameObject("Branch_Save");
            saveRoot.transform.SetParent(GB.Actors);
            var sofia = GB.Npc("Sofia", new Vector3(40, 0.2f, -56), 0, "Fabric");
            sofia.transform.SetParent(saveRoot.transform, true);
            var free = sofia.AddComponent<ObjectiveInteract>();
            free.objectiveId = "defuse"; free.prompt = "Free Sofia"; free.dialogue = DataBuilder.Dialogue("dlg_save_done");
            GB.Marker("defuse", sofia.transform.position).transform.SetParent(saveRoot.transform, true);
            Vector3[] cp = { new Vector3(38, 0.2f, -30), new Vector3(42, 0.2f, -40), new Vector3(38, 0.2f, -48), new Vector3(42, 0.2f, -53) };
            for (int i = 0; i < cp.Length; i++)
            {
                var e = GB.Enemy("Captor " + i, cp[i], 0, null, i % 2 == 0 ? "agency_smg" : "agency_pistol");
                e.stationary = false;
                e.transform.SetParent(saveRoot.transform, true);
                GB.OnDeath(e, "free_sofia");
            }
            GB.Marker("free_sofia", new Vector3(40, 0.2f, -40)).transform.SetParent(saveRoot.transform, true);
            saveRoot.SetActive(false);

            // --- Chase branch: the attacker's car flees around the docks
            var chaseRoot = new GameObject("Branch_Chase");
            chaseRoot.transform.SetParent(GB.Actors);
            var route = GB.Route("Flee", true, new Vector3(90, 0, 15), new Vector3(100, 0, 75), new Vector3(-100, 0, 80), new Vector3(-110, 0, 15), new Vector3(0, 0, 12));
            var fleeCar = GB.Car("Attacker's Car", new Vector3(60, 0, 15), 90, "CarBlack", false);
            fleeCar.transform.SetParent(chaseRoot.transform, true);
            fleeCar.GetComponent<Combat.Health>().maxHealth = 140;
            fleeCar.GetComponent<Combat.Health>().current = 140;
            var flee = fleeCar.GetComponent<AIDriver>();
            flee.route = route;
            flee.cruiseSpeed = 22;
            var grab = fleeCar.gameObject.AddComponent<ObjectiveInteract>();
            grab.objectiveId = "grab_intel"; grab.prompt = "Search the wreck"; grab.addEvidence = 2; grab.giveItemId = "kill_list";
            grab.notification = "A list of retired agents. Half are crossed out. Mine is next.";
            grab.range = 3.5f;
            var chase = new GameObject("Chase").AddComponent<ChaseSequence>();
            chase.transform.SetParent(chaseRoot.transform);
            chase.kind = ChaseSequence.ChaseKind.Pursue;
            chase.playerCar = playerCar;
            chase.fleeingTarget = flee;
            chase.pursuers = new AIDriver[0];
            chase.objectiveId = "chase_attacker";
            chaseRoot.SetActive(false);

            var choiceGo = new GameObject("LovedOneChoice");
            choiceGo.transform.SetParent(GB.Logic);
            var choice = choiceGo.AddComponent<ChoiceTrigger>();
            choice.choice = DataBuilder.Choice(StoryKeys.ChoiceLovedOne);
            choice.openOnObjective = "decide";
            choice.branches = new[]
            {
                new ChoiceTrigger.BranchMission { optionId = "save", mission = DataBuilder.Mission("interlude_save"), activate = new[] { saveRoot } },
                new ChoiceTrigger.BranchMission { optionId = "chase", mission = DataBuilder.Mission("interlude_chase"), activate = new[] { chaseRoot } },
            };

            var rage = GB.Reaction("decide", false);
            rage.setFlags = new[] { StoryKeys.RageUnlocked };
            rage.fillRage = 100;
            rage.dialogue = DataBuilder.Dialogue("dlg_interlude_rage");
            var luca = GB.Reaction("grab_intel", true);
            luca.requireFlag = StoryKeys.LucaAlly;
            luca.setFlags = new[] { StoryKeys.LovedOneSaved };
            luca.dialogue = DataBuilder.Dialogue("dlg_luca_rescue");
            var lost = GB.Reaction("grab_intel", true);
            lost.forbidFlag = StoryKeys.LucaAlly;
            lost.dialogue = DataBuilder.Dialogue("dlg_sofia_lost");

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_interlude_intro"),
                GB.Shot("Harbor", new Vector3(-40, 12, 40), new Vector3(40, 0, -30), 50),
                GB.Shot("Pier", new Vector3(46, 2, 8), new Vector3(40, 1, -50), 35),
                GB.Shot("Adrian", new Vector3(3, 1.8f, 81), new Vector3(0, 1.5f, 85), 35));
            var dir = GB.Finish(DataBuilder.Mission("interlude_loved_one"));
            dir.introCutscene = intro;
            GB.Save();
        }
    }
}
