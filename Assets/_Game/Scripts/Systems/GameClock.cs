using System;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// In-game day counter and time of day. Civilian routines read the hour; Act 0 errands
    /// advance the day when the player sleeps at the mansion.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        public static GameClock Instance { get; private set; }

        [Tooltip("Real seconds per in-game hour.")]
        public float secondsPerHour = 60f;
        public float Hour = 8f;

        public event Action<int> OnDayChanged;

        public int Day => GameSession.Instance.State.progress.day;

        void Awake() { Instance = this; }

        void Update()
        {
            Hour += Time.deltaTime / Mathf.Max(1f, secondsPerHour);
            if (Hour >= 24f) Hour -= 24f;
        }

        public void ResetForSceneLoad() => OnDayChanged = null;

        public void AdvanceDay(float wakeHour = 8f)
        {
            GameSession.Instance.State.progress.day++;
            Hour = wakeHour;
            OnDayChanged?.Invoke(Day);
        }

        public void SetDay(int day)
        {
            GameSession.Instance.State.progress.day = day;
            OnDayChanged?.Invoke(day);
        }
    }
}
