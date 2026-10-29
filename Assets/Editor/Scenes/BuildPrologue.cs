using ExitClause.Combat;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Prologue, Prague on a rainy night. Tutorial path: alley (move) -> courtyard (sneak past a patrol)
    /// -> side door guard (takedown) -> locked service door (lockpick) -> hotel lobby -> stairs -> Sorel's
    /// suite (cutscene, eliminate) -> guards rush in (combat) -> back exit -> extraction car -> drive to the bridge.
    /// </summary>
    public static class BuildPrologue
    {
        static readonly Color Sodium = new Color(1f, 0.7f, 0.4f);

        public static void Build()
        {
            GB.NewScene("Prologue_Prague", MoodPreset.PragueNight);
            GB.Ground(new Vector3(0, 0, 90), new Vector2(140, 280), "WetAsphalt");
            GB.Box("Cobbles", new Vector3(0, 0.01f, 46), new Vector3(30, 0.02f, 30), "Cobblestone");

            // Alley
            GB.Building("AlleyLeft", new Vector3(-10, 0, 15), new Vector3(12, 16, 34), "Brick");
            GB.Building("AlleyRight", new Vector3(10, 0, 15), new Vector3(12, 14, 34), "Plaster");
            GB.Lamp(new Vector3(-3.5f, 0, 10), Sodium, 2500, 14);
            GB.Lamp(new Vector3(3.5f, 0, 24), Sodium, 2500, 14);
            GB.Crate(new Vector3(-2.5f, 0, 18)); GB.Crate(new Vector3(-2.8f, 1, 18.2f), 0.8f);
            GB.Player(new Vector3(0, 0, 2), 0);
            GB.ObjTrigger("move", new Vector3(0, 1.5f, 29), new Vector3(8, 3, 2));

            // Courtyard walls and cover
            GB.Building("CourtLeft", new Vector3(-23, 0, 47), new Vector3(16, 18, 34), "Brick");
            GB.Building("CourtRight", new Vector3(23, 0, 47), new Vector3(16, 18, 34), "Plaster");
            GB.Box("Planter1", new Vector3(-12, 0.5f, 40), new Vector3(3, 1, 1.2f), "Concrete");
            GB.Box("Planter2", new Vector3(-12, 0.5f, 50), new Vector3(3, 1, 1.2f), "Concrete");
            GB.Box("Fountain", new Vector3(0, 0.6f, 46), new Vector3(4, 1.2f, 4), "Concrete");
            GB.Box("Van", new Vector3(10, 1.2f, 54), new Vector3(2.2f, 2.4f, 5), "ConcreteDark");
            GB.Lamp(new Vector3(0, 0, 38), Sodium, 3500, 16);
            var courtRoute = GB.Route("Courtyard", false, new Vector3(-4, 0, 52), new Vector3(10, 0, 52), new Vector3(10, 0, 40));
            GB.Enemy("Courtyard Guard", new Vector3(10, 0, 52), 90, courtRoute, "agency_pistol", "guard");
            GB.ObjTrigger("sneak", new Vector3(-12, 1.5f, 60), new Vector3(5, 3, 3));

            // Side door guard (faces east, approach from the west)
            var sideGuard = GB.Enemy("Side Door Guard", new Vector3(-6.8f, 0, 62.3f), 90, null, "agency_pistol", "guard");
            sideGuard.dropsDisguise = "guard";
            sideGuard.GetComponent<Stealth.AIPerception>().viewAngle = 80;
            GB.OnDeath(sideGuard, "takedown");
            GB.Marker("takedown", sideGuard.transform.position);

            // Hotel ground floor: south wall has the locked service door at x -8.4
            BuildHotel();

            // Extraction alley and road to the bridge
            GB.Building("BackLeft", new Vector3(-16, 0, 120), new Vector3(14, 15, 50), "Brick");
            GB.Building("BackRight", new Vector3(16, 0, 120), new Vector3(14, 12, 50), "Plaster");
            GB.Building("FarLeft", new Vector3(-16, 0, 175), new Vector3(14, 18, 40), "PlasterBlue");
            GB.Building("FarRight", new Vector3(16, 0, 175), new Vector3(14, 15, 40), "Brick");
            for (int z = 100; z < 215; z += 18) { GB.Lamp(new Vector3(-6, 0, z), Sodium, 3000, 16); GB.Lamp(new Vector3(6, 0, z + 9), Sodium, 3000, 16); }
            var car = GB.Car("Extraction Car", new Vector3(0, 0, 98), 0, "CarBlack");
            car.GetComponent<Vehicles.VehicleEntry>().objectiveOnEnter = "extraction";
            GB.Marker("extraction", car.transform.position);
            GB.Box("BridgeDeck", new Vector3(0, 0.15f, 212), new Vector3(16, 0.3f, 24), "Cobblestone");
            GB.Box("BridgeRailL", new Vector3(-8, 0.7f, 212), new Vector3(0.4f, 1.2f, 24), "Concrete");
            GB.Box("BridgeRailR", new Vector3(8, 0.7f, 212), new Vector3(0.4f, 1.2f, 24), "Concrete");
            GB.ObjTrigger("drive", new Vector3(0, 1.5f, 205), new Vector3(14, 4, 6), true, true);
            var vale = GB.Npc("Marcus Vale", new Vector3(4, 0.3f, 214), 200, "TeamSuit");
            GB.Car("Vale's Car", new Vector3(5, 0.3f, 219), 90, "CarSilver", false);

            // Radio, reactions, cutscenes
            GB.Activate("takedown", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_prologue_takedown"));
            GB.Activate("lockpick", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_prologue_lockpick"));
            GB.Activate("extraction", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_prologue_drive"));
            var r = GB.Reaction("drive", true);
            r.setPension = 2400000f;
            r.grantMansion = true;

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_prologue_intro"),
                GB.Shot("Rooftops", new Vector3(14, 22, -14), new Vector3(0, 2, 30), 45),
                GB.Shot("Alley", new Vector3(1.5f, 1.8f, 7), new Vector3(0, 1.4f, 1)),
                GB.Shot("Courtyard", new Vector3(-6, 6, 30), new Vector3(10, 1, 52)));
            var sorel = GB.Cutscene("Sorel", DataBuilder.Dialogue("dlg_prologue_sorel"),
                GB.Shot("Sorel1", new Vector3(3, 6, 80), new Vector3(0, 5.8f, 86)),
                GB.Shot("Sorel2", new Vector3(-2.5f, 5.9f, 83), new Vector3(0, 5.8f, 86), 35));
            GB.CutsceneOn("eliminate", false, sorel);
            var outro = GB.Cutscene("Outro", DataBuilder.Dialogue("dlg_prologue_outro"),
                GB.Shot("Bridge1", new Vector3(9, 3, 203), new Vector3(4, 1.6f, 214)),
                GB.Shot("Bridge2", new Vector3(1.5f, 1.8f, 210), new Vector3(4, 1.6f, 214), 32),
                GB.Shot("BridgeWide", new Vector3(-12, 12, 190), new Vector3(4, 0, 214), 50));

            var dir = GB.Finish(DataBuilder.Mission("prologue_last_job"));
            dir.introCutscene = intro;
            dir.outroCutscene = outro;
            dir.openingRadio = null;
            GB.Save();
        }

        static void BuildHotel()
        {
            var c = new Vector3(0, 0, 77);
            GB.Box("HotelFloor", c - Vector3.up * 0.1f, new Vector3(28, 0.2f, 26), "MarbleDark");
            // South wall with service door gap at t=0.8 (x = -8.4)
            GB.Wall(new Vector3(14, 0, 64), new Vector3(-14, 0, 64), 4, "Plaster", 0.3f, 0.8f);
            GB.Wall(new Vector3(-14, 0, 64), new Vector3(-14, 0, 90), 4, "Plaster");
            GB.Wall(new Vector3(14, 0, 90), new Vector3(14, 0, 64), 4, "Plaster");
            // North wall with exit at x = 0
            GB.Wall(new Vector3(-14, 0, 90), new Vector3(14, 0, 90), 4, "Plaster", 0.3f, 0.5f);
            GB.Door(new Vector3(-9.2f, 0, 64), 0, 1.6f, true, "lockpick");
            GB.Marker("lockpick", new Vector3(-8.4f, 0, 63.5f));
            GB.Door(new Vector3(-0.8f, 0, 90), 0, 1.6f, false);
            // Upper floor outer walls and roof
            GB.Wall(new Vector3(14, 4.2f, 64), new Vector3(-14, 4.2f, 64), 3.6f, "Plaster");
            GB.Wall(new Vector3(-14, 4.2f, 64), new Vector3(-14, 4.2f, 90), 3.6f, "Plaster");
            GB.Wall(new Vector3(-14, 4.2f, 90), new Vector3(14, 4.2f, 90), 3.6f, "Plaster");
            GB.Wall(new Vector3(14, 4.2f, 90), new Vector3(14, 4.2f, 64), 3.6f, "Plaster");
            GB.Box("HotelRoof", new Vector3(0, 7.95f, 77), new Vector3(28.6f, 0.3f, 26.6f), "ConcreteDark");

            // Lobby furniture, cover and a hiding spot
            GB.Box("Reception", new Vector3(-4, 0.55f, 72), new Vector3(5, 1.1f, 1), "Wood");
            GB.Box("Sofa1", new Vector3(4, 0.45f, 70), new Vector3(3, 0.9f, 1), "Fabric");
            GB.Box("Sofa2", new Vector3(-8, 0.45f, 82), new Vector3(3, 0.9f, 1), "Fabric");
            GB.Box("Pillar1", new Vector3(-4, 2, 78), new Vector3(0.8f, 4, 0.8f), "Marble");
            GB.Box("Pillar2", new Vector3(4, 2, 78), new Vector3(0.8f, 4, 0.8f), "Marble");
            GB.Hide(new Vector3(-12.8f, 0, 76), 90);
            GB.Light(new Vector3(0, 3.6f, 72), new Color(1f, 0.85f, 0.65f), 4000, 14);
            GB.Light(new Vector3(0, 3.6f, 84), new Color(1f, 0.85f, 0.65f), 4000, 14);
            var lobbyRoute = GB.Route("Lobby", true, new Vector3(-8, 0, 70), new Vector3(8, 0, 70), new Vector3(8, 0, 86), new Vector3(-8, 0, 86));
            GB.Enemy("Lobby Guard", new Vector3(-8, 0, 70), 90, lobbyRoute, "agency_pistol", "guard");

            // Stairs up the east side to the suite at y 4.2, with a stairwell opening
            GB.Stairs(new Vector3(10.5f, 0, 69.5f), Vector3.forward, 4.2f, 2.2f, "Marble");
            GB.Box("UpperA", new Vector3(-2.6f, 4.1f, 77), new Vector3(22.8f, 0.2f, 26), "MarbleDark");
            GB.Box("UpperB", new Vector3(11.4f, 4.1f, 82.7f), new Vector3(5.2f, 0.2f, 14.6f), "MarbleDark");
            GB.Box("UpperC", new Vector3(12.8f, 4.1f, 69.7f), new Vector3(2.4f, 0.2f, 11.4f), "MarbleDark");
            GB.Box("Railing", new Vector3(8.9f, 4.7f, 69.7f), new Vector3(0.1f, 1f, 11.4f), "Metal");

            // Suite
            GB.Wall(new Vector3(-14, 4.2f, 80), new Vector3(8.8f, 4.2f, 80), 3.6f, "Plaster", 0.3f, 0.5f);
            GB.Box("Desk", new Vector3(0, 4.7f, 87), new Vector3(3, 0.9f, 1.2f), "Wood");
            GB.Light(new Vector3(0, 7.4f, 85), new Color(1f, 0.8f, 0.6f), 3000, 10);
            var sorel = GB.Npc("Viktor Sorel", new Vector3(0, 4.2f, 86.2f), 180, "Fabric");
            var h = sorel.AddComponent<Health>();
            h.maxHealth = 60; h.current = 60;
            sorel.AddComponent<LockOnTarget>();
            GB.OnDeath(h, "eliminate").skipAheadIfEarly = true;
            GB.Marker("eliminate", sorel.transform.position);
            // Spans the whole upper landing in front of the suite wall (z 75.5..80), so every route from the
            // stairs to the suite door passes through it. The old 4 m box at the stair top was easy to walk around.
            GB.ObjTrigger("reach_target", new Vector3(0, 5.45f, 77.75f), new Vector3(28f, 2.5f, 4.5f));

            // Escape fight: guards appear when Sorel dies
            var g1 = GB.Enemy("Hotel Guard 1", new Vector3(-6, 4.2f, 74), 0, null, "agency_smg", "", "EnemySuit", false);
            var g2 = GB.Enemy("Hotel Guard 2", new Vector3(6, 0, 72), 0, null, "agency_pistol", "", "EnemySuit", false);
            var g3 = GB.Enemy("Hotel Guard 3", new Vector3(-6, 0, 86), 180, null, "agency_pistol", "", "EnemySuit", false);
            foreach (var g in new[] { g1, g2, g3 })
            {
                g.stationary = false;
                g.GetComponent<Stealth.AIPerception>().Suspicion = 0.9f;
                GB.OnDeath(g, "escape_fight");
            }
            GB.Activate("escape_fight", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_prologue_escape"),
                g1.gameObject, g2.gameObject, g3.gameObject);
        }
    }
}
