using System;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>A single subtitle line with an optional voice clip (placeholders until you record real ones).</summary>
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea] public string text;
        public float duration = 3f;
        public AudioClip voice;

        public DialogueLine() { }

        public DialogueLine(string speaker, string text, float duration = 3f)
        {
            this.speaker = speaker;
            this.text = text;
            this.duration = duration;
        }
    }
}
