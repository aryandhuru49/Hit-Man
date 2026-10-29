using ExitClause.Combat;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Advances an objective when this Health dies (targets, team members, drones). Optionally sets a flag.</summary>
    [RequireComponent(typeof(Health))]
    public class ObjectiveOnDeath : MonoBehaviour
    {
        public string objectiveId;
        public string setFlag;
        /// <summary>
        /// For story targets: a death cannot be undone, so if the target dies before its objective comes up,
        /// the objectives in between are completed and the mission carries on instead of soft-locking.
        /// </summary>
        public bool skipAheadIfEarly;

        void Start()
        {
            GetComponent<Health>().OnDied += _ =>
            {
                if (!string.IsNullOrEmpty(setFlag)) GameSession.Instance.Story.SetFlag(setFlag);
                var mm = MissionManager.Instance;
                if (skipAheadIfEarly && mm.Current != null && !mm.IsObjectiveDone(objectiveId) && mm.Current.objectives.Exists(x => x.id == objectiveId))
                    while (mm.CurrentObjective != null && mm.CurrentObjective.id != objectiveId) mm.ForceComplete(mm.CurrentObjective.id);
                mm.Progress(objectiveId);
            };
        }
    }
}
