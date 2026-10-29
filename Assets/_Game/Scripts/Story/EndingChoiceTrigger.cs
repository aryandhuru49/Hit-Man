using System.Collections.Generic;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// After the Handler falls: offers only the endings the player has earned, records the pick
    /// and completes the final objective so the scene director moves to the Ending scene.
    /// </summary>
    public class EndingChoiceTrigger : MonoBehaviour
    {
        public string openOnObjective = "final_choice";

        void Start()
        {
            MissionManager.Instance.OnObjectiveActivated += o => { if (o.id == openOnObjective) Open(); };
        }

        public void Open()
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinition>();
            choice.id = StoryKeys.ChoiceEnding;
            choice.prompt = "The Handler is dead. The agency's ledger is in your hands. What now?";
            choice.options = new List<ChoiceOption>();
            foreach (var e in GameDatabase.Instance.endings)
            {
                bool ok = EndingResolver.IsAvailable(e);
                choice.options.Add(new ChoiceOption(e.id, e.title + (ok ? "" : "  (locked)"), ok ? "" : LockedHint(e))
                    { requiresFlag = ok ? null : "__locked__" });
            }
            UIRoot.Instance.Choice.Open(choice, option =>
            {
                GameSession.Instance.Story.RecordChoice(StoryKeys.ChoiceEnding, option.id);
                MissionManager.Instance.Progress(openOnObjective);
            });
        }

        static string LockedHint(EndingDefinition e)
        {
            if (e.minEvidence > 0) return $"Needs {e.minEvidence} pieces of evidence.";
            if (e.requiresNoSpare) return "Needs a ruthless path: no one spared, or frequent Rage.";
            return "";
        }
    }
}
