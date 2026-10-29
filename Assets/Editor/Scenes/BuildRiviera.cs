using ExitClause.AI;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEditor;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Act 0, the Riviera town at golden hour. Villa Serena (laptop, bed, dinner table, mailbox), the market
    /// (two aisles and a parking lot for the tail), the coast road jog with the tinted car, the cafe with
    /// Sofia and the watcher (Luca), the new neighbour, a gadget dealer and strolling civilians.
    /// Day-specific actors are switched on by DayActivators.
    /// </summary>
    public static class BuildRiviera
    {
        public static void Build()
        {
            GB.NewScene("Act0_Riviera", MoodPreset.RivieraGolden);
            GB.Box("Sea", new Vector3(0, -1.2f, -120), new Vector3(500, 1, 200), "Water");
            GB.Ground(new Vector3(0, 0, 60), new Vector2(360, 160), "Sand");
            GB.Road(new Vector3(-170, 0, -8), new Vector3(170, 0, -8), 9);
            GB.Road(new Vector3(0, 0, -4), new Vector3(0, 0, 30), 6, "Cobblestone");
            GB.Road(new Vector3(-50, 0, 5), new Vector3(50, 0, 5), 6, "Cobblestone");
            GB.Box("SeaWall", new Vector3(0, 0.4f, -13), new Vector3(340, 0.8f, 0.6f), "Concrete");

            // Villa Serena
            GB.Room("Villa", new Vector3(0, 0, 42), new Vector2(18, 14), 3.6f, "Plaster", "Terracotta", -1, -1, 0.5f, -1);
            GB.Box("Garden", new Vector3(0, 0.02f, 28), new Vector3(20, 0.04f, 12), "Grass");
            GB.Box("LemonTree1", new Vector3(-7, 1.5f, 27), new Vector3(2, 3, 2), "Hedge");
            GB.Box("LemonTree2", new Vector3(7, 1.5f, 27), new Vector3(2, 3, 2), "Hedge");
            GB.Light(new Vector3(0, 3.2f, 42), new Color(1f, 0.85f, 0.65f), 3500, 12);
            var laptop = GB.Prop<BankTerminal>("Laptop", new Vector3(-6, 0.95f, 46), new Vector3(0.5f, 0.05f, 0.35f), "ScreenGreen", "pension");
            laptop.objectiveOnUse = "pension";
            GB.Box("LaptopDesk", new Vector3(-6, 0.45f, 46), new Vector3(1.6f, 0.9f, 0.8f), "Wood");
            var bed = GB.Prop<SleepBed>("Bed", new Vector3(6, 0.35f, 46), new Vector3(2, 0.7f, 2.4f), "Fabric", null);
            var wake = new GameObject("WakePoint");
            wake.transform.SetPositionAndRotation(new Vector3(4, 0, 43), Quaternion.Euler(0, 180, 0));
            bed.wakePoint = wake.transform;
            bed.objectiveId = "sleep";
            foreach (var d in new[] { 1, 2, 3 }) GB.Marker("sleep_" + d, bed.transform.position);
            var table = GB.Prop<ObjectiveInteract>("DinnerTable", new Vector3(0, 0.8f, 40), new Vector3(3, 0.1f, 1.4f), "Wood", "dinner");
            table.objectiveId = "dinner";
            table.prompt = "Sit down for dinner";
            var mailbox = GB.Prop<ObservableClue>("Mailbox", new Vector3(3, 1.1f, 23), new Vector3(0.4f, 0.4f, 0.3f), "Metal", "mail");
            mailbox.clue = DataBuilder.Clue("clue_mail");
            mailbox.days = new[] { 4 };
            mailbox.range = 3f;
            GB.Box("MailPost", new Vector3(3, 0.45f, 23), new Vector3(0.1f, 0.9f, 0.1f), "Metal");
            GB.Player(new Vector3(0, 0, 33), 180);

            // Neighbour house
            GB.Building("NeighbourHouse", new Vector3(24, 0, 42), new Vector3(14, 7, 12), "PlasterPink");
            var claire = GB.Npc("Claire (neighbour)", new Vector3(17, 0, 33), 250, "Fabric");
            var cTalk = claire.AddComponent<NPCConversation>();
            cTalk.npcName = "Claire";
            cTalk.dialogue = DataBuilder.Dialogue("dlg_neighbor");
            cTalk.clueOnTalk = DataBuilder.Clue("clue_neighbor");
            cTalk.objectiveId = "neighbor";
            cTalk.requireActiveObjective = true;
            GB.Marker("neighbor", claire.transform.position);

            // Market (two aisles) + parking
            GB.Room("Market", new Vector3(-45, 0, 25), new Vector2(26, 16), 4.5f, "PlasterBlue", "Concrete", -1, 0.5f, 0.3f, -1);
            for (int i = 0; i < 3; i++)
            {
                GB.Box("ShelfProduce" + i, new Vector3(-53 + i * 3.5f, 0.9f, 27), new Vector3(1, 1.8f, 7), "Wood");
                GB.Box("ShelfWine" + i, new Vector3(-42 + i * 3.5f, 0.9f, 27), new Vector3(1, 1.8f, 7), "Wood");
            }
            GB.Trigger<ZoneVolume>("Zone_Produce", new Vector3(-52, 1.5f, 25), new Vector3(12, 3, 15)).zoneId = "produce aisle";
            GB.Trigger<ZoneVolume>("Zone_Wine", new Vector3(-38.5f, 1.5f, 25), new Vector3(12, 3, 15)).zoneId = "wine aisle";
            GB.Trigger<ZoneVolume>("Zone_Parking", new Vector3(-45, 1.5f, 5), new Vector3(26, 3, 14)).zoneId = "parking lot";
            GB.Box("Parking", new Vector3(-45, 0.03f, 5), new Vector3(26, 0.04f, 14), "Asphalt");
            GB.Car("Parked1", new Vector3(-54, 0, 5), 0, "CarSilver", false);
            GB.Car("Parked2", new Vector3(-36, 0, 5), 0, "CarBlue", false);
            var shop = GB.Prop<ShopTerminal>("MarketCounter", new Vector3(-45, 0.6f, 31.5f), new Vector3(4, 1.2f, 1), "Wood", "market");
            shop.shopName = "Mercato";
            shop.objectiveOnUse = "market";
            GB.Light(new Vector3(-45, 4, 25), new Color(1f, 0.95f, 0.85f), 5000, 18);
            BuildTail();

            // Gadget dealer in a garage
            GB.Building("DealerGarage", new Vector3(-20, 0, 52), new Vector3(10, 5, 8), "Brick");
            var dealer = GB.Prop<ShopTerminal>("Dealer", new Vector3(-20, 0.6f, 47.3f), new Vector3(2, 1.2f, 0.6f), "Metal");
            dealer.shopName = "Renzo's Garage (gadgets)";

            // Cafe
            GB.Room("Cafe", new Vector3(45, 0, 24), new Vector2(14, 10), 3.6f, "Plaster", "Wood", -1, -1, 0.5f, -1);
            for (int i = 0; i < 3; i++) GB.Box("Table" + i, new Vector3(40 + i * 5, 0.4f, 15), new Vector3(1, 0.8f, 1), "Metal");
            GB.Box("WindowTable", new Vector3(48, 0.4f, 21), new Vector3(1, 0.8f, 1), "Metal");
            GB.Light(new Vector3(45, 3.2f, 24), new Color(1f, 0.8f, 0.6f), 3000, 12);
            var sofia = GB.Npc("Sofia (cafe)", new Vector3(40, 0, 14), 0, "Fabric");
            var sTalk = sofia.AddComponent<NPCConversation>();
            sTalk.npcName = "Sofia";
            sTalk.dialogue = DataBuilder.Dialogue("dlg_sofia_cafe");
            sTalk.objectiveId = "coffee";
            sTalk.requireActiveObjective = true;
            GB.Marker("coffee", sofia.transform.position);
            var luca = GB.Npc("Luca Brandt", new Vector3(48, 0, 20.2f), 200, "Civilian");
            var head = new GameObject("Focus");
            head.transform.SetParent(luca.transform, false);
            head.transform.localPosition = Vector3.up * 1.6f;
            var watch = luca.AddComponent<ObservableClue>();
            watch.clue = DataBuilder.Clue("clue_cafe_watcher");
            watch.focusPoint = head.transform;
            watch.range = 15f;
            var choiceGo = new GameObject("LucaChoice");
            choiceGo.transform.SetParent(luca.transform, false);
            var lucaChoice = choiceGo.AddComponent<ChoiceTrigger>();
            lucaChoice.choice = DataBuilder.Choice(StoryKeys.ChoiceLuca);
            lucaChoice.enabled = true;
            lucaChoice.range = 0f;
            var lTalk = luca.AddComponent<NPCConversation>();
            lTalk.npcName = "the man at the window";
            lTalk.dialogue = DataBuilder.Dialogue("dlg_luca");
            lTalk.followUpChoice = lucaChoice;
            lTalk.objectiveId = "confront_luca";
            lTalk.requireActiveObjective = true;
            GB.Marker("cafe_watcher", luca.transform.position);
            GB.Marker("confront_luca", luca.transform.position);
            Days(new[] { 3 }, sofia, luca);
            Days(new[] { 4 }, claire);

            // Jog route along the coast road, and the tinted car (days 2-4)
            foreach (var x in new[] { -70f, 20f, 100f })
            {
                var t = GB.ObjTrigger("jog", new Vector3(x, 1.5f, -8), new Vector3(6, 3, 9));
                t.oneShot = true;
            }
            var tinted = GB.Car("Tinted Sedan", new Vector3(60, 0, -2.5f), 270, "CarBlack", false);
            tinted.GetComponent<Rigidbody>().isKinematic = true;
            var tClue = tinted.gameObject.AddComponent<ObservableClue>();
            tClue.clue = DataBuilder.Clue("clue_tinted_car");
            tClue.range = 18f;
            Days(new[] { 2, 3, 4 }, tinted);

            // Sofia at the villa for dinner (day 4)
            var sofiaHome = GB.Npc("Sofia (villa)", new Vector3(1.5f, 0, 38.5f), 0, "Fabric");
            Days(new[] { 4 }, sofiaHome);

            // Town houses and strolling locals
            string[] colors = { "Plaster", "PlasterPink", "PlasterBlue", "Terracotta" };
            for (int i = 0; i < 10; i++)
            {
                float x = -150 + i * 30;
                if (Mathf.Abs(x - 0) < 15 || Mathf.Abs(x + 45) < 18 || Mathf.Abs(x - 45) < 12) continue;
                GB.Building("House" + i, new Vector3(x, 0, 70 + (i % 3) * 8), new Vector3(14, 8 + (i % 4) * 2, 12), colors[i % 4]);
            }
            var strollA = GB.Route("StrollA", true, new Vector3(-30, 0, 5), new Vector3(30, 0, 5), new Vector3(30, 0, 12), new Vector3(-30, 0, 12));
            var strollB = GB.Route("StrollB", true, new Vector3(-100, 0, -3), new Vector3(100, 0, -3));
            for (int i = 0; i < 4; i++) GB.Civilian("Local" + i, new Vector3(-25 + i * 15, 0, 6), i % 2 == 0 ? strollA : strollB);

            // Radio thoughts
            GB.Activate("market_tail", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_act0_market"));
            GB.Activate("jog", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_act0_jog"));
            GB.Activate("pin_board", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_act0_board"));
            GB.Activate("dinner", ObjectiveActivator.Mode.ActivateWhenStarted, DataBuilder.Dialogue("dlg_act0_dinner"));

            var intro = GB.Cutscene("Intro", DataBuilder.Dialogue("dlg_act0_intro"),
                GB.Shot("SeaView", new Vector3(-20, 6, -25), new Vector3(0, 2, 40), 50),
                GB.Shot("Villa", new Vector3(8, 2.5f, 22), new Vector3(0, 2, 40)),
                GB.Shot("Town", new Vector3(30, 10, -15), new Vector3(-30, 0, 30), 55));
            var dir = GB.Finish(DataBuilder.Mission("act0_normal_life"));
            dir.introCutscene = intro;
            GB.Save();
        }

        static void Days(int[] days, params Object[] objs)
        {
            var go = new GameObject("Days_" + string.Join("_", days));
            go.transform.SetParent(GB.Logic);
            var d = go.AddComponent<DayActivator>();
            d.days = days;
            var list = new System.Collections.Generic.List<GameObject>();
            foreach (var o in objs) list.Add(o is Component c ? c.gameObject : (GameObject)o);
            d.targets = list.ToArray();
        }

        /// <summary>The grey-jacket man: a Civilian prefab re-purposed as a FollowerAI with an observable clue.</summary>
        static void BuildTail()
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(PrefabBuilder.Load("Civilian"));
            go.name = "Grey Jacket (tail)";
            go.transform.SetParent(GB.Actors, true);
            go.transform.position = new Vector3(-30, 0, 12);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(go.GetComponent<CivilianAI>());
            foreach (var r in go.transform.Find("Body").GetComponentsInChildren<Renderer>())
                if (r.name != "Head") r.sharedMaterial = MaterialLibrary.Get("ConcreteDark");
            var f = go.AddComponent<FollowerAI>();
            f.activeDuringObjective = "market_tail";
            var focus = new GameObject("Focus");
            focus.transform.SetParent(go.transform, false);
            focus.transform.localPosition = Vector3.up * 1.6f;
            var clue = go.AddComponent<ObservableClue>();
            clue.clue = DataBuilder.Clue("clue_market_tail");
            clue.distinctZones = true;
            clue.range = 18f;
            clue.focusPoint = focus.transform;
            Days(new[] { 1 }, go);
        }
    }
}
