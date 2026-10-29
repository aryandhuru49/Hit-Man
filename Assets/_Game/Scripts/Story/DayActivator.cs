using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Enables objects only on certain in-game days (the tinted car on days 2-4, the new neighbour from day 3).</summary>
    public class DayActivator : MonoBehaviour
    {
        public int[] days;
        public GameObject[] targets;

        void Start()
        {
            GameClock.Instance.OnDayChanged += _ => Apply();
            Apply();
        }

        void Apply()
        {
            int d = GameSession.Instance.State.progress.day;
            bool on = System.Array.IndexOf(days, d) >= 0;
            foreach (var t in targets) if (t != null) t.SetActive(on);
        }
    }
}
