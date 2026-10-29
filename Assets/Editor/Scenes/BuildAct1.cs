using System.Collections.Generic;
using ExitClause.AI;
using ExitClause.Story;
using ExitClause.Systems;
using ExitClause.Vehicles;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Act 1. The mansion ambush (dining hall, two team members plus hired guns, identify Elias, take
    /// Kessler's phone, escape to the garage) and the coast road car chase in a separate scene.
    /// </summary>
    public static class BuildAct1
    {
        static readonly Color Warm = new Color(1f, 0.8f, 0.55f);

        public static void BuildMansion()
        {
            GB.NewScene("Act1_Mansion", MoodPreset.RivieraNight);
            GB.Ground(new Vector3(0, 0, 0), new Vector2(160, 160), "Grass");
            GB.Road(new Vector3(30, 0, 0), new Vector3(70, 0, 0), 7);
            GB.Room("DiningHall", Vector3.zero, new Vector2(24, 18), 4.5f, "Plaster", "Marble", 0.5f, 0.5f, -1, 0.5f);
            // Garage: open to the east
            GB.Box("GarageFloor", new Vector3(18, -0.1f, 0), new Vector3(12, 0.2f, 12), "Concrete");
            GB.Wall(new Vector3(12, 0, 6), new Vector3(24, 0, 6), 4, "Plaster");
            GB.Wall(new Vector3(24, 0, -6), new Vector3(12, 0, -6), 4, "Plaster");
            GB.Box("GarageRoof", new Vector3(18, 4.1f, 0), new Vector3(12.3f, 0.2f, 12.3f), "ConcreteDark");
            GB.Box("Table", new Vector3(0, 0.75f, 0), new Vector3(4, 0.1f, 1.6f), "Wood");
            GB.Box("Sideboard", new Vector3(-8, 0.5f, -7.5f), new Vector3(4, 1, 0.8f), "Wood");
            GB.Box("Column1", new Vector3(-5, 2.25f, 4), new Vector3(0.8f, 4.5f, 0.8f), "Marble");
            GB.Box("Column2", new Vector3(5, 2.25f, 4), new Vector3(0.8f, 4.5f, 0.8f), "Marble");
            GB.Box("Column3", new Vector3(-5, 2.25f, -4), new Vector3(0.8f, 4.5f, 0.8f), "Marble");
            GB.Box("Column4", new Vector3(5, 2.25f, -4), new Vector3(0.8f, 4.5f, 0.8f), "Marble");
            GB.Light(new Vector3(0, 4f, 0), Warm, 6000, 16);
            GB.Light(new Vector3(18, 3.6f, 0), new Color(0.9f, 0.95f, 1f), 3000, 12);
            for (int z = -30; z <= 30; z += 20) GB.Lamp(new Vector3(-20, 0, z), Warm, 2500, 15);
            GB.Box("Terrace", new Vector3(0, 0.05f, 14), new Vector3(20, 0.1f, 10), "Terracotta");
            GB.Box("Hedge1", new Vector3(-12, 1, 22), new Vector3(10, 2, 1.5f), "Hedge");
            GB.Box("Hedge2", new Vector3(12, 1, 22), new Vector3(10, 2, 1.5f), "Hedge");

            GB.Player(new Vector3(0, 0, -2), 0);
            var sofia = GB.Npc("Sofia", new Vector3(0, 0, 1.6f), 180, "Fabric");

            var elias = GB.Enemy("Elias Ward", new Vector3(-16, 0, 0), 90, null, "agency_smg", "", "TeamSuit", false);
            var kessler = GB.Enemy("Kessler", new Vector3(0, 0, 13), 180, null, "agency_smg", "", "TeamSuit", false);
            foreach (var e in new[] { elias, kessler })
            {
                e.isTeamMember = true;
                e.stationary = false;
                e.GetComponent<Combat.Health>().maxHealth = 160;
                e.GetComponent<Combat.Health>().current = 160;
                GB.OnDeath(e, "kill_team");
            }
            var id = elias.gameObject.AddComponent<ObjectiveInteract>();
            id.objectiveId = "identify"; id.prompt = "Check his face"; id.giveItemId = "phone_elias";
            id.dialogue = DataBuilder.Dialogue("dlg_act1_elias"); id.addEvidence = 0;
            var ph = kessler.gameObject.AddComponent<ObjectiveInteract>();
            ph.objectiveId = "phone_kessler"; ph.prompt = "Take Kessler's phone"; ph.giveItemId = "phone_kessler";
            ph.notification = "Kessler's phone. Locked, but it will talk eventually.";
            var hired = new List<GameObject> { elias.gameObject, kessler.gameObject };
            Vector3[] hp = { new Vector3(-16, 0, 4), new Vector3(-3, 0, 14), new Vector3(3, 0, 15) };
            for (int i = 0; i < hp.Length; i++)
            {
                var h = GB.Enemy("Hired Gun " + i, hp[i], 180, null, "agency_pistol", "", "EnemySuit", false);
                h.stationary = false;
                hired.Add(h.gameObject);
            }
            GB.Activate("kill_team", ObjectiveActivator.Mode.ActivateWhenStarted, null, hired.ToArray());
            var drive1 = GB.Enemy("Driveway Gun 1", new Vector3(40, 0, 6), 270, null, "agency_smg", "", "EnemySuit", false);
            var drive2 = GB.Enemy("Driveway Gun 2", new Vector3(44, 0, -6), 270, null, "agency_pistol", "", "EnemySuit", false);
            GB.Activate("garage", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_act1_garage"), drive1.gameObject, drive2.gameObject);

            var start = GB.Reaction("kill_team", false);
            start.deactivate = new[] { sofia };
            start.giveItems = new[] { "smg" };
            start.notification = "Grabbed the MP-7 from the hall cabinet (press 2).";

            var car = GB.Car("Villa Car", new Vector3(18, 0, 0), 90, "CarRed");
            car.GetComponent<VehicleEntry>().objectiveOnEnter = "garage";
            GB.Marker("garage", car.transform.position);

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_act1_intro"),
                GB.Shot("Table", new Vector3(3.5f, 1.7f, -3.5f), new Vector3(0, 1.2f, 0.5f), 35),
                GB.Shot("Toast", new Vector3(-2.5f, 1.5f, 3), new Vector3(0, 1.3f, -1), 30),
                GB.Shot("Window", new Vector3(0, 3, 10), new Vector3(0, 1.2f, 0), 50));
            var dir = GB.Finish(DataBuilder.Mission("act1_ambush"));
            dir.introCutscene = intro;
            GB.Save();
        }

        public static void BuildCoastRoad()
        {
            GB.NewScene("Act1_CoastRoad", MoodPreset.RivieraNight);
            GB.Ground(new Vector3(300, 0, 45), new Vector2(720, 110), "Grass");
            GB.Box("Sea", new Vector3(300, -2, -60), new Vector3(800, 1, 100), "Water");
            Vector3[] pts =
            {
                new Vector3(-60, 0, 0), new Vector3(80, 0, 0), new Vector3(140, 0, 15), new Vector3(200, 0, 10), new Vector3(260, 0, 0),
                new Vector3(330, 0, 5), new Vector3(400, 0, 20), new Vector3(470, 0, 10), new Vector3(540, 0, 2), new Vector3(640, 0, 2)
            };
            for (int i = 0; i + 1 < pts.Length; i++)
            {
                GB.Road(pts[i], pts[i + 1], 10);
                Vector3 d = (pts[i + 1] - pts[i]).normalized;
                Vector3 seaSide = Vector3.Cross(d, Vector3.up);
                if (seaSide.z > 0) seaSide = -seaSide;
                GB.Wall(pts[i] + seaSide * 5.8f, pts[i + 1] + seaSide * 5.8f, 0.8f, "Concrete", 0.3f);
                Vector3 mid = (pts[i] + pts[i + 1]) * 0.5f - seaSide * 18f;
                GB.Box("Cliff" + i, mid + Vector3.up * 8, new Vector3(20, 16, Vector3.Distance(pts[i], pts[i + 1]) * 0.8f), "Rock",
                    null, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg);
                GB.Lamp(pts[i] - seaSide * 6f, Warm, 3000, 22, 6f);
            }
            // Tunnel near the end
            GB.Box("TunnelWallL", new Vector3(590, 3, 8.5f), new Vector3(80, 6, 1), "ConcreteDark");
            GB.Box("TunnelWallR", new Vector3(590, 3, -4.5f), new Vector3(80, 6, 1), "ConcreteDark");
            GB.Box("TunnelRoof", new Vector3(590, 6.5f, 2), new Vector3(80, 1, 15), "Rock");
            for (int x = 560; x <= 620; x += 15) GB.Light(new Vector3(x, 5.5f, 2), new Color(1f, 0.6f, 0.3f), 3000, 12);
            // Obstacles
            GB.Car("Parked A", new Vector3(120, 0, 7), 80, "CarSilver", false);
            GB.Car("Parked B", new Vector3(300, 0, -3), 85, "CarBlue", false);
            GB.Car("Parked C", new Vector3(450, 0, 15), 100, "CarSilver", false);

            GB.Player(new Vector3(4, 0, -3.5f), 90);
            var playerCar = GB.Car("Villa Car", new Vector3(6, 0, 0), 90, "CarRed");
            var pursuers = new List<AIDriver>();
            float[] xs = { -22, -34, -46 };
            for (int i = 0; i < 3; i++)
            {
                var c = GB.Car("Pursuer " + i, new Vector3(xs[i], 0, i % 2 == 0 ? 2 : -2), 90, "CarBlack", false);
                var ai = c.GetComponent<AIDriver>();
                ai.cruiseSpeed = 25 + i * 2;
                ai.gunmanWeapon = DataBuilder.Weapon("agency_smg");
                pursuers.Add(ai);
            }
            var chaseGo = new GameObject("Chase");
            chaseGo.transform.SetParent(GB.Logic);
            var chase = chaseGo.AddComponent<ChaseSequence>();
            chase.kind = ChaseSequence.ChaseKind.Escape;
            chase.playerCar = playerCar;
            chase.pursuers = pursuers.ToArray();
            chase.objectiveId = "chase";
            var finish = GB.ObjTrigger("chase", new Vector3(625, 2, 2), new Vector3(8, 6, 14), true, true);
            finish.finishesChase = chase;

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_coast_intro"),
                GB.Shot("Behind", new Vector3(-60, 4, 6), new Vector3(0, 1, 0), 40),
                GB.Shot("Front", new Vector3(30, 2, -4), new Vector3(0, 1, 0), 35));
            var outro = GB.Cutscene("Outro", DataBuilder.Dialogue("dlg_act1_outro"),
                GB.Shot("TunnelExit", new Vector3(660, 3, 10), new Vector3(630, 1, 2), 40),
                GB.Shot("Sea", new Vector3(620, 8, -30), new Vector3(640, 0, 2), 50));
            var dir = GB.Finish(DataBuilder.Mission("act1_escape"));
            dir.introCutscene = intro;
            dir.outroCutscene = outro;
            GB.Save();
        }
    }
}
