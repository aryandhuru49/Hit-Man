using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>A branching decision. Each option records itself in StoryState and sets flags.</summary>
    [CreateAssetMenu(menuName = "Exit Clause/Choice")]
    public class ChoiceDefinition : ScriptableObject
    {
        public string id;
        public string prompt;
        [Tooltip("Seconds before the default option is picked. 0 = no timer.")]
        public float timeLimit;
        public int defaultOption;
        public List<ChoiceOption> options = new List<ChoiceOption>();
    }
}
