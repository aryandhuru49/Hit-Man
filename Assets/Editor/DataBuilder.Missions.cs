using ExitClause.Systems;

namespace ExitClause.EditorTools
{
    /// <summary>Mission and objective definitions for every chapter. Objective ids are referenced by the scene builders.</summary>
    public static partial class DataBuilder
    {
        static void BuildMissions()
        {
            var prologue = M("prologue_last_job", "The Last Job", "Prague. One more contract, then freedom.", "Prologue_Prague", "Act0_Riviera", 5000,
                O("move", "Follow the alley to the Hotel Zlata (WASD, Shift to run)"),
                O("sneak", "Sneak through the courtyard (C to crouch)"),
                O("takedown", "Take down the side door guard from behind (F)"),
                O("lockpick", "Pick the service door lock (E)"),
                O("reach_target", "Find Viktor Sorel's suite on the top floor"),
                O("eliminate", "Eliminate Viktor Sorel"),
                O("escape_fight", "Fight your way out: drop the hotel guards", 3),
                O("extraction", "Get in the extraction car (E)"),
                O("drive", "Drive to the bridge rendezvous"));
            prologue.setFlagsOnComplete.Add(StoryKeys.PrologueDone);

            M("act0_normal_life", "Normal Life", "Villa Serena, the Italian Riviera", "Act0_Riviera", "Act1_Mansion", 0,
                O("pension", "Check your pension on the villa laptop"),
                O("market", "Buy groceries at the market"),
                O("market_tail", "The man in the grey jacket: observe him in both aisles and the parking lot (E)"),
                O("sleep_1", "Go home and sleep"),
                O("jog", "Morning jog: reach the three markers on the coast road", 3),
                O("tinted_car", "Note the black sedan on the coast road on three different days", 1, true),
                O("sleep_2", "Go home and sleep"),
                O("coffee", "Meet Sofia at the cafe"),
                O("cafe_watcher", "Observe the man at the window (E)"),
                O("confront_luca", "Talk to the man at the window"),
                O("sleep_3", "Go home and sleep"),
                O("neighbor", "Say hello to the new neighbour"),
                O("mail", "Check the mailbox"),
                O("pin_board", "Pin your observations on the suspect board (J)", 3),
                O("dinner", "Dinner with Sofia at the villa"));

            var ambush = M("act1_ambush", "The Ambush", "Dinner is interrupted.", "Act1_Mansion", "Act1_CoastRoad", 0,
                O("kill_team", "Kill the two agents who broke in", 2),
                O("identify", "Check the dead agent's face"),
                O("phone_kessler", "Take Kessler's phone"),
                O("garage", "Get to the garage and take the car"));
            ambush.setFlagsOnComplete.Add(StoryKeys.AmbushSurvived);

            M("act1_escape", "Coast Road", "Lose them.", "Act1_CoastRoad", "Interlude_Harbor", 0,
                O("chase", "Lose the pursuers on the coast road"));

            M("interlude_loved_one", "The Loved One", "Pier nine. Come alone.", "Interlude_Harbor", "", 0,
                O("reach_harbor", "Get to pier nine"),
                O("decide", "Decide"));
            M("interlude_save", "Save Sofia", "Nothing else matters.", "Interlude_Harbor", "Act2_Safehouse", 0,
                O("free_sofia", "Kill Sofia's captors", 4),
                O("defuse", "Free Sofia (E)"),
                O("escort", "Get Sofia to the car"));
            M("interlude_chase", "The List", "Don't let him get away.", "Interlude_Harbor", "Act2_Safehouse", 0,
                O("chase_attacker", "Run the attacker off the road"),
                O("grab_intel", "Take the list from the wreck"));

            var safe = M("act2_safehouse", "Turning Their Tools", "Genoa safehouse", "Act2_Safehouse", "", 0,
                O("crack_elias", "Crack Elias's phone (smudge marks + feedback)"),
                O("decrypt_elias", "Decrypt Elias's messages (find the cipher shift)"),
                O("crack_kessler", "Crack Kessler's phone"),
                O("decrypt_kessler", "Decrypt Kessler's messages"),
                O("spoof", "Send Hale a fake relocation order (Kessler's phone, Orders tab)", 1, true),
                O("plant_intel", "Plant fake intel about your location (Elias's phone, Orders tab)", 1, true),
                O("travel", "Choose your next target on the map"));
            safe.objectives.Find(o => o.id == "decrypt_kessler").setFlagsOnComplete.Add(StoryKeys.DevicesCracked);

            M("act2_safehouse_return", "The Hunt", "Genoa safehouse", "Act2_Safehouse", "", 0,
                O("spoof", "Send Hale a fake relocation order (Kessler's phone, Orders tab)", 1, true),
                O("plant_intel", "Plant fake intel about your location (Elias's phone, Orders tab)", 1, true),
                O("travel", "Choose your next target on the map"));

            var sniper = M("act2_sniper", "The Sniper", "Swiss Alps", "Act2_Alps", "Act2_Safehouse", 2000,
                O("approach", "Cross the valley to Hale's ridge. Move when the laser drops."),
                O("kill_sniper", "Eliminate Hale"),
                O("nest", "Search Hale's nest"));
            sniper.setFlagsOnComplete.Add(StoryKeys.SniperDefeated);

            var infil = M("act2_infiltrator", "The Infiltrator", "Marseille docks", "Act2_Marseille", "Act2_Safehouse", 2000,
                O("dock_market", "Reach the dock market"),
                O("study_crowd", "Study faces in the crowd for tells", 3),
                O("confront", "Confront the person you believe is Nadia"),
                O("subdue", "Deal with her backup", 3),
                O("decide_nadia", "Decide Nadia's fate"));
            infil.setFlagsOnComplete.Add(StoryKeys.InfiltratorDefeated);

            var tech = M("act2_tech", "The Tech Specialist", "Berlin", "Act2_Berlin", "Act2_Safehouse", 2000,
                O("infiltrate", "Get into the old Kraftwerk without being spotted"),
                O("cameras", "Hack the CCTV relay to blind the cameras", 1, true),
                O("server_room", "Reach the server hall"),
                O("kill_brenner", "Take down Konrad Brenner"),
                O("laptop", "Crack Brenner's laptop"));
            tech.setFlagsOnComplete.Add(StoryKeys.TechDefeated);

            var agency = M("act3_agency", "The Agency", "Zurich", "Act3_Zurich", "Ending", 0,
                O("lobby", "Enter Helvetia Private Trust"),
                O("disguise", "Get a staff uniform from the locker room"),
                O("vault_access", "Hack the executive lift panel"),
                O("ledger", "Collect the ledger files", 3),
                O("directors", "Take down the agency directors", 2),
                O("penthouse", "Go up to the penthouse"),
                O("kill_handler", "Kill Marcus Vale"),
                O("final_choice", "Decide what happens to the agency"));
            agency.setFlagsOnComplete.Add(StoryKeys.HandlerDefeated);
        }
    }
}
