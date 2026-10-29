using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Talk to an NPC: plays dialogue, then optionally a choice, then advances an objective and/or logs a clue.</summary>
    public class NPCConversation : Interactable
    {
        public string npcName = "Stranger";
        public DialogueSequence dialogue;
        public ChoiceTrigger followUpChoice;
        public string objectiveId;
        public ClueDefinition clueOnTalk;
        public bool oneShot = true;
        public bool requireActiveObjective;

        bool m_talked;

        public override string Prompt => "Talk to " + npcName;

        public override bool CanInteract(PlayerCharacter player)
        {
            if (!base.CanInteract(player) || (oneShot && m_talked)) return false;
            return !requireActiveObjective || MissionManager.Instance.IsObjectiveActive(objectiveId);
        }

        public override void Interact(PlayerCharacter player)
        {
            m_talked = true;
            var look = player.transform.position - transform.position;
            look.y = 0;
            if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);
            DialogueRunner.Play(dialogue, () =>
            {
                if (clueOnTalk != null) JournalManager.Instance.Observe(clueOnTalk);
                if (followUpChoice != null) followUpChoice.Open();
                MissionManager.Instance.Progress(objectiveId);
            });
        }
    }
}
