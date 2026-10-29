using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>A card on the suspect board that clues can be pinned to.</summary>
    [CreateAssetMenu(menuName = "Exit Clause/Suspect")]
    public class SuspectDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string theory;
        public Color cardColor = new Color(0.8f, 0.75f, 0.6f);
    }
}
