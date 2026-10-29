using System;
using System.Collections.Generic;

namespace ExitClause.Story
{
    /// <summary>One answer to a ChoiceDefinition, with its consequences.</summary>
    [Serializable]
    public class ChoiceOption
    {
        public string id;
        public string label;
        public string consequenceHint;
        public List<string> setFlags = new List<string>();
        public List<string> clearFlags = new List<string>();
        public string requiresFlag;
        public string requiresMissingFlag;
        public string progressObjective;

        public ChoiceOption() { }

        public ChoiceOption(string id, string label, string hint)
        {
            this.id = id;
            this.label = label;
            consequenceHint = hint;
        }
    }
}
