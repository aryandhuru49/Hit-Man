using System;
using System.Collections.Generic;

namespace ExitClause.Story
{
    /// <summary>One step of a mission. Scene triggers advance it by id.</summary>
    [Serializable]
    public class ObjectiveDefinition
    {
        public string id;
        public string text;
        public int requiredCount = 1;
        public bool optional;
        public bool hidden;
        public int rewardCash;
        public List<string> setFlagsOnActivate = new List<string>();
        public List<string> setFlagsOnComplete = new List<string>();

        public ObjectiveDefinition() { }

        public ObjectiveDefinition(string id, string text, int count = 1, bool optional = false)
        {
            this.id = id;
            this.text = text;
            requiredCount = count;
            this.optional = optional;
        }
    }
}
