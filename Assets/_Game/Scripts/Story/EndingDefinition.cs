using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Text and requirements for one of the endings. EndingResolver decides which are available.</summary>
    [CreateAssetMenu(menuName = "Exit Clause/Ending")]
    public class EndingDefinition : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea(4, 12)] public string epilogue;
        [TextArea] public string lovedOneSavedLine;
        [TextArea] public string lovedOneLostLine;
        [TextArea] public string informantLine;
        public int minEvidence;
        public int minRageUses;
        public bool requiresNoSpare;
    }
}
