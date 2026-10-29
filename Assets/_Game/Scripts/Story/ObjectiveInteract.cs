using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Generic "use this" story object: a document, a phone on a body, a car key, a dinner table.
    /// Advances an objective, can give an item, set a flag, add evidence and play a line of dialogue.
    /// </summary>
    public class ObjectiveInteract : Interactable
    {
        public string objectiveId;
        [Tooltip("Only usable while the objective is active.")]
        public bool requireActiveObjective = true;
        public bool oneShot = true;
        public bool hideAfterUse;
        public string giveItemId;
        public string setFlag;
        public int addEvidence;
        public int addCash;
        public DialogueSequence dialogue;
        [TextArea] public string notification;

        bool m_used;

        public override bool CanInteract(PlayerCharacter player)
        {
            if (!base.CanInteract(player) || (oneShot && m_used)) return false;
            return !requireActiveObjective || string.IsNullOrEmpty(objectiveId) || MissionManager.Instance.IsObjectiveActive(objectiveId);
        }

        public override void Interact(PlayerCharacter player)
        {
            m_used = true;
            if (!string.IsNullOrEmpty(giveItemId))
            {
                var item = GameDatabase.Instance.GetItem(giveItemId);
                if (item != null) InventoryManager.Instance.Add(item, 1); else InventoryManager.Instance.Add(giveItemId, 1);
            }
            var story = GameSession.Instance.Story;
            if (!string.IsNullOrEmpty(setFlag)) story.SetFlag(setFlag);
            if (addEvidence != 0)
            {
                story.AddCounter(StoryKeys.EvidenceCollected, addEvidence);
                UIRoot.Instance.HUD.Notify($"Evidence secured ({story.GetCounter(StoryKeys.EvidenceCollected)})");
            }
            if (addCash != 0) EconomyManager.Instance.AddCash(addCash, name);
            if (!string.IsNullOrEmpty(notification)) UIRoot.Instance.HUD.Notify(notification);
            if (dialogue != null) DialogueRunner.Play(dialogue);
            MissionManager.Instance.Progress(objectiveId);
            if (hideAfterUse) gameObject.SetActive(false);
        }
    }
}
