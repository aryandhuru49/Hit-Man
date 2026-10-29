using System;
using System.Collections.Generic;

namespace ExitClause.Systems
{
    /// <summary>
    /// Branching story memory: boolean flags, integer counters and recorded choices.
    /// Stored as lists because JsonUtility cannot serialize dictionaries.
    /// </summary>
    [Serializable]
    public class StoryState
    {
        public List<string> flags = new List<string>();
        public List<StoryCounter> counters = new List<StoryCounter>();
        public List<ChoiceRecord> choices = new List<ChoiceRecord>();

        public bool HasFlag(string flag) => flags.Contains(flag);

        public void SetFlag(string flag, bool value = true)
        {
            if (value && !flags.Contains(flag)) flags.Add(flag);
            else if (!value) flags.Remove(flag);
        }

        public int GetCounter(string key)
        {
            var c = counters.Find(x => x.key == key);
            return c != null ? c.value : 0;
        }

        public void AddCounter(string key, int delta)
        {
            var c = counters.Find(x => x.key == key);
            if (c == null) counters.Add(new StoryCounter { key = key, value = delta });
            else c.value += delta;
        }

        public void SetCounter(string key, int value)
        {
            var c = counters.Find(x => x.key == key);
            if (c == null) counters.Add(new StoryCounter { key = key, value = value });
            else c.value = value;
        }

        public void RecordChoice(string choiceId, string optionId)
        {
            choices.RemoveAll(c => c.choiceId == choiceId);
            choices.Add(new ChoiceRecord { choiceId = choiceId, optionId = optionId });
        }

        public string GetChoice(string choiceId) => choices.Find(c => c.choiceId == choiceId)?.optionId;
    }

    [Serializable]
    public class StoryCounter
    {
        public string key;
        public int value;
    }

    [Serializable]
    public class ChoiceRecord
    {
        public string choiceId;
        public string optionId;
    }
}
