using System;
using ExitClause.AI;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>
    /// Scene-wide alarm. Cameras, drones and discovered bodies raise it; every enemy agent then
    /// converges on the reported position with heightened perception for a while.
    /// </summary>
    public static class AlertManager
    {
        public static float AlarmUntil { get; private set; }
        public static bool AlarmActive => Time.time < AlarmUntil;
        public static event Action<Vector3> OnAlarm;

        public static void RaiseAlarm(Vector3 position, bool loud)
        {
            AlarmUntil = Time.time + 30f;
            foreach (var e in EnemyAI.All) e.RespondToAlarm(position);
            if (loud) NoiseSystem.Emit(position, 60f, NoiseType.Alarm, null);
            UIRoot.Instance?.HUD?.Notify(loud ? "ALARM RAISED" : "Guards are searching");
            OnAlarm?.Invoke(position);
        }

        /// <summary>Reset when a new scene loads.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            AlarmUntil = 0f;
            OnAlarm = null;
        }
    }
}
