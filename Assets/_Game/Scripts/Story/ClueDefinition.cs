using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// An observation for the Suspicion Journal. Some clues are real (point at the agency),
    /// some are red herrings. trueSuspectId says which board suspect the clue really belongs to.
    /// </summary>
    [CreateAssetMenu(menuName = "Exit Clause/Clue")]
    public class ClueDefinition : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea] public string description;
        public bool isRedHerring;
        public string trueSuspectId;
        [Tooltip("How many sightings before the clue is logged.")]
        public int observationsRequired = 1;
        [Tooltip("Each sighting must be on a different in-game day (the tinted car).")]
        public bool requiresSeparateDays;
        [Tooltip("Added to the evidence counter that unlocks the Expose ending.")]
        public int evidenceValue;
        [Tooltip("Mission objective advanced when this clue is logged.")]
        public string objectiveId;
    }
}
