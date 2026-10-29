using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Shows objects only when the story state matches: the spared informant in Zurich, Luca helping
    /// in the Interlude, extra agency squads when Rage left too much evidence, the misdirected sniper.
    /// </summary>
    public class FlagActivator : MonoBehaviour
    {
        public string requiredFlag;
        public string forbiddenFlag;
        public string counterKey;
        public int counterAtLeast;
        public GameObject[] targets;
        public GameObject[] targetsWhenFalse;

        void Awake() => Evaluate();

        public void Evaluate()
        {
            var story = GameSession.Instance != null ? GameSession.Instance.Story : null;
            if (story == null) return;
            bool ok = true;
            if (!string.IsNullOrEmpty(requiredFlag)) ok &= story.HasFlag(requiredFlag);
            if (!string.IsNullOrEmpty(forbiddenFlag)) ok &= !story.HasFlag(forbiddenFlag);
            if (!string.IsNullOrEmpty(counterKey)) ok &= story.GetCounter(counterKey) >= counterAtLeast;
            foreach (var t in targets) if (t != null) t.SetActive(ok);
            foreach (var t in targetsWhenFalse) if (t != null) t.SetActive(!ok);
        }
    }
}
