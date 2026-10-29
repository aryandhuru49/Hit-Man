using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Applies the consequences of a chosen option: records it, sets/clears flags, advances objectives, switches mission.</summary>
    public static class StoryChoices
    {
        public static void Apply(ChoiceDefinition choice, ChoiceOption option)
        {
            var story = GameSession.Instance.Story;
            story.RecordChoice(choice.id, option.id);
            foreach (var f in option.setFlags) story.SetFlag(f);
            foreach (var f in option.clearFlags) story.SetFlag(f, false);
            Debug.Log($"[Story] {choice.id} -> {option.id}");
            if (!string.IsNullOrEmpty(option.progressObjective)) MissionManager.Instance.Progress(option.progressObjective);
        }

        public static bool IsAvailable(ChoiceOption option)
        {
            var story = GameSession.Instance.Story;
            if (!string.IsNullOrEmpty(option.requiresFlag) && !story.HasFlag(option.requiresFlag)) return false;
            if (!string.IsNullOrEmpty(option.requiresMissingFlag) && story.HasFlag(option.requiresMissingFlag)) return false;
            return true;
        }
    }
}
