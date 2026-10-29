namespace ExitClause.Systems
{
    /// <summary>Well-known story flag, counter and choice names shared by code, data and scene builders.</summary>
    public static class StoryKeys
    {
        // Flags
        public const string PrologueDone = "prologue_done";
        public const string MansionOwned = "mansion_owned";
        public const string LucaAlly = "luca_ally";
        public const string AmbushSurvived = "ambush_survived";
        public const string LovedOneSaved = "loved_one_saved";
        public const string ChasedAttacker = "chased_attacker";
        public const string RageUnlocked = "rage_unlocked";
        public const string SniperMisdirected = "sniper_misdirected";
        public const string FakeIntelPlanted = "fake_intel_planted";
        public const string DevicesCracked = "devices_cracked";
        public const string SniperDefeated = "sniper_defeated";
        public const string InfiltratorDefeated = "infiltrator_defeated";
        public const string InfiltratorSpared = "infiltrator_spared";
        public const string TechDefeated = "tech_defeated";
        public const string NetworkAccess = "agency_network_access";
        public const string HandlerDefeated = "handler_defeated";

        // Counters
        public const string EvidenceCollected = "evidence_collected";
        public const string RageUses = "rage_uses";
        public const string RageEvidence = "rage_evidence_left";
        public const string TeamKilled = "team_killed";

        // Choices
        public const string ChoiceLovedOne = "choice_loved_one";
        public const string ChoiceSpareInfiltrator = "choice_spare_infiltrator";
        public const string ChoiceLuca = "choice_luca";
        public const string ChoiceEnding = "choice_ending";
    }
}
