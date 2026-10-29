using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Shows or hides scene objects as the mission moves on: spawn the guards when an objective starts,
    /// open the exit when it completes, and so on.
    /// </summary>
    public class ObjectiveActivator : MonoBehaviour
    {
        public enum Mode { ActiveWhileObjectiveActive, ActivateWhenStarted, ActivateWhenCompleted, DeactivateWhenCompleted }

        public string objectiveId;
        public Mode mode = Mode.ActivateWhenStarted;
        public GameObject[] targets;
        [Tooltip("Optional radio line played when this fires.")]
        public DialogueSequence dialogue;

        void Start()
        {
            var mm = MissionManager.Instance;
            mm.OnObjectiveActivated += OnActivated;
            mm.OnObjectiveCompleted += OnCompleted;
            mm.OnMissionStarted += _ => Refresh();
            Refresh();
        }

        void OnDestroy()
        {
            var mm = MissionManager.Instance;
            if (mm == null) return;
            mm.OnObjectiveActivated -= OnActivated;
            mm.OnObjectiveCompleted -= OnCompleted;
        }

        void Refresh()
        {
            var mm = MissionManager.Instance;
            bool active = mm.IsObjectiveActive(objectiveId);
            bool done = mm.IsObjectiveDone(objectiveId);
            switch (mode)
            {
                case Mode.ActiveWhileObjectiveActive: Set(active); break;
                case Mode.ActivateWhenStarted: Set(active || done); break;
                case Mode.ActivateWhenCompleted: Set(done); break;
                case Mode.DeactivateWhenCompleted: Set(!done); break;
            }
        }

        void OnActivated(Story.ObjectiveDefinition o)
        {
            if (o.id != objectiveId) return;
            Refresh();
            if (dialogue != null && (mode == Mode.ActivateWhenStarted || mode == Mode.ActiveWhileObjectiveActive)) DialogueRunner.Play(dialogue);
        }

        void OnCompleted(Story.ObjectiveDefinition o)
        {
            if (o.id != objectiveId) return;
            Refresh();
            if (dialogue != null && mode == Mode.ActivateWhenCompleted) DialogueRunner.Play(dialogue);
        }

        void Set(bool on)
        {
            foreach (var t in targets) if (t != null && t.activeSelf != on) t.SetActive(on);
        }
    }
}
