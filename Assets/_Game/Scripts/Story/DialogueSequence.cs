using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Ordered lines of dialogue used by cutscenes (Timeline subtitle clips) and in-game radio chatter.</summary>
    [CreateAssetMenu(menuName = "Exit Clause/Dialogue")]
    public class DialogueSequence : ScriptableObject
    {
        public string id;
        public List<DialogueLine> lines = new List<DialogueLine>();

        public float TotalDuration
        {
            get { float t = 0; foreach (var l in lines) t += l.duration; return t; }
        }
    }
}
