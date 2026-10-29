using System.Collections.Generic;
using ExitClause.Systems;

namespace ExitClause.Story
{
    /// <summary>
    /// Decides which endings the player has earned from the story state:
    /// Expose needs enough evidence; Take Over needs a ruthless run (no one spared or heavy Rage use);
    /// Disappear is always possible.
    /// </summary>
    public static class EndingResolver
    {
        public const string Expose = "ending_expose";
        public const string TakeOver = "ending_takeover";
        public const string Disappear = "ending_disappear";

        public static bool IsAvailable(EndingDefinition e)
        {
            var s = GameSession.Instance.Story;
            if (s.GetCounter(StoryKeys.EvidenceCollected) < e.minEvidence) return false;
            bool ruthless = !s.HasFlag(StoryKeys.InfiltratorSpared) || s.GetCounter(StoryKeys.RageUses) >= e.minRageUses;
            if (e.requiresNoSpare && !ruthless) return false;
            return true;
        }

        public static List<EndingDefinition> Available()
        {
            var list = new List<EndingDefinition>();
            foreach (var e in GameDatabase.Instance.endings) if (e != null && IsAvailable(e)) list.Add(e);
            return list;
        }

        /// <summary>Full epilogue text with the loved-one and informant variations stitched in.</summary>
        public static string BuildEpilogue(EndingDefinition e)
        {
            var s = GameSession.Instance.Story;
            string text = e.epilogue;
            text += "\n\n" + (s.HasFlag(StoryKeys.LovedOneSaved) ? e.lovedOneSavedLine : e.lovedOneLostLine);
            if (s.HasFlag(StoryKeys.InfiltratorSpared)) text += "\n\n" + e.informantLine;
            text += $"\n\nEvidence secured: {s.GetCounter(StoryKeys.EvidenceCollected)}   Rage episodes: {s.GetCounter(StoryKeys.RageUses)}   Team eliminated: {s.GetCounter(StoryKeys.TeamKilled)}/5";
            return text;
        }
    }
}
