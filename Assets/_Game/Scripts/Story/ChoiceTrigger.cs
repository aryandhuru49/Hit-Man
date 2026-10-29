using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Presents a ChoiceDefinition, either when an objective becomes active or when used as an
    /// Interactable ("Decide"). The chosen option's consequences are applied by StoryChoices, and an
    /// option can switch the scene to a branch mission.
    /// </summary>
    public class ChoiceTrigger : Interactable
    {
        public ChoiceDefinition choice;
        [Tooltip("Open automatically when this objective becomes active (empty = only via Interact).")]
        public string openOnObjective;
        public BranchMission[] branches;

        [System.Serializable]
        public class BranchMission
        {
            public string optionId;
            public MissionDefinition mission;
            public GameObject[] activate;
        }

        bool m_done;

        void Start()
        {
            // Reloading a save after the decision: restore the branch objects instead of asking again.
            var previous = choice != null ? GameSession.Instance.Story.GetChoice(choice.id) : null;
            if (previous != null && SceneLoader.Instance.ResumingFromSave)
            {
                m_done = true;
                foreach (var b in branches)
                    if (b.optionId == previous) foreach (var go in b.activate) if (go != null) go.SetActive(true);
                return;
            }
            if (!string.IsNullOrEmpty(openOnObjective))
            {
                MissionManager.Instance.OnObjectiveActivated += o => { if (o.id == openOnObjective) Open(); };
                if (MissionManager.Instance.IsObjectiveActive(openOnObjective)) Open();
            }
        }

        public override string Prompt => choice != null ? choice.prompt : "Decide";
        public override bool CanInteract(PlayerCharacter player) => base.CanInteract(player) && !m_done && string.IsNullOrEmpty(openOnObjective);
        public override void Interact(PlayerCharacter player) => Open();

        public void Open()
        {
            if (m_done || choice == null) return;
            UIRoot.Instance.Choice.Open(choice, option =>
            {
                m_done = true;
                StoryChoices.Apply(choice, option);
                foreach (var b in branches)
                {
                    if (b.optionId != option.id) continue;
                    foreach (var go in b.activate) if (go != null) go.SetActive(true);
                    if (b.mission != null) MissionManager.Instance.StartMission(b.mission, false);
                }
            });
        }
    }
}
